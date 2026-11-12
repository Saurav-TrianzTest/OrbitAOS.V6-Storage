#!/bin/bash
set -e

echo "=== OrbitAOS.V6 Container Build and Push ==="
echo ""

# Project name (sanitized: lower-case, non-alphanumeric to hyphen, no leading/trailing hyphen)
PROJECT_NAME="OrbitAOS.V6"
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

# Ask for target platform
echo "Choose the target platform:"
echo "  1) AWS ECR"
echo "  2) Docker Hub"
echo ""
read -p "Enter choice (1 or 2): " PLATFORM_CHOICE

if [ "$PLATFORM_CHOICE" = "1" ]; then
    echo ""
    echo "=== AWS ECR ==="
    read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS account ID (e.g., 123456789012): " AWS_ACCOUNT_ID
    REGISTRY_URL="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    ECR_REPO="$IMAGE_NAME"
    FULL_IMAGE_NAME="${REGISTRY_URL}/${ECR_REPO}"

    read -p "Enter image tag (press Enter for 'latest'): " IMAGE_TAG
    if [ -z "$IMAGE_TAG" ]; then
        IMAGE_TAG="latest"
    fi

    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    if [ -z "$IMAGE_TAG" ]; then
        IMAGE_TAG="latest"
    fi

    FULL_IMAGE_NAME="${FULL_IMAGE_NAME}:${IMAGE_TAG}"

    echo ""
    echo "Logging in to AWS ECR..."
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"

    echo "Creating ECR repository if it does not exist..."
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || \
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"

    echo ""
    echo "Building Docker image..."
    docker build -f Dockerfile -t "$FULL_IMAGE_NAME" .

    echo ""
    echo "Pushing image to ECR..."
    docker push "$FULL_IMAGE_NAME"

    echo ""
    echo "=== Success ==="
    echo "Image pushed: $FULL_IMAGE_NAME"

elif [ "$PLATFORM_CHOICE" = "2" ]; then
    echo ""
    echo "=== Docker Hub ==="
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -s -p "Enter Docker Hub password (or access token): " DOCKER_PASSWORD
    echo ""

    read -p "Enter image tag (press Enter for 'latest'): " IMAGE_TAG
    if [ -z "$IMAGE_TAG" ]; then
        IMAGE_TAG="latest"
    fi

    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    if [ -z "$IMAGE_TAG" ]; then
        IMAGE_TAG="latest"
    fi

    FULL_IMAGE_NAME="${DOCKER_USERNAME}/${IMAGE_NAME}:${IMAGE_TAG}"

    echo ""
    echo "Logging in to Docker Hub..."
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin

    echo ""
    echo "Building Docker image..."
    docker build -f Dockerfile -t "$FULL_IMAGE_NAME" .

    echo ""
    echo "Pushing image to Docker Hub..."
    docker push "$FULL_IMAGE_NAME"

    echo ""
    echo "=== Success ==="
    echo "Image pushed: $FULL_IMAGE_NAME"

else
    echo "Invalid choice. Exiting."
    exit 1
fi
