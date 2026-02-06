# OrbitAOS.V6 - AWS ECS Fargate Deployment Guide

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Local Development Setup](#local-development-setup)
4. [Docker Deployment](#docker-deployment)
5. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
6. [ECS Fargate Setup](#ecs-fargate-setup)
7. [ECS Task Definition Explained](#ecs-task-definition-explained)
8. [ECS Service Configuration](#ecs-service-configuration)
9. [Deployment Walkthrough](#deployment-walkthrough)
10. [Configuration Management](#configuration-management)
11. [Security Considerations](#security-considerations)
12. [Monitoring and Logging](#monitoring-and-logging)
13. [Troubleshooting](#troubleshooting)
14. [Scaling and Management](#scaling-and-management)

---

## Overview

This guide provides comprehensive instructions for containerizing and deploying the **OrbitAOS.V6** ASP.NET Core 6.0 application to AWS ECS Fargate.

**Application Details:**
- **Framework**: ASP.NET Core 6.0
- **Type**: Web Application with Identity and Entity Framework Core
- **Database**: SQL Server
- **Health Endpoints**: `/health`, `/ready`
- **Port**: 80 (HTTP)

---

## Prerequisites

### Required Tools

1. **.NET 6.0 SDK** - For local development
   ```bash
   dotnet --version  # Should show 6.0.x
   ```

2. **Docker Desktop** - For containerization
   ```bash
   docker --version
   docker-compose --version
   ```

3. **AWS CLI v2** - For ECS deployment
   ```bash
   aws --version  # Should show aws-cli/2.x.x
   ```

4. **Git** - For version control
   ```bash
   git --version
   ```

### AWS Account Requirements

- Active AWS account with appropriate permissions
- AWS CLI configured with credentials
- IAM permissions for:
  - ECS (tasks, services, clusters)
  - ECR (repository management, image push/pull)
  - CloudWatch Logs
  - VPC and networking resources
  - IAM role creation and management
  - Application Load Balancer (if using load balancer)

---

## Local Development Setup

### 1. Clone and Build

```bash
# Navigate to project directory
cd /path/to/OrbitAOS.V6

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run locally
dotnet run
```

### 2. Configure Database Connection

Update `appsettings.Development.json` with your local SQL Server connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrbitAOS;User Id=sa;Password=YourPassword;MultipleActiveResultSets=true"
  }
}
```

### 3. Run Database Migrations

```bash
dotnet ef database update
```

### 4. Test Health Endpoints

```bash
curl http://localhost:5000/health
curl http://localhost:5000/ready
```

---

## Docker Deployment

### Build and Run Locally

```bash
# Build Docker image
docker build -t orbitaos-v6:latest -f Dockerfile .

# Run container
docker run -d \
  -p 8080:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DB_SERVER=your-db-server \
  -e DB_NAME=OrbitAOS \
  -e DB_USER=your-db-user \
  -e DB_PASSWORD=your-db-password \
  --name orbitaos-v6 \
  orbitaos-v6:latest

# Check logs
docker logs -f orbitaos-v6

# Test application
curl http://localhost:8080/health
```

### Using Docker Compose

```bash
# Start application
docker-compose up -d

# View logs
docker-compose logs -f

# Stop application
docker-compose down
```

---

## AWS ECS Fargate Prerequisites

### 1. AWS CLI Configuration

```bash
# Configure AWS credentials
aws configure

# Verify configuration
aws sts get-caller-identity
```

### 2. VPC and Networking Setup

**Required Resources:**
- VPC with CIDR block (e.g., 10.0.0.0/16)
- At least 2 subnets in different availability zones
- Internet Gateway attached to VPC
- Route table with route to Internet Gateway
- Security Group with appropriate inbound/outbound rules

**Security Group Rules:**

**Inbound:**
- Port 80 (HTTP) from 0.0.0.0/0 (or your IP range)
- Port 443 (HTTPS) from 0.0.0.0/0 (optional)

**Outbound:**
- All traffic to 0.0.0.0/0 (for pulling images and database access)

### 3. IAM Roles Setup

**Task Execution Role** (ecsTaskExecutionRole):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ecs-tasks.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
```

Attach managed policies:
- `AmazonECSTaskExecutionRolePolicy`
- `CloudWatchLogsFullAccess`

**Task Role** (ecsTaskRole) - Optional:

For application-specific AWS service access (S3, DynamoDB, etc.).

### 4. CloudWatch Logs Setup

```bash
# Create log group
aws logs create-log-group --log-group-name /ecs/orbitaos-v6 --region us-east-1

# Set retention policy (optional)
aws logs put-retention-policy --log-group-name /ecs/orbitaos-v6 --retention-in-days 7 --region us-east-1
```

---

## ECS Fargate Setup

### 1. Create ECS Cluster

```bash
aws ecs create-cluster --cluster-name orbitaos-cluster --region us-east-1
```

### 2. Create ECR Repository

```bash
aws ecr create-repository --repository-name orbitaos-v6 --region us-east-1
```

---

## ECS Task Definition Explained

### Key Components

**1. Launch Type Configuration:**
```json
"requiresCompatibilities": ["FARGATE"],
"networkMode": "awsvpc"
```
- **FARGATE**: Serverless compute for containers
- **awsvpc**: Each task gets its own ENI with private IP

**2. CPU and Memory:**
```json
"cpu": "512",
"memory": "1024"
```

**Valid Fargate Combinations:**
- CPU: 256 (.25 vCPU) → Memory: 512, 1024, 2048 MB
- CPU: 512 (.5 vCPU) → Memory: 1024, 2048, 3072, 4096 MB
- CPU: 1024 (1 vCPU) → Memory: 2048-8192 MB
- CPU: 2048 (2 vCPU) → Memory: 4096-16384 MB
- CPU: 4096 (4 vCPU) → Memory: 8192-30720 MB

**3. Container Definition:**
```json
{
  "name": "orbitaos-v6",
  "image": "123456789.dkr.ecr.us-east-1.amazonaws.com/orbitaos-v6:latest",
  "essential": true,
  "portMappings": [{"containerPort": 80, "protocol": "tcp"}],
  "environment": [...],
  "logConfiguration": {...}
}
```

**4. Logging:**
```json
"logConfiguration": {
  "logDriver": "awslogs",
  "options": {
    "awslogs-group": "/ecs/orbitaos-v6",
    "awslogs-region": "us-east-1",
    "awslogs-stream-prefix": "ecs"
  }
}
```

---

## ECS Service Configuration

### Key Components

**1. Service Basics:**
```json
{
  "serviceName": "orbitaos-v6-service",
  "taskDefinition": "orbitaos-v6-task",
  "desiredCount": 2,
  "launchType": "FARGATE"
}
```

**2. Network Configuration:**
```json
"networkConfiguration": {
  "awsvpcConfiguration": {
    "subnets": ["subnet-xxx", "subnet-yyy"],
    "securityGroups": ["sg-xxx"],
    "assignPublicIp": "ENABLED"
  }
}
```

**3. Deployment Configuration:**
```json
"deploymentConfiguration": {
  "maximumPercent": 200,
  "minimumHealthyPercent": 50
}
```
- Allows rolling deployments with zero downtime
- Can run up to 4 tasks (2 × 200%) during deployment
- Maintains at least 1 task (2 × 50%) during deployment

**4. Load Balancer Integration:**
```json
"loadBalancers": [{
  "targetGroupArn": "arn:aws:elasticloadbalancing:...",
  "containerName": "orbitaos-v6",
  "containerPort": 80
}],
"healthCheckGracePeriodSeconds": 300
```

---

## Deployment Walkthrough

### Step 1: Build and Push Docker Image

**Linux/macOS:**
```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

**Windows:**
```cmd
scripts\build-push.bat
```

**What the script does:**
1. Prompts for registry selection (ECR or Docker Hub)
2. Collects registry credentials
3. Authenticates with the registry
4. Creates ECR repository if it doesn't exist
5. Builds Docker image with proper tagging
6. Pushes image to registry

### Step 2: Deploy to ECS Fargate

**Linux/macOS:**
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

**Windows:**
```cmd
scripts\deploy-image.bat
```

**What the script does:**
1. Collects deployment configuration (region, cluster, network, etc.)
2. Prompts for database connection details
3. Asks about load balancer requirement
4. Creates/updates ALB and target group if needed
5. Registers ECS task definition
6. Creates or updates ECS service
7. Waits for service stability
8. Displays deployment status and access URLs

### Step 3: Verify Deployment

```bash
# Check service status
aws ecs describe-services \
  --cluster orbitaos-cluster \
  --services orbitaos-v6-service \
  --region us-east-1

# View running tasks
aws ecs list-tasks \
  --cluster orbitaos-cluster \
  --service-name orbitaos-v6-service \
  --region us-east-1

# Check task health
aws ecs describe-tasks \
  --cluster orbitaos-cluster \
  --tasks <task-arn> \
  --region us-east-1
```

### Step 4: Test Application

```bash
# If using load balancer
curl http://<alb-dns-name>/health

# Check application logs
aws logs tail /ecs/orbitaos-v6 --follow --region us-east-1
```

---

## Configuration Management

### Environment Variables

Set in task definition:
- `ASPNETCORE_ENVIRONMENT`: Production/Staging/Development
- `ASPNETCORE_URLS`: http://+:80
- `DB_SERVER`: Database server hostname
- `DB_NAME`: Database name
- `DB_USER`: Database user
- `DB_PASSWORD`: Database password (use AWS Secrets Manager for production)

### Using AWS Secrets Manager (Recommended)

Replace environment variables with secrets:

```json
"secrets": [
  {
    "name": "DB_PASSWORD",
    "valueFrom": "arn:aws:secretsmanager:region:account-id:secret:db-password"
  }
]
```

### Application Settings Override

Mount configuration files using EFS or use environment variables:

```bash
# Environment variable pattern matching
ConnectionStrings__DefaultConnection="Server=...;Database=...;"
```

---

## Security Considerations

### 1. Network Security

- Use private subnets with NAT Gateway for production
- Restrict security group rules to necessary ports only
- Enable VPC Flow Logs for network monitoring

### 2. Secrets Management

- **NEVER** hardcode passwords in task definitions
- Use AWS Secrets Manager or Systems Manager Parameter Store
- Rotate secrets regularly
- Enable encryption at rest

### 3. IAM Best Practices

- Use least privilege principle for task roles
- Separate execution role from task role
- Enable CloudTrail for audit logging
- Use IAM policy conditions for additional restrictions

### 4. Container Security

- Use non-root user in containers (implemented in Dockerfile)
- Scan images for vulnerabilities using ECR image scanning
- Keep base images updated
- Minimize image size and attack surface

### 5. Application Security

- Enable HTTPS in production (use ALB with SSL certificate)
- Configure CORS policies appropriately
- Implement rate limiting
- Enable ASP.NET Core Data Protection
- Use secure authentication tokens

---

## Monitoring and Logging

### CloudWatch Logs

```bash
# View logs in real-time
aws logs tail /ecs/orbitaos-v6 --follow --region us-east-1

# Search logs
aws logs filter-log-events \
  --log-group-name /ecs/orbitaos-v6 \
  --filter-pattern "ERROR" \
  --region us-east-1

# Create metric filter
aws logs put-metric-filter \
  --log-group-name /ecs/orbitaos-v6 \
  --filter-name ErrorCount \
  --filter-pattern "[time, request_id, level = ERROR, msg]" \
  --metric-transformations \
    metricName=ErrorCount,metricNamespace=OrbitAOS,metricValue=1
```

### CloudWatch Alarms

```bash
# CPU utilization alarm
aws cloudwatch put-metric-alarm \
  --alarm-name orbitaos-high-cpu \
  --alarm-description "Alert when CPU exceeds 80%" \
  --metric-name CPUUtilization \
  --namespace AWS/ECS \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2
```

### Application Insights

Consider adding Application Insights SDK:

```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.21.0" />
```

---

## Troubleshooting

### Common Issues

#### 1. Task Fails to Start

**Symptoms**: Tasks immediately stop after starting

**Solutions**:
```bash
# Check stopped task reason
aws ecs describe-tasks --cluster orbitaos-cluster --tasks <task-id>

# Common causes:
# - Invalid CPU/memory combination
# - Image pull errors (check ECR permissions)
# - Health check failures
# - Application crashes (check CloudWatch logs)
```

#### 2. Cannot Pull Image from ECR

**Symptoms**: "CannotPullContainerError"

**Solutions**:
- Verify task execution role has ECR permissions
- Check ECR repository policy
- Ensure image exists and tag is correct
- Verify region matches

```bash
# Test ECR access
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com
```

#### 3. Database Connection Failures

**Symptoms**: Application crashes with SQL connection errors

**Solutions**:
- Verify database security group allows inbound from ECS security group
- Check database endpoint is accessible from VPC
- Verify connection string environment variables
- Test database connectivity from task

```bash
# Execute command in running task
aws ecs execute-command \
  --cluster orbitaos-cluster \
  --task <task-id> \
  --container orbitaos-v6 \
  --interactive \
  --command "/bin/bash"
```

#### 4. Load Balancer Health Check Failures

**Symptoms**: Tasks continuously restart, unhealthy target

**Solutions**:
- Verify health endpoint returns 200 OK
- Check health check path in target group
- Increase health check grace period
- Verify security group allows ALB → ECS traffic

```bash
# Check target health
aws elbv2 describe-target-health \
  --target-group-arn <target-group-arn>
```

#### 5. High Memory Usage

**Symptoms**: Tasks OOM killed, "OutOfMemory" errors

**Solutions**:
- Increase task memory allocation
- Profile application memory usage
- Check for memory leaks
- Optimize Entity Framework queries

---

## Scaling and Management

### Manual Scaling

```bash
# Scale service
aws ecs update-service \
  --cluster orbitaos-cluster \
  --service orbitaos-v6-service \
  --desired-count 4 \
  --region us-east-1
```

### Auto Scaling

```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/orbitaos-cluster/orbitaos-v6-service \
  --min-capacity 2 \
  --max-capacity 10

# Create scaling policy (CPU-based)
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/orbitaos-cluster/orbitaos-v6-service \
  --policy-name cpu-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json
```

**scaling-policy.json:**
```json
{
  "TargetValue": 70.0,
  "PredefinedMetricSpecification": {
    "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
  },
  "ScaleOutCooldown": 60,
  "ScaleInCooldown": 300
}
```

### Blue/Green Deployments

Use AWS CodeDeploy for blue/green deployments:

1. Create CodeDeploy application
2. Configure deployment group with ECS service
3. Create AppSpec file
4. Deploy new task definition revision

### Rolling Updates

```bash
# Force new deployment
aws ecs update-service \
  --cluster orbitaos-cluster \
  --service orbitaos-v6-service \
  --force-new-deployment \
  --region us-east-1
```

---

## Additional Resources

- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [AWS Fargate Pricing](https://aws.amazon.com/fargate/pricing/)

---

## Support

For issues or questions:
1. Check CloudWatch logs: `/ecs/orbitaos-v6`
2. Review ECS service events
3. Consult AWS support
4. Review application-specific logs

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-06  
**Target Platform**: AWS ECS Fargate  
**Application**: OrbitAOS.V6 (.NET 6.0)