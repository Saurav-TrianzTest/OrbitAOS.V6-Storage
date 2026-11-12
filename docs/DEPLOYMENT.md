# Deployment Guide — OrbitAOS.V6 on AWS EKS

## Prerequisites and System Requirements

### Prerequisites

- **Docker** (20.10+) — for building and pushing container images
- **AWS CLI** (2.0+) — for ECR authentication and EKS cluster configuration
- **kubectl** (1.21+) — for applying Kubernetes manifests
- **eksctl** (0.140+) — for creating EKS clusters (optional, if you need to create a new cluster)
- **PowerShell Core** (for Windows build/push and deploy scripts)

### IAM Permissions

The AWS user/role running these scripts needs the following permissions:

- `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:CompleteLayerUpload`, `ecr:UploadLayerPart`, `ecr:PutImage`, `ecr:InitiateLayerUpload`
- `ecr:DescribeRepositories`, `ecr:CreateRepository`
- `eks:DescribeCluster`, `eks:ListClusters`
- `eks:AccessKubernetesCluster` (or equivalent `eks:DescribeCluster` + kubeconfig)
- Kubernetes RBAC permissions to create/modify resources in the target namespace

### System Requirements

- **Runtime:** .NET 6.0 (self-contained deployment, includes the ASP.NET Core shared framework)
- **Base image:** `mcr.microsoft.com/dotnet/runtime:6.0`
- **Container port:** 8080 (non-root user, listens on `http://+:8080`)
- **Platform:** Linux/amd64

## Local Setup with Docker Compose

To run the application locally with Docker Compose:

```bash
# Set required environment variables
export ConnectionStrings__DefaultConnection="Server=sql-server,1433;Database=OrbitAOS;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
export AppUrls__Documentation="https://docs.microsoft.com/aspnet/core"

# Build and start
docker compose up --build
```

The application will be available at `http://localhost:8080`.

### docker-compose.yml Overview

The `docker-compose.yml` defines a single service (`orbitaos-v6`) with:

- **Image:** `orbitaos-v6:latest`
- **Port mapping:** `8080:8080`
- **Environment variables:** `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`, `ConnectionStrings__DefaultConnection`, `AppUrls__Documentation`
- **Volumes:** `orbitaos-v6-logs` for logs, and bind-mounts for `appsettings.json` and `appsettings.Production.json`

## Building and Pushing the Container Image

### Using build-push.sh (Linux/macOS)

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

The script will:
1. Ask you to choose a target platform (AWS ECR or Docker Hub)
2. Prompt for the required registry details
3. Ask for an image tag (press Enter for `latest`)
4. Build the Docker image with `docker build -f Dockerfile -t <registry>/<repo>:<tag> .`
5. Log in to the chosen registry
6. Create the ECR repository if it does not exist (AWS ECR only)
7. Push the image

### Using build-push.bat (Windows)

```cmd
scripts\build-push.bat
```

The Windows script follows the same flow with Windows error handling (`!ERRORLEVEL!`).

### Docker Build Stages

The Dockerfile uses a three-stage multi-stage build:

1. **Build stage (`mcr.microsoft.com/dotnet/sdk:6.0`):** Restores NuGet packages and publishes the application as self-contained for `linux-x64`.
2. **PurgeCSS stage (`node:18-alpine`):** Strips unused CSS from Bootstrap files using PurgeCSS, implementing the `cz-css-1005` remediation.
3. **Runtime stage (`mcr.microsoft.com/dotnet/runtime:6.0`):** Runs the self-contained application as a non-root user on port 8080.

The runtime stage uses `mcr.microsoft.com/dotnet/runtime:6.0` as the base image. Since this image does not include the ASP.NET Core shared framework, the build publishes the application as self-contained (`--self-contained true -r linux-x64`), which bundles the ASP.NET Core runtime into the publish output.

## AWS EKS Deployment

### EKS Cluster Setup

If you do not already have an EKS cluster:

```bash
# Create a cluster with eksctl
eksctl create cluster \
  --name orbitaos-v6-cluster \
  --region us-east-1 \
  --version 1.28 \
  --nodegroup-name orbitaos-v6-nodes \
  --node-type t3.medium \
  --nodes 2 \
  --nodes-max 4

# Or configure kubectl for an existing cluster
aws eks update-kubeconfig --region us-east-1 --name orbitaos-v6-cluster
```

### Deploying to EKS

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

The script will:
1. Ask for the AWS region and EKS cluster name
2. Prompt for the full image URI (e.g., `123456789012.dkr.ecr.us-east-1.amazonaws.com/orbitaos-v6:latest`)
3. Configure kubectl for the target EKS cluster
4. Ask for non-credential environment variable values (e.g., `AppUrls__Documentation`)
5. Ask for credential values without echoing (e.g., `ConnectionStrings__DefaultConnection`)
6. Apply `kubernetes/namespace.yaml`
7. Create a Kubernetes Secret (`orbitaos-v6-secrets`) with the credentials
8. Apply `kubernetes/deployment.yaml` (with placeholder replacements)
9. Apply `kubernetes/service.yaml`
10. Apply `kubernetes/ingress.yaml`
11. Wait for the deployment rollout to complete
12. Display pod, service, and ingress status
13. Print the application URL

### Kubernetes Manifests

#### namespace.yaml

Creates a `Namespace` named `orbitaos-v6` with the label `name: orbitaos-v6`.

#### deployment.yaml

