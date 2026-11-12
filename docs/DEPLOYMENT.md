# DEPLOYMENT GUIDE — OrbitAOS.V6 on AWS EKS

## Overview

OrbitAOS.V6 is an ASP.NET Core 6.0 MVC web application with Razor Pages, Identity, and SQL Server. This guide covers local setup, building and pushing container images to AWS ECR, and deploying to AWS EKS.

## Prerequisites and System Requirements

### Local Development
- .NET 6.0 SDK (or .NET 8 SDK for building)
- Node.js 18+ (for CSS purging in Docker build)
- Docker Desktop
- SQL Server (localdb or remote)

### AWS EKS Deployment
- AWS CLI v2
- kubectl (matching EKS cluster version)
- eksctl
- Docker
- IAM permissions for ECR and EKS

### AWS IAM Permissions Required
- `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:CompleteLayerUpload`, `ecr:UploadLayerPart`, `ecr:InitiateLayerUpload`, `ecr:PutImage`, `ecr:CreateRepository`, `ecr:DescribeRepositories`
- `eks:DescribeCluster`, `eks:ListClusters`
- Kubernetes RBAC for creating namespaces, deployments, services, ingresses, and secrets

## Local Setup with Docker Compose

1. Ensure Docker Desktop is running.
2. Build and run the application locally:

```bash
docker-compose up --build
```

3. The application will be available at `http://localhost:8080`.
4. The health endpoint is at `http://localhost:8080/health`.

### Environment Variables (Docker Compose)
| Variable | Description | Default |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | `Production` |
| `ASPNETCORE_URLS` | Kestrel listen URL | `http://+:8080` |
| `TZ` | Timezone | `UTC` |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | (from appsettings.json) |

## Building and Pushing Container Images

### Using build-push.sh (Linux/macOS)

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

The script will:
1. Prompt for target platform (AWS ECR or Docker Hub).
2. Collect registry credentials and image tag.
3. Log in to the chosen registry.
4. Build the Docker image with a multi-stage build (SDK → CSS purge → runtime).
5. Push the image to the registry.

### Using build-push.bat (Windows)

```cmd
scripts\build-push.bat
```

### Docker Build Details
- **Build stage**: `mcr.microsoft.com/dotnet/sdk:6.0` — restores, builds, and publishes the .NET application.
- **CSS purge stage**: `node:18-alpine` — runs PurgeCSS to strip unused Bootstrap CSS from `wwwroot/lib/bootstrap/dist/css/`.
- **Runtime stage**: `mcr.microsoft.com/dotnet/runtime:6.0` — runs the published application as a non-root user on port 8080.
- The application is published as **self-contained** (`-r linux-x64 --self-contained true`) because the `runtime:6.0` base image does not include the ASP.NET Core shared framework. The self-contained executable is started directly via `ENTRYPOINT ["./OrbitAOS.V6"]`.
- The application listens on `http://+:8080` (ASPNETCORE_URLS environment variable).

## EKS Cluster Setup

### 1. Create an EKS Cluster

```bash
eksctl create cluster \
  --name orbitaaos-v6 \
  --region us-east-1 \
  --nodegroup-name orbitaaos-v6-nodes \
  --node-type t3.medium \
  --nodes 2 \
  --nodes-max 4
```

### 2. Configure kubectl

```bash
aws eks update-kubeconfig --region us-east-1 --name orbitaaos-v6
kubectl cluster-info
```

### 3. Install the AWS Load Balancer Controller

Follow the official guide: https://docs.aws.amazon.com/eks/latest/userguide/aws-load-balancer-controller.html

## Deploying to EKS

### Using deploy-image.sh (Linux/macOS)

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

The script will:
1. Prompt for AWS region, EKS cluster name, and image URI.
2. Configure kubectl for the target cluster.
3. Prompt for environment variable values (with optional skip).
4. Collect credential values securely (without echoing).
5. Create a Kubernetes Secret for credentials.
6. Apply namespace, deployment, service, and ingress manifests.
7. Wait for the deployment rollout to complete.
8. Display pod, service, and ingress status.
9. Show the application URL.

### Using deploy-image.bat (Windows)

```cmd
scripts\deploy-image.bat
```

### Kubernetes Manifests

#### namespace.yaml
Creates the `orbitaaos-v6` namespace with label `name: orbitaaos-v6`.

#### deployment.yaml
- **Replicas**: 2
- **Container**: `orbitaaos-v6` with image `{{IMAGE_URI}}` (replaced at deploy time)
- **Port**: 8080
- **Environment variables**: `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`, `TZ`, `ConnectionStrings__DefaultConnection`, `DocsUrl`
- **Secrets**: Credentials are injected from the `orbitaaos-v6-secrets` Kubernetes Secret via `envFrom`
- **Resources**: requests (250m CPU, 512Mi memory), limits (500m CPU, 1Gi memory)
- **Liveness probe**: HTTP GET `/health` on port 8080 (30s initial delay, 10s interval)
- **Readiness probe**: HTTP GET `/health` on port 8080 (10s initial delay, 5s interval)
- **Image pull secret**: `ecr-regcred` for AWS ECR authentication

#### service.yaml
- **Type**: ClusterIP
- **Port**: 80 → 8080 (TCP)

