#!/bin/bash
set -e

echo "=== OrbitAOS.V6 Build and Push Script ==="
echo ""

# Project name
PROJECT_NAME="OrbitAOS.V6"
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo "Project name: $PROJECT_NAME"
echo "Image name: $IMAGE_NAME"
echo ""

# Prompt for target platform
echo "Choose target platform:"
echo "1. AWS ECR"
echo "2. Docker Hub"
echo ""
read -p "Enter choice (1 or 2): " PLATFORM_CHOICE

if [ "$PLATFORM_CHOICE" = "1" ]; then
    # AWS ECR
    read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
    read -p "Enter ECR repository name (or press Enter for default): " ECR_REPO_INPUT
    ECR_REPO=${ECR_REPO_INPUT:-$IMAGE_NAME}
    REGISTRY_URL="${AWS_REGION}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    FULL_IMAGE_NAME="${REGISTRY_URL}/${ECR_REPO}"

    read -p "Enter image tag (or press Enter for latest): " IMAGE_TAG
    if [ -z "$IMAGE_TAG" ]; then
        IMAGE_TAG="latest"
    fi

    echo ""
    echo "Building and pushing to AWS ECR..."
    echo "Registry: $REGISTRY_URL"
    echo "Repository: $ECR_REPO"
    echo "Full image name: $FULL_IMAGE_NAME:$IMAGE_TAG"
    echo ""

    # Login to ECR
    echo "Logging in to AWS ECR..."
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"

    # Create repository if it doesn't exist
    echo "Checking if ECR repository exists..."
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || \
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"

    # Build the image
    echo "Building Docker image..."
    docker build -f Dockerfile -t "$FULL_IMAGE_NAME:$IMAGE_TAG" .

    # Push the image
    echo "Pushing Docker image..."
    docker push "$FULL_IMAGE_NAME:$IMAGE_TAG"

    echo ""
    echo "=== Build and push complete ==="
    echo "Image: $FULL_IMAGE_NAME:$IMAGE_TAG"

elif [ "$PLATFORM_CHOICE" = "2" ]; then
    # Docker Hub
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -s -p "Enter Docker Hub password: " DOCKER_PASSWORD
    echo ""
    read -p "Enter Docker Hub repository name (or press Enter for default): " DOCKER_REPO_INPUT
    DOCKER_REPO=${DOCKER_REPO_INPUT:-$IMAGE_NAME}
    FULL_IMAGE_NAME="docker.io/${DOCKER_USERNAME}/${DOCKER_REPO}"

    read -p "Enter image tag (or press Enter for latest): " IMAGE_TAG
    if [ -z "$IMAGE_TAG" ]; then
        IMAGE_TAG="latest"
    fi

    echo ""
    echo "Building and pushing to Docker Hub..."
    echo "Repository: $FULL_IMAGE_NAME"
    echo "Tag: $IMAGE_TAG"
    echo ""

    # Login to Docker Hub
    echo "Logging in to Docker Hub..."
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin

    # Build the image
    echo "Building Docker image..."
    docker build -f Dockerfile -t "$FULL_IMAGE_NAME:$IMAGE_TAG" .

    # Push the image
    echo "Pushing Docker image..."
    docker push "$FULL_IMAGE_NAME:$IMAGE_TAG"

    echo ""
    echo "=== Build and push complete ==="
    echo "Image: $FULL_IMAGE_NAME:$IMAGE_TAG"

else
    echo "Invalid choice. Exiting."
    exit 1
fi
