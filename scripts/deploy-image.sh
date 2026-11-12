#!/bin/bash
set -e
set -o pipefail

NAMESPACE="orbitaaos-v6"
APP_NAME="orbitaaos-v6"

echo "=== Deploy $APP_NAME to AWS EKS ==="
echo ""

# Ask for AWS details
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter EKS cluster name: " CLUSTER_NAME
read -p "Enter full image URI (registry/repo:tag): " IMAGE_URI

echo ""
echo "=== Configuring kubectl for cluster $CLUSTER_NAME in $AWS_REGION ==="
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"

echo "Verifying cluster connectivity..."
kubectl cluster-info || exit 1

echo ""
echo "=== Collecting environment variable values ==="

# Collect non-credential environment variables
echo "Enter values for environment variables (or press Enter to skip):"
read -p "Enter value for ConnectionStrings__DefaultConnection (or press Enter to skip): " DB_CONN_VAL
read -p "Enter value for DocsUrl (or press Enter to skip): " DOCS_URL_VAL

echo ""
echo "=== Collecting credential values (not echoed) ==="
read -s -p "Enter value for DB_PASSWORD (or press Enter to skip): " DB_PASSWORD_VAL
echo ""

# Build secret literals
SECRET_ARGS=""
if [ -n "$DB_PASSWORD_VAL" ]; then
    SECRET_ARGS="$SECRET_ARGS --from-literal=DB_PASSWORD=$DB_PASSWORD_VAL"
fi

# Create or update the Kubernetes Secret
echo ""
echo "=== Creating Kubernetes Secret ==="
if [ -n "$SECRET_ARGS" ]; then
    kubectl create secret generic ${APP_NAME}-secrets -n ${NAMESPACE} $SECRET_ARGS --dry-run=client -o yaml | kubectl apply -f -
    echo "Secret ${APP_NAME}-secrets created/updated."
else
    echo "No credentials provided. Skipping secret creation."
fi

echo ""
echo "=== Applying Kubernetes manifests ==="

# Apply namespace
echo "Applying namespace..."
kubectl apply -f kubernetes/namespace.yaml

# Apply deployment with placeholder replacements
echo "Applying deployment..."
sed -i.bak "s|{{IMAGE_URI}}|$IMAGE_URI|g" kubernetes/deployment.yaml
sed -i.bak "s|{{NAMESPACE}}|$NAMESPACE|g" kubernetes/deployment.yaml
if [ -n "$DB_CONN_VAL" ]; then
    sed -i.bak "s|{{DB_CONNECTION_STRING}}|$DB_CONN_VAL|g" kubernetes/deployment.yaml
fi
if [ -n "$DOCS_URL_VAL" ]; then
    sed -i.bak "s|{{DOCS_URL}}|$DOCS_URL_VAL|g" kubernetes/deployment.yaml
fi
# Remove backup files created by sed -i.bak
rm -f kubernetes/deployment.yaml.bak

kubectl apply -f kubernetes/deployment.yaml

# Apply service
echo "Applying service..."
kubectl apply -f kubernetes/service.yaml

# Apply ingress
echo "Applying ingress..."
kubectl apply -f kubernetes/ingress.yaml

echo ""
echo "=== Waiting for deployment rollout ==="
kubectl rollout status deployment/${APP_NAME} -n ${NAMESPACE}

echo ""
echo "=== Deployment Status ==="
kubectl get pods,svc,ingress -n ${NAMESPACE}

echo ""
echo "=== Application URL ==="
INGRESS_HOST=$(kubectl get ingress ${APP_NAME}-ingress -n ${NAMESPACE} -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null || kubectl get ingress ${APP_NAME}-ingress -n ${NAMESPACE} -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")
echo "Application URL: http://${INGRESS_HOST}"

echo ""
echo "=== Deployment Complete ==="
echo "If the deployment fails, rollback with:"
echo "  kubectl rollout undo deployment/${APP_NAME} -n ${NAMESPACE}"
