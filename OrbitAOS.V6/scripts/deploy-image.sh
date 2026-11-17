#!/bin/bash
set -e
set -o pipefail

echo "=== OrbitAOS.V6 EKS Deployment Script ==="
echo ""

# Prompt for AWS details
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter EKS cluster name: " CLUSTER_NAME
read -p "Enter image URI (full path with registry and tag, e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/orbita-os-v6:latest): " IMAGE_URI

echo ""
echo "Configuring kubectl for EKS cluster: $CLUSTER_NAME in region $AWS_REGION..."
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"

echo "Checking cluster connectivity..."
kubectl cluster-info || exit 1

# Namespace
NAMESPACE="orbita-os-v6"

echo ""
echo "Applying namespace..."
kubectl apply -f kubernetes/namespace.yaml

# Prompt for environment variables
echo ""
echo "Enter environment variable values (press Enter to skip):"
read -p "Enter value for DB_HOST (or press Enter to skip): " DB_HOST_VAL
read -p "Enter value for DB_PORT (or press Enter to skip): " DB_PORT_VAL
read -p "Enter value for DB_NAME (or press Enter to skip): " DB_NAME_VAL
read -p "Enter value for DocsUrl (or press Enter to skip): " DOCS_URL_VAL

# Prompt for credentials (without echoing)
echo ""
echo "Enter credential values (not echoed):"
read -s -p "Enter value for ConnectionStrings__DefaultConnection: " CONNECTION_STRING_VAL
echo ""

# Create Secret
echo ""
echo "Creating Kubernetes Secret..."
SECRET_ARGS=""
if [ -n "$CONNECTION_STRING_VAL" ]; then
    SECRET_ARGS="--from-literal=connection-string=\"$CONNECTION_STRING_VAL\""
fi
if [ -n "$DB_HOST_VAL" ]; then
    if [ -n "$SECRET_ARGS" ]; then
        SECRET_ARGS="$SECRET_ARGS --from-literal=db-host=\"$DB_HOST_VAL\""
    else
        SECRET_ARGS="--from-literal=db-host=\"$DB_HOST_VAL\""
    fi
fi
if [ -n "$DB_PORT_VAL" ]; then
    if [ -n "$SECRET_ARGS" ]; then
        SECRET_ARGS="$SECRET_ARGS --from-literal=db-port=\"$DB_PORT_VAL\""
    else
        SECRET_ARGS="--from-literal=db-port=\"$DB_PORT_VAL\""
    fi
fi
if [ -n "$DB_NAME_VAL" ]; then
    if [ -n "$SECRET_ARGS" ]; then
        SECRET_ARGS="$SECRET_ARGS --from-literal=db-name=\"$DB_NAME_VAL\""
    else
        SECRET_ARGS="--from-literal=db-name=\"$DB_NAME_VAL\""
    fi
fi

if [ -n "$SECRET_ARGS" ]; then
    kubectl create secret generic orbita-os-v6-secrets -n "$NAMESPACE" $SECRET_ARGS --dry-run=client -o yaml | kubectl apply -f -
fi

# Replace placeholders in deployment.yaml
echo ""
echo "Deploying application..."
cp kubernetes/deployment.yaml /tmp/deployment.yaml.bak
sed -i.bak 's|{{IMAGE_URI}}|'"$IMAGE_URI"'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{DB_HOST}}|'"${DB_HOST_VAL:-}"'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{DB_PORT}}|'"${DB_PORT_VAL:-}"'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{DB_NAME}}|'"${DB_NAME_VAL:-}"'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{DOCS_URL}}|'"${DOCS_URL_VAL:-}"'|g' kubernetes/deployment.yaml
rm -f kubernetes/deployment.yaml.bak

# Apply deployment
kubectl apply -f kubernetes/deployment.yaml

# Apply service
kubectl apply -f kubernetes/service.yaml

# Apply ingress
kubectl apply -f kubernetes/ingress.yaml

# Wait for deployment
echo ""
echo "Waiting for deployment to roll out..."
kubectl rollout status deployment/orbita-os-v6 -n "$NAMESPACE"

# Show resources
echo ""
echo "=== Deployment Complete ==="
echo ""
echo "Pods:"
kubectl get pods -n "$NAMESPACE"
echo ""
echo "Services:"
kubectl get svc -n "$NAMESPACE"
echo ""
echo "Ingress:"
kubectl get ingress -n "$NAMESPACE"
echo ""
echo "Application URL: https://orbita-os-v6.example.com"
echo ""
echo "=== Rollback Instructions ==="
echo "If the deployment fails, run:"
echo "  kubectl rollout undo deployment/orbita-os-v6 -n $NAMESPACE"
echo ""
echo "To check logs:"
echo "  kubectl logs -f deployment/orbita-os-v6 -n $NAMESPACE"
