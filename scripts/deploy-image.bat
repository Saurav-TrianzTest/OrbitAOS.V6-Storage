@echo off
setlocal enabledelayedexpansion

echo ========================================
echo    AWS ECS Fargate Deployment Script
echo ========================================
echo.

REM Get AWS configuration
set /p "AWS_REGION=Enter AWS region (e.g., us-east-1): "
set /p "CLUSTER_NAME=Enter ECS cluster name (e.g., my-ecs-cluster): "
set /p "VPC_ID=Enter VPC ID (e.g., vpc-0abc123def456): "
set /p "SUBNET_INPUT=Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p "SECURITY_GROUP=Enter Security Group ID (e.g., sg-0abc123def): "
set /p "IMAGE_URI=Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/testsanity:latest): "

REM Parse subnets
for /f "tokens=1,2 delims=," %%a in ("!SUBNET_INPUT!") do (
    set "SUBNET_1=%%a"
    set "SUBNET_2=%%b"
)
if "!SUBNET_2!"=="" set "SUBNET_2=!SUBNET_1!"

echo.
echo Configuration:
echo   Region: !AWS_REGION!
echo   Cluster: !CLUSTER_NAME!
echo   VPC: !VPC_ID!
echo   Subnets: !SUBNET_1!, !SUBNET_2!
echo   Security Group: !SECURITY_GROUP!
echo   Image: !IMAGE_URI!
echo.

REM Get AWS Account ID
echo Retrieving AWS Account ID...
for /f "delims=" %%i in ('aws sts get-caller-identity --query Account --output text') do set "ACCOUNT_ID=%%i"
echo Account ID: !ACCOUNT_ID!
echo.

REM Check/create ECS cluster
echo Checking if ECS cluster exists...
aws ecs describe-clusters --clusters !CLUSTER_NAME! --region !AWS_REGION! >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Cluster does not exist. Creating cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name !CLUSTER_NAME! --region !AWS_REGION!
)
echo.

REM Load balancer configuration
set "TARGET_GROUP_ARN="
set /p "NEED_LB=Do you need a load balancer for this service? (y/n): "

if /i "!NEED_LB!"=="y" (
    echo Creating Application Load Balancer and Target Group...
    
    REM Create target group
    set "TG_NAME=testsanity-tg-!RANDOM!"
    for /f "delims=" %%i in ('aws elbv2 create-target-group --name !TG_NAME! --protocol HTTP --port 5000 --vpc-id !VPC_ID! --target-type ip --health-check-path /health --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text') do set "TARGET_GROUP_ARN=%%i"
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    
    REM Create load balancer
    set "ALB_NAME=testsanity-alb-!RANDOM!"
    for /f "delims=" %%i in ('aws elbv2 create-load-balancer --name !ALB_NAME! --subnets !SUBNET_1! !SUBNET_2! --security-groups !SECURITY_GROUP! --scheme internet-facing --type application --ip-address-type ipv4 --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text') do set "ALB_ARN=%%i"
    
    echo Load Balancer ARN: !ALB_ARN!
    
    REM Get load balancer DNS
    for /f "delims=" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns !ALB_ARN! --region !AWS_REGION! --query "LoadBalancers[0].DNSName" --output text') do set "ALB_DNS=%%i"
    
    REM Create listener
    aws elbv2 create-listener --load-balancer-arn !ALB_ARN! --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=!TARGET_GROUP_ARN! --region !AWS_REGION! >nul
    
    echo Load Balancer DNS: !ALB_DNS!
    echo.
)

REM Replace placeholders in task definition
echo Preparing task definition...
copy ecs\task-definition.json ecs\task-definition-resolved.json >nul
powershell -Command "(Get-Content ecs\task-definition-resolved.json) -replace '{{IMAGE_URI}}', '%IMAGE_URI%' -replace '{{AWS_REGION}}', '%AWS_REGION%' -replace '{{ACCOUNT_ID}}', '%ACCOUNT_ID%' | Set-Content ecs\task-definition-resolved.json"

REM Register task definition
echo Registering task definition...
for /f "delims=" %%i in ('aws ecs register-task-definition --cli-input-json file://ecs/task-definition-resolved.json --region !AWS_REGION! --query "taskDefinition.taskDefinitionArn" --output text') do set "TASK_DEF_ARN=%%i"

echo Task Definition ARN: !TASK_DEF_ARN!
echo.

REM Prepare service definition
echo Preparing service definition...
copy ecs\service-definition.json ecs\service-definition-resolved.json >nul
powershell -Command "(Get-Content ecs\service-definition-resolved.json) -replace '{{CLUSTER_NAME}}', '%CLUSTER_NAME%' -replace '{{SUBNET_1}}', '%SUBNET_1%' -replace '{{SUBNET_2}}', '%SUBNET_2%' -replace '{{SECURITY_GROUP}}', '%SECURITY_GROUP%' -replace '{{TARGET_GROUP_ARN}}', '%TARGET_GROUP_ARN%' | Set-Content ecs\service-definition-resolved.json"

REM Check if service exists
set "SERVICE_NAME=testsanity-service"
echo Checking if service exists...
for /f "delims=" %%i in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[?status==`ACTIVE`].serviceName" --output text 2^>nul') do set "EXISTING_SERVICE=%%i"

if "!EXISTING_SERVICE!"=="" (
    echo Creating new service: !SERVICE_NAME!
    aws ecs create-service --cli-input-json file://ecs/service-definition-resolved.json --region !AWS_REGION! >nul
) else (
    echo Updating existing service: !SERVICE_NAME!
    aws ecs update-service --cluster !CLUSTER_NAME! --service !SERVICE_NAME! --task-definition !TASK_DEF_ARN! --desired-count 2 --force-new-deployment --region !AWS_REGION! >nul
)

echo.
echo Waiting for service to become stable...
aws ecs wait services-stable --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!

echo.
echo ========================================
echo    Deployment Completed Successfully
echo ========================================
echo.

REM Display service details
aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].{Name:serviceName,Status:status,Running:runningCount,Desired:desiredCount}" --output table

echo.
if defined ALB_DNS (
    echo Application URL: http://!ALB_DNS!
    echo Health Check: http://!ALB_DNS!/health
)
echo CloudWatch Logs: /ecs/testsanity
echo.
echo Troubleshooting:
echo   - View logs: aws logs tail /ecs/testsanity --follow --region !AWS_REGION!
echo   - List tasks: aws ecs list-tasks --cluster !CLUSTER_NAME! --region !AWS_REGION!
echo   - Describe service: aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!
echo.

endlocal