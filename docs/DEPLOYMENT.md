# OrbitContainerFix - Deployment Guide

## Overview

OrbitContainerFix is an ASP.NET Core 6.0 web application deployed to AWS Elastic Kubernetes Service (EKS). This document covers prerequisites, local setup, building and pushing container images, and deploying to EKS.

## Prerequisites

### System Requirements

- **.NET 6.0 SDK** (for local development and building)
- **Docker** (for building and running container images)
- **AWS CLI** (for ECR authentication and EKS cluster management)
- **kubectl** (for interacting with the Kubernetes cluster)
- **eksctl** (for EKS cluster creation, if needed)

### AWS Permissions

The deploying user/role needs the following IAM permissions:

- `ecr:GetAuthorizationToken`
- `ecr:BatchCheckLayerAvailability`
- `ecr:GetDownloadUrlForLayer`
- `ecr:BatchGetImage`
- `ecr:PutImage`
- `ecr:InitiateLayerUpload`
- `ecr:UploadLayerPart`
- `ecr:CompleteLayerUpload`
- `ecr:DescribeRepositories`
- `ecr:CreateRepository`
- `eks:DescribeCluster`
- `eks:ListClusters`

## Local Setup with Docker Compose

1. Ensure Docker is running.
2. Set the required environment variables:

   ```bash
   export DB_CONNECTION_STRING="Server=your-db-server;Database=OrbitContainerFix;User Id=sa;Password=YourPassword123;"
   export DOCS_URL="https://docs.microsoft.com/aspnet/core"
   ```

3. Build and start the application:

   ```bash
   docker-compose up --build
   ```

4. The application will be available at `http://localhost:8080`.
5. The health check endpoint is available at `http://localhost:8080/health`.

## Building and Pushing the Container Image

### Using build-push.sh (Linux/macOS)

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

The script will:
1. Ask you to select a target platform (AWS ECR or Docker Hub).
2. For AWS ECR: ask for the AWS region, ECR registry URL, and repository name.
3. Ask for an image tag (press Enter for `latest`).
4. Log in to the selected registry.
5. Build the Docker image using the multi-stage Dockerfile.
6. Push the image to the registry.

### Using build-push.bat (Windows)

```cmd
scripts\build-push.bat
```

The script follows the same flow as the Linux version but uses Windows commands.

### Dockerfile Details

The project uses a multi-stage Docker build:

- **Build stage**: Uses `mcr.microsoft.com/dotnet/sdk:6.0` to restore, compile, and publish the application as a self-contained deployment for `linux-x64`.
- **Runtime stage**: Uses `mcr.microsoft.com/dotnet/runtime:6.0` as the base image. Since this image does not include the ASP.NET Core shared framework, the application is published as self-contained, bundling the ASP.NET Core runtime within the application.

The application runs on port 8080 inside the container, listening on `http://+:8080`.

## Deploying to AWS EKS

### EKS Cluster Setup

If you need to create a new EKS cluster:

```bash
eksctl create cluster \
  --name orbitcontainerfix-cluster \
  --region us-east-1 \
  --nodegroup-name orbitcontainerfix-nodes \
  --node-type t3.medium \
  --nodes 2 \
  --nodes-max 4 \
  --managed
```

### Configuring kubectl

```bash
aws eks update-kubeconfig --region us-east-1 --name orbitcontainerfix-cluster
```

Verify connectivity:

```bash
kubectl cluster-info
```

### Deploying the Application

1. Ensure you have pushed the container image to a registry accessible by EKS (e.g., AWS ECR).
2. Run the deploy script:

   ```bash
   chmod +x scripts/deploy-image.sh
   ./scripts/deploy-image.sh
   ```

3. The script will ask for:
   - AWS region
   - EKS cluster name
   - Full image URI (e.g., `123456789012.dkr.ecr.us-east-1.amazonaws.com/orbitcontainerfix:v1.0.0`)
   - Environment variable values (DB connection string, docs URL)

4. The script will:
   - Create a Kubernetes Secret with the DB connection string
   - Configure kubectl for the EKS cluster
   - Apply the namespace, deployment, service, and ingress manifests
   - Wait for the deployment rollout to complete
   - Display the current pods, services, and ingress resources

### Deployment Manifests

The Kubernetes manifests are located in the `kubernetes/` directory:

- **namespace.yaml**: Creates the `orbitcontainerfix` namespace.
- **deployment.yaml**: Defines a Deployment with 2 replicas, resource limits, liveness/readiness probes on `/health`, and environment variables. The DB connection string is sourced from a Kubernetes Secret.
- **service.yaml**: Creates a ClusterIP Service routing port 80 to the container's port 8080.
- **ingress.yaml**: Creates an Ingress with the AWS Load Balancer Controller (ALB), configured as internet-facing.

### Rolling Updates

To deploy a new version:

1. Build and push the new image.
2. Update the image URI in the deployment:

   ```bash
   kubectl set image deployment/orbitcontainerfix orbitcontainerfix=<new-image-uri> -n orbitcontainerfix
   ```

3. Monitor the rollout:

   ```bash
   kubectl rollout status deployment/orbitcontainerfix -n orbitcontainerfix
   ```

### Rollback

To roll back to a previous revision:

```bash
kubectl rollout undo deployment/orbitcontainerfix -n orbitcontainerfix
```

### Scaling

To manually scale the deployment:

```bash
kubectl scale deployment/orbitcontainerfix --replicas=3 -n orbitcontainerfix
```

To set up a Horizontal Pod Autoscaler (HPA):

```bash
kubectl autoscale deployment/orbitcontainerfix --cpu-percent=60 --min=2 --max=10 -n orbitcontainerfix
```

## Configuration Management

### Environment Variables

The application reads the following environment variables:

| Variable | Description | Default |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | `Production` |
| `ASPNETCORE_URLS` | Kestrel listen URL | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | Database connection string | (from appsettings.json) |
| `DocsUrl` | Documentation URL shown on home page | `https://docs.microsoft.com/aspnet/core` |

### Kubernetes Secrets

The DB connection string is stored in a Kubernetes Secret named `orbitcontainerfix-secrets` in the `orbitcontainerfix` namespace. It is referenced in the deployment via `valueFrom.secretKeyRef`.

### Configuration Files

- `appsettings.json`: Base configuration with local development defaults.
- `appsettings.Development.json`: Development-specific overrides.
- `appsettings.Production.json`: Production-specific overrides (if present).

## Security Considerations

### Non-Root Container User

The Dockerfile creates a non-root user (`appuser`, UID 1000) and runs the application as that user. This follows the principle of least privilege.

### HTTPS Redirection

The application has `UseHttpsRedirection()` enabled. In production behind a load balancer, ensure the ALB terminates TLS and forwards traffic to the container on port 8080.

### Security Headers

The application should be deployed behind a load balancer that handles TLS termination. Consider adding additional security headers via the ingress annotations.

### Authentication and Authorization

The application uses ASP.NET Core Identity with Entity Framework for user management. The default identity configuration requires account confirmation for sign-in.

### CORS

No CORS is configured, which is appropriate for a same-origin web application. If API endpoints are consumed from other origins, configure CORS policies in `Program.cs`.

### Data Protection

The application uses ASP.NET Core's default data protection. In a multi-pod deployment, configure a shared data protection key ring using a Kubernetes Secret or AWS Secrets Manager.

## .NET Notes

### Runtime Settings

- **Target Framework**: .NET 6.0
- **Self-contained deployment**: The application is published as self-contained (`-r linux-x64 --self-contained true`) because the runtime base image (`mcr.microsoft.com/dotnet/runtime:6.0`) does not include the ASP.NET Core shared framework.
- **Kestrel Configuration**: The application listens on `http://+:8080` via the `ASPNETCORE_URLS` environment variable.
- **Garbage Collector**: `DOTNET_GC_SERVER=1` enables server GC for better throughput in multi-core containers. `DOTNET_GC_CONCURRENT=1` enables concurrent GC.

### Health Check

The application exposes a health check endpoint at `/health` using ASP.NET Core's built-in `AddHealthChecks()` and `MapHealthChecks("/health")`. The Kubernetes liveness and readiness probes use this endpoint.

### Static File Caching

The application configures `StaticFileOptions` with cache-control headers (`max-age=1 day, must-revalidate`) for static files served from `wwwroot`. This enables CDN caching through CloudFront and the ALB.

### Monitoring

The application uses ASP.NET Core's built-in logging. In production, consider integrating with AWS CloudWatch or Application Insights for centralized log collection.

### Database

The application uses Entity Framework Core with SQL Server. The connection string is provided via the `ConnectionStrings__DefaultConnection` environment variable, sourced from a Kubernetes Secret at runtime.