Creates a `Deployment` with:
- **Replicas:** 2
- **Container:** `orbitaos-v6` with image `{{IMAGE_URI}}` (replaced at deploy time)
- **Port:** 8080
- **Security context:** Runs as non-root user (UID/GID 1000)
- **Environment variables:**
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ASPNETCORE_URLS=http://+:8080`
  - `ConnectionStrings__DefaultConnection` — from Kubernetes Secret (`orbitaos-v6-secrets`, key `connection-string`)
  - `AppUrls__Documentation` — from deploy-time input (placeholder `{{APPURLS__DOCUMENTATION}}`)
- **Resources:**
  - Requests: CPU 250m, Memory 512Mi
  - Limits: CPU 500m, Memory 1Gi
- **Liveness probe:** HTTP GET `/health` on port 8080 (initial delay 30s, period 10s)
- **Readiness probe:** HTTP GET `/health` on port 8080 (initial delay 10s, period 5s)

#### service.yaml

Creates a `ClusterIP` `Service` named `orbitaos-v6-service` that maps port 80 to the container's port 8080.

#### ingress.yaml

Creates an `Ingress` named `orbitaos-v6-ingress` with:
- **Ingress class:** `alb` (AWS Load Balancer Controller)
- **Scheme:** `internet-facing`
- **Health check path:** `/health`
- **Host:** `orbitaos-v6.example.com`
- **Path:** `/` (Prefix) → `orbitaos-v6-service:80`

### Rollback

If the deployment fails or you need to roll back:

```bash
kubectl rollout undo deployment/orbitaos-v6 -n orbitaos-v6
```

To check rollout status:

```bash
kubectl rollout status deployment/orbitaos-v6 -n orbitaos-v6
```

## Configuration Management

### Environment Variables

The application reads configuration through .NET's configuration system, which supports environment variable overrides. The following environment variables are recognized:

| Variable | Description | Default |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | The ASP.NET Core environment | `Production` |
| `ASPNETCORE_URLS` | The URL the application listens on | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | (from appsettings.json) |
| `AppUrls__Documentation` | Documentation URL shown on the home page | (from appsettings.json) |

> **Note:** In .NET, environment variables use double underscores (`__`) as the hierarchy separator. For example, `AppUrls__Documentation` maps to the configuration key `AppUrls:Documentation`.

### Kubernetes Secrets

Credentials (such as the database connection string) are stored in a Kubernetes Secret named `orbitaos-v6-secrets` and injected into the pod via `valueFrom.secretKeyRef`. The secret is created during deployment by `deploy-image.sh` / `deploy-image.bat`.

### ConfigMaps

Non-sensitive configuration values (such as `AppUrls__Documentation`) are passed as environment variables during deployment. For production multi-environment deployments, these can be moved to a Kubernetes ConfigMap.

## Health Check Endpoint

The application exposes a health check endpoint at `GET /health`.

- **Implementation:** ASP.NET Core Health Checks (`builder.Services.AddHealthChecks()` and `app.MapHealthChecks("/health")`)
- **Port:** 8080 (same as the application)
- **Used by:** Kubernetes liveness and readiness probes, and the AWS Load Balancer Controller health check

The endpoint returns a `200 OK` response when the application is healthy.

## .NET Notes

### Runtime Settings

- **Framework:** .NET 6.0 (TargetFramework `net6.0`)
- **Deployment mode:** Self-contained (`--self-contained true -r linux-x64`)
  - The runtime stage uses `mcr.microsoft.com/dotnet/runtime:6.0`, which does not include the ASP.NET Core shared framework. A self-contained publish bundles the ASP.NET Core runtime into the application, so no additional shared framework layer is needed.
- **Entry point:** The published self-contained executable (`OrbitAOS.V6`)
- **Listening port:** 8080 via `ASPNETCORE_URLS=http://+:8080`

### Garbage Collector Tuning

The application runs with the default .NET 6.0 garbage collector settings. For containerized environments, the following environment variables can be used to tune GC behavior:

- `DOTNET_gcServer=1` — Enables server GC (recommended for multi-core containers)
- `DOTNET_gcConcurrent=1` — Enables concurrent GC
- `DOTNET_GCHeapHardLimit` — Sets the maximum GC heap size

The Dockerfile sets `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` to enable full globalization support (required for date/time and currency formatting).

### Security Considerations

- **Non-root user:** The runtime stage creates and runs as `appuser` (UID 1000, GID 1000). The Dockerfile uses `adduser` and `USER appuser` to enforce this.
- **HTTPS redirection:** The application calls `app.UseHttpsRedirection()` in `Program.cs`. In the container, `ASPNETCORE_URLS` is set to `http://+:8080` (HTTP), and HTTPS termination is handled by the AWS Load Balancer (ALB) in front of the container.
- **Security headers:** The application uses the default ASP.NET Core security headers. For additional headers, consider adding `X-Content-Type-Options`, `X-Frame-Options`, and `Content-Security-Policy` via middleware.
- **Authentication and authorization:** The application uses ASP.NET Core Identity with SQL Server. Authentication is configured via `app.UseAuthentication()` and `app.UseAuthorization()`.
- **CORS:** No CORS policy is configured by default. If cross-origin requests are needed, add `builder.Services.AddCors()` and configure policies in `Program.cs`.
- **Data protection:** ASP.NET Core Identity uses data protection for token generation. In a multi-replica deployment, configure a shared data protection key ring (e.g., via Azure Key Vault or a shared file volume).
- **Container image security:** The runtime stage is based on `mcr.microsoft.com/dotnet/runtime:6.0`, which receives regular security updates. No package manager is used in the runtime stage. The image is scanned as part of the CI/CD pipeline.

### Monitoring

- **Logging:** The application uses the default .NET logging configuration. Logs are written to stdout/stderr and can be collected by the container runtime.
- **Application Insights:** Not currently configured. To add Application Insights, add the `Microsoft.ApplicationInsights.AspNetCore` package and configure the instrumentation key via the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable.