#### ingress.yaml
- **Ingress class**: ALB (`kubernetes.io/ingress.class: alb`)
- **Scheme**: internet-facing
- **Host**: `orbitaaos-v6.example.com`
- **Health check path**: `/health`

### Post-Deployment Steps

1. **Configure ECR authentication** for the cluster:

```bash
kubectl create secret docker-registry ecr-regcred \
  --docker-server=<account>.dkr.ecr.<region>.amazonaws.com \
  --docker-username=AWS \
  --docker-password=$(aws ecr get-login-password --region <region>) \
  --namespace=orbitaaos-v6
```

2. **Verify the deployment**:

```bash
kubectl get pods -n orbitaaos-v6
kubectl get svc -n orbitaaos-v6
kubectl get ingress -n orbitaaos-v6
```

3. **Test the health endpoint**:

```bash
kubectl port-forward svc/orbitaaos-v6-service 8080:80 -n orbitaaos-v6
curl http://localhost:8080/health
```

## Configuration Management

### Environment Variables
All configuration is injected at runtime via environment variables:

| Variable | Source | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Dockerfile/deployment | Set to `Production` |
| `ASPNETCORE_URLS` | Dockerfile/deployment | Listen URL: `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | appsettings.json / deployment | SQL Server connection string |
| `DocsUrl` | appsettings.json / deployment | Documentation URL (was hardcoded in Index.cshtml) |
| `TZ` | Dockerfile/deployment | Timezone, set to `UTC` |

### Kubernetes Secrets
Credentials are stored in the `orbitaaos-v6-secrets` Secret, created by the deploy script. The Secret contains:
- `DB_PASSWORD` — SQL Server database password

### Configuration Files
- `appsettings.json` — base configuration with placeholder/default values
- `appsettings.Development.json` — development-specific settings
- No `appsettings.Production.json` exists; production configuration comes from environment variables

## Security Considerations and Best Practices

1. **Non-root user**: The Docker runtime image runs as a non-root user (UID 10001) for security.
2. **HTTPS redirection**: The application enforces HTTPS redirection via `app.UseHttpsRedirection()`.
3. **Authentication and authorization**: ASP.NET Core Identity with cookie authentication is configured. All controllers require authorization by default.
4. **CORS**: No CORS policy is configured; the application is same-origin only. If API access from other domains is needed, add a specific CORS policy.
5. **Data protection**: ASP.NET Core Identity uses data protection APIs. In a multi-pod deployment, configure a shared data protection key ring (e.g., via Azure Blob Storage or a shared volume).
6. **Secret management**: Database credentials are stored in Kubernetes Secrets, never in the image or source code.
7. **Container scanning**: Regularly scan the container image for vulnerabilities using tools like Trivy or Amazon ECR image scanning.
8. **Network policies**: Consider adding Kubernetes Network Policies to restrict pod-to-pod communication.
9. **Resource limits**: CPU and memory limits are set to prevent resource exhaustion.
10. **Image scanning**: Enable Amazon ECR image scanning on push.

## .NET Notes

### Runtime Settings
- **Target framework**: .NET 6.0 (`net6.0`)
- **Base image**: `mcr.microsoft.com/dotnet/runtime:6.0` (Linux, non-root)
- **Listen port**: 8080 (configured via `ASPNETCORE_URLS=http://+:8080`)
- **Environment**: `ASPNETCORE_ENVIRONMENT=Production`

### Garbage Collector Tuning
The Dockerfile sets the following GC environment variables for container optimization:
- `DOTNET_gcServer=1` — enables server GC mode (better for multi-core containers)
- `DOTNET_gcConcurrent=1` — enables background GC
- `DOTNET_GCHeapHardLimit=80000000` — limits GC heap to ~1GB (adjust based on container memory limits)

### Kestrel Configuration
Kestrel is configured via the `ASPNETCORE_URLS` environment variable to listen on `http://+:8080`. No HTTPS endpoint is configured in the container; HTTPS termination is handled by the ALB ingress.

### Static File Caching
The `UseStaticFiles()` middleware is configured with `StaticFileOptions` that sets:
- `Cache-Control: public, max-age=31536000, immutable` for fingerprinted assets (files with hash in filename)
- `Cache-Control: public, max-age=86400, must-revalidate` for non-fingerprinted assets
- `Vary: Accept-Encoding` header on non-fingerprinted assets

This enables CDN caching through CloudFront and the ALB ingress.

### CSS Purging
A Node.js build stage in the Dockerfile runs PurgeCSS to strip unused Bootstrap CSS rules before producing the final container image. This reduces image size and improves pull times. The PurgeCSS configuration is in `purgecss.config.js`, scanning all `.cshtml` view files for used CSS classes.

### Health Check Endpoint
The application exposes a health check endpoint at `/health` using ASP.NET Core's built-in `AddHealthChecks()` and `MapHealthChecks("/health")`. This is used by Kubernetes liveness and readiness probes.

### SQL Server Connection
The application uses Entity Framework Core with SQL Server. The connection string is provided via the `ConnectionStrings__DefaultConnection` environment variable, which maps to `ConnectionStrings:DefaultConnection` in the .NET configuration system (using the `__` separator for hierarchy levels).

### Monitoring
Consider integrating Application Insights or OpenTelemetry for production monitoring:
- Add the `Microsoft.ApplicationInsights.AspNetCore` NuGet package.
- Configure the instrumentation key via the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable.
