# TestSanity - AWS ECS Fargate Deployment Guide

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Local Development](#local-development)
4. [Docker Build and Push](#docker-build-and-push)
5. [AWS ECS Fargate Setup](#aws-ecs-fargate-setup)
6. [Deployment Process](#deployment-process)
7. [Configuration Management](#configuration-management)
8. [Monitoring and Logging](#monitoring-and-logging)
9. [Troubleshooting](#troubleshooting)
10. [Security Best Practices](#security-best-practices)
11. [Scaling and Performance](#scaling-and-performance)

---

## Overview

This guide provides comprehensive instructions for deploying the TestSanity .NET 8.0 ASP.NET Core application to AWS ECS Fargate. The application is containerized using Docker and deployed using AWS ECS with Fargate launch type for serverless container management.

**Technology Stack:**
- .NET 8.0
- ASP.NET Core Web API
- Docker
- AWS ECS Fargate
- AWS ECR (Elastic Container Registry)
- Application Load Balancer (optional)
- CloudWatch Logs

---

## Prerequisites

### Required Software

1. **Docker Desktop**
   - Version 20.10.0 or higher
   - Download: https://www.docker.com/products/docker-desktop

2. **AWS CLI**
   - Version 2.x or higher
   - Installation: `curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" && unzip awscliv2.zip && sudo ./aws/install`
   - Configure: `aws configure`

3. **.NET SDK** (for local development)
   - Version 8.0 or higher
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0

### AWS Account Requirements

1. **AWS Account** with appropriate permissions
2. **IAM User** with the following permissions:
   - ECR: Full access for image management
   - ECS: Full access for cluster, service, and task management
   - EC2: VPC, subnet, and security group management
   - ELB: Load balancer and target group management
   - CloudWatch: Logs and metrics access
   - IAM: PassRole permission for ECS task execution role

3. **AWS Resources:**
   - VPC with at least 2 subnets in different availability zones
   - Security group allowing inbound traffic on port 5000
   - Internet Gateway attached to VPC for public access

### IAM Roles

Create the following IAM roles:

1. **ecsTaskExecutionRole** (required):
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
   Attach managed policy: `AmazonECSTaskExecutionRolePolicy`

2. **ecsTaskRole** (optional, for application permissions):
   - Attach policies based on your application's AWS service requirements
   - Example: S3 access, DynamoDB access, etc.

---

## Local Development

### Building the Application Locally

```bash
# Restore dependencies
dotnet restore

# Build the application
dotnet build -c Release

# Run the application
dotnet run --urls=http://localhost:5000

# Test the health endpoint
curl http://localhost:5000/health
```

### Running with Docker Compose

```bash
# Build and start the application
docker-compose up --build

# Access the application
curl http://localhost:5000/health

# Stop the application
docker-compose down
```

### Environment Variables

Create a `.env` file for local development:

```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
DATABASE_CONNECTION=Server=db-host;Database=mydb;User=user;Password=pass
REDIS_CONNECTION=redis-host:6379
KAFKA_BOOTSTRAP_SERVERS=kafka-host:9092
```

---

## Docker Build and Push

### Using AWS ECR

#### Linux/macOS:

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### Windows:

```batch
scripts\build-push.bat
```

### Manual Build and Push

```bash
# Build the Docker image
docker build -t testsanity:latest .

# Tag for ECR
AWS_REGION=us-east-1
AWS_ACCOUNT_ID=123456789012
REPO_NAME=testsanity

docker tag testsanity:latest $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPO_NAME:latest

# Login to ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Create ECR repository (if not exists)
aws ecr create-repository --repository-name $REPO_NAME --region $AWS_REGION

# Push image
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPO_NAME:latest
```

---

## AWS ECS Fargate Setup

### Network Configuration

1. **VPC Setup:**
   ```bash
   # Create VPC
   aws ec2 create-vpc --cidr-block 10.0.0.0/16 --region us-east-1
   
   # Create subnets in different AZs
   aws ec2 create-subnet --vpc-id vpc-xxxxx --cidr-block 10.0.1.0/24 --availability-zone us-east-1a
   aws ec2 create-subnet --vpc-id vpc-xxxxx --cidr-block 10.0.2.0/24 --availability-zone us-east-1b
   
   # Create and attach Internet Gateway
   aws ec2 create-internet-gateway
   aws ec2 attach-internet-gateway --vpc-id vpc-xxxxx --internet-gateway-id igw-xxxxx
   ```

2. **Security Group:**
   ```bash
   # Create security group
   aws ec2 create-security-group --group-name testsanity-sg --description "Security group for TestSanity" --vpc-id vpc-xxxxx
   
   # Allow inbound traffic on port 5000
   aws ec2 authorize-security-group-ingress --group-id sg-xxxxx --protocol tcp --port 5000 --cidr 0.0.0.0/0
   
   # Allow outbound traffic
   aws ec2 authorize-security-group-egress --group-id sg-xxxxx --protocol -1 --cidr 0.0.0.0/0
   ```

### ECS Cluster Setup

```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name testsanity-cluster --region us-east-1

# Verify cluster creation
aws ecs describe-clusters --clusters testsanity-cluster --region us-east-1
```

### CloudWatch Log Group

```bash
# Create log group
aws logs create-log-group --log-group-name /ecs/testsanity --region us-east-1

# Set retention policy (optional)
aws logs put-retention-policy --log-group-name /ecs/testsanity --retention-in-days 7 --region us-east-1
```

---

## Deployment Process

### Using Deployment Scripts

#### Linux/macOS:

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows:

```batch
scripts\deploy-image.bat
```

### Manual Deployment

1. **Register Task Definition:**

   Update `ecs/task-definition.json` with your values:
   - Replace `{{IMAGE_URI}}` with your ECR image URI
   - Replace `{{AWS_REGION}}` with your AWS region
   - Replace `{{ACCOUNT_ID}}` with your AWS account ID

   ```bash
   aws ecs register-task-definition --cli-input-json file://ecs/task-definition.json --region us-east-1
   ```

2. **Create or Update Service:**

   Update `ecs/service-definition.json` with your values:
   - Replace `{{CLUSTER_NAME}}` with your cluster name
   - Replace `{{SUBNET_1}}` and `{{SUBNET_2}}` with your subnet IDs
   - Replace `{{SECURITY_GROUP}}` with your security group ID
   - Replace `{{TARGET_GROUP_ARN}}` with your target group ARN (if using load balancer)

   ```bash
   # Create new service
   aws ecs create-service --cli-input-json file://ecs/service-definition.json --region us-east-1
   
   # Or update existing service
   aws ecs update-service --cluster testsanity-cluster --service testsanity-service --task-definition testsanity-task --force-new-deployment --region us-east-1
   ```

3. **Wait for Service Stability:**

   ```bash
   aws ecs wait services-stable --cluster testsanity-cluster --services testsanity-service --region us-east-1
   ```

### Verify Deployment

```bash
# Check service status
aws ecs describe-services --cluster testsanity-cluster --services testsanity-service --region us-east-1

# List running tasks
aws ecs list-tasks --cluster testsanity-cluster --service-name testsanity-service --region us-east-1

# Get task details
aws ecs describe-tasks --cluster testsanity-cluster --tasks <task-id> --region us-east-1
```

---

## Configuration Management

### Environment-Specific Settings

For production deployments, use AWS Systems Manager Parameter Store or Secrets Manager:

```bash
# Store secret in Secrets Manager
aws secretsmanager create-secret --name testsanity/database-connection --secret-string "Server=prod-db;Database=prod;User=app;Password=xxx"

# Store parameter in Parameter Store
aws ssm put-parameter --name /testsanity/redis-connection --value "redis.prod:6379" --type String
```

Update task definition to reference secrets:

```json
"secrets": [
  {
    "name": "DATABASE_CONNECTION",
    "valueFrom": "arn:aws:secretsmanager:region:account-id:secret:testsanity/database-connection"
  }
]
```

### Application Settings

Update `appsettings.Production.json` for production-specific configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://+:5000"
      }
    }
  }
}
```

---

## Monitoring and Logging

### CloudWatch Logs

```bash
# View logs in real-time
aws logs tail /ecs/testsanity --follow --region us-east-1

# Filter logs
aws logs filter-log-events --log-group-name /ecs/testsanity --filter-pattern "ERROR" --region us-east-1

# Get log streams
aws logs describe-log-streams --log-group-name /ecs/testsanity --region us-east-1
```

### CloudWatch Metrics

Key metrics to monitor:
- `CPUUtilization`: Task CPU usage
- `MemoryUtilization`: Task memory usage
- `TargetResponseTime`: Application response time (with ALB)
- `HealthyHostCount`: Number of healthy targets
- `UnHealthyHostCount`: Number of unhealthy targets

### Application Insights (Optional)

Integrate Application Insights for .NET monitoring:

```csharp
// Add to Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

---

## Troubleshooting

### Common Issues and Solutions

#### 1. Task Fails to Start

**Symptoms:** Tasks transition from PENDING to STOPPED immediately.

**Diagnosis:**
```bash
# Get stopped task details
aws ecs describe-tasks --cluster testsanity-cluster --tasks <task-id> --region us-east-1
```

**Common Causes:**
- Invalid CPU/memory combination (must be valid Fargate values)
- Image pull errors (check ECR permissions)
- Container health check failures
- Insufficient ENI capacity in subnet

**Solutions:**
- Verify task definition uses valid CPU/memory (e.g., cpu: "512", memory: "1024")
- Ensure task execution role has ECR pull permissions
- Check CloudWatch logs for application errors
- Verify subnets have available IP addresses

#### 2. Service Fails Health Checks

**Symptoms:** Tasks start but are marked unhealthy by load balancer.

**Diagnosis:**
```bash
# Check target health
aws elbv2 describe-target-health --target-group-arn <target-group-arn> --region us-east-1
```

**Solutions:**
- Verify health check endpoint returns 200 OK: `curl http://<task-ip>:5000/health`
- Increase `healthCheckGracePeriodSeconds` in service definition
- Check security group allows traffic from load balancer
- Review application logs for startup errors

#### 3. Cannot Pull Image from ECR

**Symptoms:** Error "CannotPullContainerError" in task events.

**Solutions:**
```bash
# Verify image exists
aws ecr describe-images --repository-name testsanity --region us-east-1

# Check task execution role policy
aws iam get-role-policy --role-name ecsTaskExecutionRole --policy-name ECSTaskExecutionRolePolicy

# Verify ECR permissions
aws ecr get-repository-policy --repository-name testsanity --region us-east-1
```

#### 4. Out of Memory Errors

**Symptoms:** Tasks stop with exit code 137 or OOM errors in logs.

**Solutions:**
- Increase memory in task definition (e.g., "1024" -> "2048")
- Optimize .NET application memory usage
- Configure GC settings for containerized environments
- Monitor memory metrics in CloudWatch

#### 5. High CPU Usage

**Symptoms:** CPU utilization consistently above 80%.

**Solutions:**
- Increase CPU in task definition
- Profile application for performance bottlenecks
- Enable ReadyToRun compilation for faster startup
- Scale out by increasing desired count

---

## Security Best Practices

### Container Security

1. **Use Non-Root User:**
   - Dockerfile already implements non-root user
   - Verify with: `docker run --rm testsanity id`

2. **Minimal Base Images:**
   - Use official Microsoft ASP.NET runtime images
   - Keep images updated regularly

3. **Scan Images for Vulnerabilities:**
   ```bash
   # Enable ECR image scanning
   aws ecr put-image-scanning-configuration --repository-name testsanity --image-scanning-configuration scanOnPush=true
   
   # View scan results
   aws ecr describe-image-scan-findings --repository-name testsanity --image-id imageTag=latest
   ```

### Network Security

1. **Security Groups:**
   - Restrict inbound traffic to specific sources
   - Use separate security groups for tasks and load balancers

2. **Private Subnets (Recommended):**
   - Place tasks in private subnets
   - Use NAT Gateway for outbound internet access
   - Only expose load balancer in public subnets

### Secrets Management

1. **Never Hardcode Secrets:**
   - Use AWS Secrets Manager or Parameter Store
   - Reference secrets in task definition

2. **Rotate Credentials:**
   - Enable automatic rotation for database passwords
   - Update task definition after rotation

### IAM Best Practices

1. **Principle of Least Privilege:**
   - Grant only necessary permissions to task roles
   - Use separate roles for different services

2. **Service-Linked Roles:**
   - Use AWS-managed policies where possible
   - Avoid wildcard permissions

---

## Scaling and Performance

### Auto Scaling Configuration

```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/testsanity-cluster/testsanity-service \
  --min-capacity 2 \
  --max-capacity 10 \
  --region us-east-1

# Create CPU-based scaling policy
aws application-autoscaling put-scaling-policy \
  --policy-name testsanity-cpu-scaling \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/testsanity-cluster/testsanity-service \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json \
  --region us-east-1
```

**scaling-policy.json:**
```json
{
  "TargetValue": 70.0,
  "PredefinedMetricSpecification": {
    "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
  },
  "ScaleInCooldown": 300,
  "ScaleOutCooldown": 60
}
```

### Performance Optimization

1. **.NET Runtime Optimization:**
   ```dockerfile
   # Enable ReadyToRun for faster startup
   RUN dotnet publish -c Release -o /app/publish -p:PublishReadyToRun=true
   
   # Configure GC for containers
   ENV DOTNET_gcServer=1 \
       DOTNET_GCHeapCount=2
   ```

2. **Connection Pooling:**
   - Configure database connection pooling
   - Use connection multiplexing for Redis

3. **Caching Strategy:**
   - Implement response caching
   - Use distributed caching with Redis

### Blue/Green Deployments

For zero-downtime deployments:

```bash
# Update service with deployment configuration
aws ecs update-service \
  --cluster testsanity-cluster \
  --service testsanity-service \
  --task-definition testsanity-task:2 \
  --deployment-configuration "maximumPercent=200,minimumHealthyPercent=100" \
  --region us-east-1
```

---

## Additional Resources

- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [AWS Fargate Best Practices](https://docs.aws.amazon.com/AmazonECS/latest/bestpracticesguide/fargate.html)
- [.NET on AWS](https://aws.amazon.com/developer/language/net/)
- [ASP.NET Core Performance Best Practices](https://docs.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)

---

## Support and Feedback

For issues or questions:
- Review CloudWatch logs: `/ecs/testsanity`
- Check ECS service events
- Review task stopped reasons
- Consult AWS Support

---

**Document Version:** 1.0  
**Last Updated:** 2026-02-03  
**Target Platform:** AWS ECS Fargate  
**Application:** TestSanity (.NET 8.0 ASP.NET Core)
