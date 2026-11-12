#!/bin/bash
set -e
set -o pipefail

PROJECT_NAME="OrbitAOS.V6"
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')
NAMESPACE="$IMAGE_NAME"

echo "=== Deploy to AWS EKS: $IMAGE_NAME ==="
echo ""

# Ask for AWS region and EKS cluster name
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter EKS cluster name: " CLUSTER_NAME

echo ""
echo "=== Configuring kubectl for EKS cluster: $CLUSTER_NAME ==="
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"

echo ""
echo "=== Verifying cluster connectivity ==="
kubectl cluster-info || exit 1

# Ask for the full image URI
read -p "Enter the full image URI (e.g., 123456789012.dkr.ecr.us-east-1.amazonaws.com/orbitaos-v6:latest): " IMAGE_URI

# Ask for environment variables (non-credential)
echo ""
echo "=== Enter environment variable values (press Enter to skip) ==="
read -p "Enter value for HOME__DOCUMENTATIONURL (or press Enter to skip): " HOME_DOC_URL

# Ask for credentials (not echoed)
echo ""
echo "=== Enter credential values (not echoed) ==="
read -s -p "Enter SQL Server connection string (or press Enter to skip): " DB_CONNECTION_STRING
echo ""

# Apply namespace first
echo ""
echo "=== Applying namespace ==="
kubectl apply -f kubernetes/namespace.yaml

# Create Secret for credentials
echo ""
echo "=== Creating Kubernetes Secret ==="
SECRET_ARGS=""
if [ -n "$DB_CONNECTION_STRING" ]; then
    SECRET_ARGS="$SECRET_ARGS --from-literal=connection-string=\"$DB_CONNECTION_STRING\""
fi

if [ -n "$SECRET_ARGS" ]; then
    eval kubectl create secret generic "$NAMESPACE-secrets" -n "$NAMESPACE" $SECRET_ARGS --dry-run=client -o yaml | kubectl apply -f -
else
    echo "No credentials provided. Skipping Secret creation."
fi

# Replace placeholders in deployment.yaml
echo ""
echo "=== Replacing placeholders in deployment.yaml ==="
cp kubernetes/deployment.yaml kubernetes/deployment.yaml.bak
sed -i.bak 's|{{IMAGE_URI}}|'"$IMAGE_URI"'|g' kubernetes/deployment.yaml
rm -f kubernetes/deployment.yaml.bak

if [ -n "$HOME_DOC_URL" ]; then
    sed -i.bak 's|{{HOME__DOCUMENTATIONURL}}|'"$HOME_DOC_URL"'|g' kubernetes/deployment.yaml
else
    sed -i.bak 's|{{HOME__DOCUMENTATIONURL}}|https://docs.microsoft.com/aspnet/core|g' kubernetes/deployment.yaml
fi
rm -f kubernetes/deployment.yaml.bak

# Apply deployment
echo ""
echo "=== Applying deployment ==="
kubectl apply -f kubernetes/deployment.yaml

# Apply service
echo ""
echo "=== Applying service ==="
kubectl apply -f kubernetes/service.yaml

# Apply ingress
echo ""
echo "=== Applying ingress ==="
kubectl apply -f kubernetes/ingress.yaml

# Wait for rollout
echo ""
echo "=== Waiting for deployment rollout ==="
kubectl rollout status deployment/"$NAMESPACE" -n "$NAMESPACE"

# Show status
echo ""
echo "=== Deployment status ==="
kubectl get pods,svc,ingress -n "$NAMESPACE"

# Show application URL
echo ""
echo "=== Application URL ==="
echo "https://$NAMESPACE.example.com"

echo ""
echo "=== Deployment complete ==="
echo "If you need to rollback, run:"
echo "  kubectl rollout undo deployment/$NAMESPACE -n $NAMESPACE"
