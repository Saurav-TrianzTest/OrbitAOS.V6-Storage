#!/bin/bash
set -e
set -o pipefail

echo "=== OrbitContainerFix Deploy to AWS EKS ==="

# Determine project name
PROJECT_NAME="OrbitContainerFix"
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')
NAMESPACE="$IMAGE_NAME"

# Ask for AWS region and EKS cluster name
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter EKS cluster name: " CLUSTER_NAME
read -p "Enter the full image URI (registry/repo:tag): " IMAGE_URI

# Ask for environment variables
echo ""
echo "Enter values for environment variables (or press Enter to skip):"
read -p "Enter value for DB_CONNECTION_STRING (or press Enter to skip): " DB_CONNECTION_STRING
read -p "Enter value for DOCS_URL (or press Enter to skip): " DOCS_URL

# Create credentials secret
if [ -n "$DB_CONNECTION_STRING" ]; then
    echo "Creating Secret with DB connection string..."
    kubectl create secret generic "$NAMESPACE-secrets" -n "$NAMESPACE" \
        --from-literal=DB_CONNECTION_STRING="$DB_CONNECTION_STRING" \
        --dry-run=client -o yaml | kubectl apply -f -
else
    echo "No DB_CONNECTION_STRING provided. Creating empty Secret..."
    kubectl create secret generic "$NAMESPACE-secrets" -n "$NAMESPACE" \
        --from-literal=DB_CONNECTION_STRING="" \
        --dry-run=client -o yaml | kubectl apply -f -
fi

# Configure kubectl for the EKS cluster
echo "Configuring kubectl for EKS cluster: $CLUSTER_NAME in region $AWS_REGION"
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"

# Verify cluster connectivity
kubectl cluster-info || exit 1

# Apply namespace
echo "Applying namespace..."
kubectl apply -f kubernetes/namespace.yaml

# Replace placeholders in deployment.yaml
echo "Replacing placeholders in deployment.yaml..."
sed -i.bak 's|{{IMAGE_URI}}|'$IMAGE_URI'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{DOCS_URL}}|'"$DOCS_URL"'|g' kubernetes/deployment.yaml
rm -f kubernetes/deployment.yaml.bak

# Apply deployment
echo "Applying deployment..."
kubectl apply -f kubernetes/deployment.yaml

# Apply service
echo "Applying service..."
kubectl apply -f kubernetes/service.yaml

# Apply ingress
echo "Applying ingress..."
kubectl apply -f kubernetes/ingress.yaml

# Wait for deployment rollout
echo "Waiting for deployment rollout..."
kubectl rollout status deployment/"$IMAGE_NAME" -n "$NAMESPACE"

# Show resources
echo "Deployment complete. Current resources:"
kubectl get pods,svc,ingress -n "$NAMESPACE"

# Show application URL
echo ""
echo "Application URL: https://$NAMESPACE.example.com"
echo ""
echo "Rollback instructions:"
echo "  kubectl rollout undo deployment/$IMAGE_NAME -n $NAMESPACE"
echo "  kubectl rollout status deployment/$IMAGE_NAME -n $NAMESPACE"
