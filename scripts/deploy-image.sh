#!/bin/bash
set -e
set -o pipefail

# deploy-image.sh — deploys the OrbitContainer image to AWS EKS

PROJECT_NAME="OrbitContainer"
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')
NAMESPACE="$IMAGE_NAME"

echo "=== OrbitContainer Deployment to AWS EKS ==="
echo ""

# Ask for AWS region and EKS cluster name
read -p "Enter AWS region (e.g. us-east-1): " AWS_REGION
read -p "Enter EKS cluster name: " CLUSTER_NAME
read -p "Enter full image URI (e.g. 123456789.dkr.ecr.us-east-1.amazonaws.com/orbitcontainer:v1.0.0): " IMAGE_URI

echo ""
echo "Configuring kubectl for EKS cluster: $CLUSTER_NAME in $AWS_REGION"
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"

echo ""
echo "Verifying cluster connectivity..."
kubectl cluster-info || { echo "Cluster connection failed. Exiting."; exit 1; }

# Ask for environment variables (non-credential)
echo ""
echo "Enter values for non-credential environment variables (or press Enter to skip):"
read -p "Enter value for DocsUrl (or press Enter to skip): " DOCS_URL

# Ask for credentials
echo ""
echo "Enter values for credentials (not echoed):"
read -s -p "Enter ConnectionStrings__DefaultConnection (or press Enter to skip): " DB_CONNECTION
echo ""

# Apply namespace
echo ""
echo "Applying namespace..."
kubectl apply -f kubernetes/namespace.yaml

# Create Secret for credentials
echo ""
echo "Creating Secret for credentials..."
SECRET_ARGS=""
if [ -n "$DB_CONNECTION" ]; then
    SECRET_ARGS="$SECRET_ARGS --from-literal=ConnectionStrings__DefaultConnection=\"$DB_CONNECTION\""
fi

if [ -n "$SECRET_ARGS" ]; then
    eval kubectl create secret generic "${NAMESPACE}-secrets" -n "$NAMESPACE" $SECRET_ARGS --dry-run=client -o yaml | kubectl apply -f -
else
    echo "No credentials provided. Skipping Secret creation."
fi

# Prepare deployment.yaml with placeholders
echo ""
echo "Preparing deployment manifests..."
sed -i.bak 's|{{IMAGE_URI}}|'"$IMAGE_URI"'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{DOCS_URL}}|'"$DOCS_URL"'|g' kubernetes/deployment.yaml
sed -i.bak 's|{{NAMESPACE}}|'"$NAMESPACE"'|g' kubernetes/deployment.yaml

# Remove .bak files
rm -f kubernetes/deployment.yaml.bak

# Apply deployment, service, and ingress
echo ""
echo "Applying deployment..."
kubectl apply -f kubernetes/deployment.yaml

echo ""
echo "Applying service..."
kubectl apply -f kubernetes/service.yaml

echo ""
echo "Applying ingress..."
kubectl apply -f kubernetes/ingress.yaml

# Wait for rollout
echo ""
echo "Waiting for deployment rollout..."
kubectl rollout status deployment/"$NAMESPACE" -n "$NAMESPACE"

# Show resources
echo ""
echo "=== Deployed resources ==="
kubectl get pods,svc,ingress -n "$NAMESPACE"

# Show application URL
echo ""
echo "=== Application URL ==="
echo "https://${NAMESPACE}.example.com"

echo ""
echo "=== Deployment complete ==="
echo "To rollback: kubectl rollout undo deployment/${NAMESPACE} -n ${NAMESPACE}"
