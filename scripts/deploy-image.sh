#!/bin/bash
set -e
set -o pipefail

echo "========================================"
echo "   AWS ECS Fargate Deployment Script"
echo "========================================"
echo ""

# Get AWS configuration
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter ECS cluster name (e.g., my-ecs-cluster): " CLUSTER_NAME
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNET_INPUT
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP
read -p "Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/testsanity:latest): " IMAGE_URI

# Convert comma-separated subnets to array
IFS=',' read -ra SUBNETS <<< "$SUBNET_INPUT"
SUBNET_1="${SUBNETS[0]}"
SUBNET_2="${SUBNETS[1]:-$SUBNET_1}"

echo ""
echo "Configuration:"
echo "  Region: $AWS_REGION"
echo "  Cluster: $CLUSTER_NAME"
echo "  VPC: $VPC_ID"
echo "  Subnets: $SUBNET_1, $SUBNET_2"
echo "  Security Group: $SECURITY_GROUP"
echo "  Image: $IMAGE_URI"
echo ""

# Get AWS Account ID
echo "Retrieving AWS Account ID..."
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo "Account ID: $ACCOUNT_ID"
echo ""

# Check/create ECS cluster
echo "Checking if ECS cluster exists..."
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo "Cluster does not exist. Creating cluster: $CLUSTER_NAME"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
}
echo ""

# Load balancer configuration
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB
TARGET_GROUP_ARN=""

if [ "$NEED_LB" = "y" ] || [ "$NEED_LB" = "Y" ]; then
    echo "Creating Application Load Balancer and Target Group..."
    
    # Create target group with target-type ip (required for Fargate awsvpc)
    TG_NAME="testsanity-tg-$(date +%s)"
    TARGET_GROUP_ARN=$(aws elbv2 create-target-group \
        --name "$TG_NAME" \
        --protocol HTTP \
        --port 5000 \
        --vpc-id "$VPC_ID" \
        --target-type ip \
        --health-check-path "/health" \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text)
    
    echo "Target Group ARN: $TARGET_GROUP_ARN"
    
    # Create load balancer
    ALB_NAME="testsanity-alb-$(date +%s)"
    ALB_ARN=$(aws elbv2 create-load-balancer \
        --name "$ALB_NAME" \
        --subnets "$SUBNET_1" "$SUBNET_2" \
        --security-groups "$SECURITY_GROUP" \
        --scheme internet-facing \
        --type application \
        --ip-address-type ipv4 \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text)
    
    echo "Load Balancer ARN: $ALB_ARN"
    
    # Get load balancer DNS
    ALB_DNS=$(aws elbv2 describe-load-balancers \
        --load-balancer-arns "$ALB_ARN" \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].DNSName' \
        --output text)
    
    # Create listener
    aws elbv2 create-listener \
        --load-balancer-arn "$ALB_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" >/dev/null
    
    echo "Load Balancer DNS: $ALB_DNS"
    echo ""
fi

# Replace placeholders in task definition
echo "Preparing task definition..."
cp ecs/task-definition.json ecs/task-definition-resolved.json
sed -i.bak "s|{{IMAGE_URI}}|$IMAGE_URI|g" ecs/task-definition-resolved.json
sed -i.bak "s|{{AWS_REGION}}|$AWS_REGION|g" ecs/task-definition-resolved.json
sed -i.bak "s|{{ACCOUNT_ID}}|$ACCOUNT_ID|g" ecs/task-definition-resolved.json
rm -f ecs/task-definition-resolved.json.bak

# Register task definition
echo "Registering task definition..."
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://ecs/task-definition-resolved.json \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

echo "Task Definition ARN: $TASK_DEF_ARN"
echo ""

# Prepare service definition
echo "Preparing service definition..."
cp ecs/service-definition.json ecs/service-definition-resolved.json
sed -i.bak "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" ecs/service-definition-resolved.json
sed -i.bak "s|{{SUBNET_1}}|$SUBNET_1|g" ecs/service-definition-resolved.json
sed -i.bak "s|{{SUBNET_2}}|$SUBNET_2|g" ecs/service-definition-resolved.json
sed -i.bak "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" ecs/service-definition-resolved.json

if [ -n "$TARGET_GROUP_ARN" ]; then
    sed -i.bak "s|{{TARGET_GROUP_ARN}}|$TARGET_GROUP_ARN|g" ecs/service-definition-resolved.json
else
    # Remove loadBalancers section if no load balancer
    sed -i.bak '/{TARGET_GROUP_ARN}/d' ecs/service-definition-resolved.json
fi

rm -f ecs/service-definition-resolved.json.bak

# Check if service exists
SERVICE_NAME="testsanity-service"
echo "Checking if service exists..."
EXISTING_SERVICE=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[?status==`ACTIVE`].serviceName' \
    --output text 2>/dev/null || echo "")

if [ -z "$EXISTING_SERVICE" ] || [ "$EXISTING_SERVICE" = "None" ]; then
    echo "Creating new service: $SERVICE_NAME"
    aws ecs create-service \
        --cli-input-json file://ecs/service-definition-resolved.json \
        --region "$AWS_REGION" >/dev/null
else
    echo "Updating existing service: $SERVICE_NAME"
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --desired-count 2 \
        --force-new-deployment \
        --region "$AWS_REGION" >/dev/null
fi

echo ""
echo "Waiting for service to become stable..."
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

echo ""
echo "========================================"
echo "   Deployment Completed Successfully"
echo "========================================"
echo ""

# Display service details
aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].{Name:serviceName,Status:status,Running:runningCount,Desired:desiredCount}' \
    --output table

echo ""
if [ -n "$ALB_DNS" ]; then
    echo "Application URL: http://$ALB_DNS"
    echo "Health Check: http://$ALB_DNS/health"
fi
echo "CloudWatch Logs: /ecs/testsanity"
echo ""
echo "Troubleshooting:"
echo "  - View logs: aws logs tail /ecs/testsanity --follow --region $AWS_REGION"
echo "  - List tasks: aws ecs list-tasks --cluster $CLUSTER_NAME --region $AWS_REGION"
echo "  - Describe service: aws ecs describe-services --cluster $CLUSTER_NAME --services $SERVICE_NAME --region $AWS_REGION"
echo ""