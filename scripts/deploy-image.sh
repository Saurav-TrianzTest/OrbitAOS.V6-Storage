#!/bin/bash
set -e
set -o pipefail

echo "========================================"
echo "AWS ECS Fargate Deployment Script"
echo "========================================"
echo ""

# Project configuration
PROJECT_NAME="orbitaos-v6"
TASK_FAMILY="${PROJECT_NAME}-task"
SERVICE_NAME="${PROJECT_NAME}-service"

# Prompt for deployment configuration
echo "=== AWS Configuration ==="
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter ECS cluster name (e.g., my-ecs-cluster): " CLUSTER_NAME

echo ""
echo "=== Network Configuration ==="
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNET_IDS
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP

# Convert comma-separated subnets to array
IFS=',' read -ra SUBNETS <<< "$SUBNET_IDS"
SUBNET_1="${SUBNETS[0]}"
SUBNET_2="${SUBNETS[1]:-$SUBNET_1}"

echo ""
echo "=== Container Configuration ==="
read -p "Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/orbitaos-v6:latest): " IMAGE_URI

echo ""
echo "=== Database Configuration ==="
read -p "Enter database server hostname: " DB_SERVER
read -p "Enter database name: " DB_NAME
read -p "Enter database user: " DB_USER
read -sp "Enter database password: " DB_PASSWORD
echo ""

echo ""
echo "=== Load Balancer Configuration ==="
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB

if [[ "$NEED_LB" =~ ^[Yy]$ ]]; then
    echo "Creating Application Load Balancer and Target Group..."
    
    # Create Target Group with ip target type (required for Fargate awsvpc)
    TG_NAME="${PROJECT_NAME}-tg"
    TARGET_GROUP_ARN=$(aws elbv2 create-target-group \
        --name "$TG_NAME" \
        --protocol HTTP \
        --port 80 \
        --vpc-id "$VPC_ID" \
        --target-type ip \
        --health-check-path "/health" \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text 2>/dev/null || aws elbv2 describe-target-groups \
        --names "$TG_NAME" \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text)
    
    echo "Target Group ARN: $TARGET_GROUP_ARN"
    
    # Create Application Load Balancer if needed
    ALB_NAME="${PROJECT_NAME}-alb"
    ALB_ARN=$(aws elbv2 describe-load-balancers \
        --names "$ALB_NAME" \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text 2>/dev/null || echo "")
    
    if [ -z "$ALB_ARN" ] || [ "$ALB_ARN" = "None" ]; then
        echo "Creating Application Load Balancer..."
        ALB_ARN=$(aws elbv2 create-load-balancer \
            --name "$ALB_NAME" \
            --subnets $SUBNET_1 $SUBNET_2 \
            --security-groups "$SECURITY_GROUP" \
            --scheme internet-facing \
            --type application \
            --ip-address-type ipv4 \
            --region "$AWS_REGION" \
            --query 'LoadBalancers[0].LoadBalancerArn' \
            --output text)
        
        echo "Load Balancer ARN: $ALB_ARN"
        
        # Create listener
        aws elbv2 create-listener \
            --load-balancer-arn "$ALB_ARN" \
            --protocol HTTP \
            --port 80 \
            --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
            --region "$AWS_REGION"
        
        echo "Waiting for load balancer to become active..."
        aws elbv2 wait load-balancer-available \
            --load-balancer-arns "$ALB_ARN" \
            --region "$AWS_REGION"
    fi
    
    # Get ALB DNS name
    ALB_DNS=$(aws elbv2 describe-load-balancers \
        --load-balancer-arns "$ALB_ARN" \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].DNSName' \
        --output text)
else
    TARGET_GROUP_ARN=""
    echo "Skipping load balancer creation"
fi

echo ""
echo "Getting AWS Account ID..."
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo "Account ID: $ACCOUNT_ID"

echo ""
echo "Checking if ECS cluster exists..."
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo "Cluster does not exist. Creating ECS cluster: $CLUSTER_NAME"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
}

echo ""
echo "Creating CloudWatch log group..."
aws logs create-log-group --log-group-name "/ecs/${PROJECT_NAME}" --region "$AWS_REGION" 2>/dev/null || echo "Log group already exists"

