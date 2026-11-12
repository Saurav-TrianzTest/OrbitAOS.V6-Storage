#!/bin/bash
set -e
set -o pipefail

echo "=== OrbitAOS.V6 EKS Deployment ==="
echo ""

# Project name (sanitized: lower-case, non-alphanumeric to hyphen, no leading/trailing hyphen)
PROJECT_NAME="OrbitAOS.V6"
NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

# Ask for AWS region and cluster name
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter EKS cluster name: " CLUSTER_NAME
read -p "Enter full image URI (with registry and tag, e.g., 123456789012.dkr.ecr.us-east-1.amazonaws.com/orbitaos-v6:latest): " IMAGE_URI

echo ""
echo "Configuring kubectl for EKS cluster '$CLUSTER_NAME' in region '$AWS_REGION'..."
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"

echo "Verifying cluster connectivity..."
kubectl cluster-info || exit 1

# Ask for environment variables (non-credential)
echo ""
echo "Enter values for environment variables (or press Enter to skip):"
read -p "Enter value for AppUrls__Documentation (or press Enter to skip): " APPURLS_VALUE

# Ask for credentials (not echoed)
echo ""
echo "Enter credential values (not echoed):"
read -s -p "Enter value for ConnectionStrings__DefaultConnection (database connection string): " DB_CONN_VALUE
echo ""

# Create namespace
echo ""
echo "Applying namespace..."
kubectl apply -f kubernetes/namespace.yaml

# Create Secret with credentials
echo ""
echo "Creating Kubernetes Secret with credentials..."
if [ -n "$DB_CONN_VALUE" ]; then
    kubectl create secret generic ${NAME}-secrets -n ${NAME} \
        --from-literal=connection-string="$DB_CONN_VALUE" \
        --dry-run=client -o yaml | kubectl apply -f -
fi

# Apply deployment with placeholder replacements using sed -i.bak (macOS compatible)
echo ""
echo "Applying deployment..."
sed -i.bak 's|{{IMAGE_URI}}|'"$IMAGE_URI"'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{APPURLS__DOCUMENTATION}}|'"$APPURLS_VALUE"'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{NAMESPACE}}|'"$NAME"'|g' kubernetes/deployment.yaml
rm -f kubernetes/deployment.yaml.bak
kubectl apply -f kubernetes/deployment.yaml

echo "Applying service..."
kubectl apply -f kubernetes/service.yaml

echo "Applying ingress..."
kubectl apply -f kubernetes/ingress.yaml

# Wait for rollout
echo ""
echo "Waiting for deployment rollout..."
kubectl rollout status deployment/${NAME} -n ${NAME}

# Show status
echo ""
echo "=== Deployment Status ==="
kubectl get pods,svc,ingress -n ${NAME}

# Show application URL
echo ""
echo "=== Application URL ==="
echo "https://${NAME}.example.com"

echo ""
echo "=== Deployment Complete ==="
echo "If rollback is needed, run:"
echo "  kubectl rollout undo deployment/${NAME} -n ${NAME}"
