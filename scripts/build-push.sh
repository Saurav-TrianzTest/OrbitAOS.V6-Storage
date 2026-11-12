#!/bin/bash
set -e

PROJECT_NAME="OrbitAOS.V6"
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo "=== Build and Push Script for $IMAGE_NAME ==="
echo ""

# Ask for target platform
echo "Select target platform:"
echo "1. AWS ECR"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " PLATFORM_CHOICE

if [ "$PLATFORM_CHOICE" = "1" ]; then
    # AWS ECR
    read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    REGISTRY_URL="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    ECR_REPO="$IMAGE_NAME"
    FULL_IMAGE_NAME="${REGISTRY_URL}/${ECR_REPO}"
elif [ "$PLATFORM_CHOICE" = "2" ]; then
    # Docker Hub
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    REGISTRY_URL="index.docker.io"
    FULL_IMAGE_NAME="${REGISTRY_URL}/${DOCKER_USERNAME}/${IMAGE_NAME}"
else
    echo "Invalid choice. Exiting."
    exit 1
fi

read -p "Enter image tag (or press Enter for 'latest'): " IMAGE_TAG
if [ -z "$IMAGE_TAG" ]; then
    IMAGE_TAG="latest"
fi

# Sanitize tag
IMAGE_TAG=$(echo "$IMAGE_TAG" | tr -cs 'a-z0-9-.' '-' | sed 's/^-*//;s/-*$//')
if [ -z "$IMAGE_TAG" ]; then
    IMAGE_TAG="latest"
fi

FULL_IMAGE_NAME="${FULL_IMAGE_NAME}:${IMAGE_TAG}"

echo ""
echo "=== Building image: $FULL_IMAGE_NAME ==="

if [ "$PLATFORM_CHOICE" = "1" ]; then
    # AWS ECR login
    echo "Logging in to AWS ECR..."
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"

    # Create repository if it doesn't exist
    echo "Checking if ECR repository exists..."
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || \
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
elif [ "$PLATFORM_CHOICE" = "2" ]; then
    # Docker Hub login
    read -s -p "Enter Docker Hub password: " DOCKER_PASSWORD
    echo ""
    echo "Logging in to Docker Hub..."
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
fi

echo "Building Docker image..."
docker build -f Dockerfile -t "$FULL_IMAGE_NAME" .

echo "Pushing image to registry..."
docker push "$FULL_IMAGE_NAME"

echo ""
echo "=== Success ==="
echo "Image pushed: $FULL_IMAGE_NAME"