echo ""
echo "Preparing task definition..."

# Create temporary task definition with replaced values
TASK_DEF_FILE="ecs/task-definition.json"
TEMP_TASK_DEF="/tmp/task-definition-${PROJECT_NAME}.json"

cat "$TASK_DEF_FILE" | \
    sed "s|{{IMAGE_URI}}|${IMAGE_URI}|g" | \
    sed "s|{{AWS_REGION}}|${AWS_REGION}|g" | \
    sed "s|{{ACCOUNT_ID}}|${ACCOUNT_ID}|g" | \
    sed "s|{{DB_SERVER}}|${DB_SERVER}|g" | \
    sed "s|{{DB_NAME}}|${DB_NAME}|g" | \
    sed "s|{{DB_USER}}|${DB_USER}|g" | \
    sed "s|{{DB_PASSWORD}}|${DB_PASSWORD}|g" > "$TEMP_TASK_DEF"

echo "Registering task definition..."
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://"$TEMP_TASK_DEF" \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

if [ $? -ne 0 ]; then
    echo "ERROR: Failed to register task definition"
    exit 1
fi

echo "Task Definition ARN: $TASK_DEF_ARN"

echo ""
echo "Preparing service definition..."

# Create temporary service definition with replaced values
SERVICE_DEF_FILE="ecs/service-definition.json"
TEMP_SERVICE_DEF="/tmp/service-definition-${PROJECT_NAME}.json"

if [ -n "$TARGET_GROUP_ARN" ]; then
    # Include load balancer configuration
    cat "$SERVICE_DEF_FILE" | \
        sed "s|{{CLUSTER_NAME}}|${CLUSTER_NAME}|g" | \
        sed "s|{{SUBNET_1}}|${SUBNET_1}|g" | \
        sed "s|{{SUBNET_2}}|${SUBNET_2}|g" | \
        sed "s|{{SECURITY_GROUP}}|${SECURITY_GROUP}|g" | \
        sed "s|{{TARGET_GROUP_ARN}}|${TARGET_GROUP_ARN}|g" > "$TEMP_SERVICE_DEF"
else
    # Remove load balancer section
    cat "$SERVICE_DEF_FILE" | \
        sed "s|{{CLUSTER_NAME}}|${CLUSTER_NAME}|g" | \
        sed "s|{{SUBNET_1}}|${SUBNET_1}|g" | \
        sed "s|{{SUBNET_2}}|${SUBNET_2}|g" | \
        sed "s|{{SECURITY_GROUP}}|${SECURITY_GROUP}|g" | \
        jq 'del(.loadBalancers, .healthCheckGracePeriodSeconds)' > "$TEMP_SERVICE_DEF"
fi

echo "Checking if service exists..."
EXISTING_SERVICE=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].serviceName' \
    --output text 2>/dev/null)

if [ "$EXISTING_SERVICE" = "$SERVICE_NAME" ]; then
    echo "Service exists. Updating service..."
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --force-new-deployment \
        --region "$AWS_REGION"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to update service"
        exit 1
    fi
else
    echo "Service does not exist. Creating service..."
    aws ecs create-service \
        --cli-input-json file://"$TEMP_SERVICE_DEF" \
        --region "$AWS_REGION"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to create service"
        exit 1
    fi
fi

echo ""
echo "Waiting for service to become stable..."
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

echo ""
echo "========================================"
echo "Deployment Completed Successfully"
echo "========================================"
echo ""
echo "Service Details:"
aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].{ServiceName:serviceName,Status:status,RunningCount:runningCount,DesiredCount:desiredCount}' \
    --output table

if [ -n "$ALB_DNS" ]; then
    echo ""
    echo "Application URL: http://$ALB_DNS"
fi

echo ""
echo "CloudWatch Logs: /ecs/${PROJECT_NAME}"
echo ""
echo "To view logs:"
echo "aws logs tail /ecs/${PROJECT_NAME} --follow --region ${AWS_REGION}"
echo ""

# Cleanup temporary files
rm -f "$TEMP_TASK_DEF" "$TEMP_SERVICE_DEF"