#!/bin/bash
set -e
set -o pipefail

# build-push.sh — builds and pushes the OrbitContainer image to AWS ECR

PROJECT_NAME="OrbitContainer"
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo "=== OrbitContainer Image Build & Push ==="
echo ""

# Ask for the target platform
echo "Choose the target registry:"
echo "1. AWS ECR"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " REGISTRY_CHOICE

if [ "$REGISTRY_CHOICE" = "1" ]; then
    read -p "Enter AWS region (e.g. us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    REGISTRY_URL="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    ECR_REPO="$IMAGE_NAME"

    # Login to ECR
    echo "Logging in to AWS ECR..."
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"

    # Create repository if it doesn't exist
    echo "Checking if ECR repository exists..."
    if ! aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1; then
        echo "Creating ECR repository: $ECR_REPO"
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
    else
        echo "ECR repository already exists: $ECR_REPO"
    fi

    FULL_IMAGE_NAME="${REGISTRY_URL}/${ECR_REPO}"
elif [ "$REGISTRY_CHOICE" = "2" ]; then
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    echo "Logging in to Docker Hub..."
    docker login --username "$DOCKER_USERNAME"
    FULL_IMAGE_NAME="docker.io/${DOCKER_USERNAME}/${IMAGE_NAME}"
else
    echo "Invalid choice. Exiting."
    exit 1
fi

# Ask for the image tag
read -p "Enter IMAGE_TAG (leave empty for 'latest'): " IMAGE_TAG
if [ -z "$IMAGE_TAG" ]; then
    IMAGE_TAG="latest"
fi

# Sanitize the tag
IMAGE_TAG=$(echo "$IMAGE_TAG" | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')

FULL_IMAGE_NAME="${FULL_IMAGE_NAME}:${IMAGE_TAG}"

echo ""
echo "Building image: $FULL_IMAGE_NAME"
docker build -f Dockerfile -t "$FULL_IMAGE_NAME" .

echo ""
echo "Pushing image: $FULL_IMAGE_NAME"
docker push "$FULL_IMAGE_NAME"

echo ""
echo "=== Build and push complete ==="
echo "Image: $FULL_IMAGE_NAME"
