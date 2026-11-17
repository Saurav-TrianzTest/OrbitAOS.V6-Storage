# OrbitAOS.V6 — Deployment Guide

## Prerequisites and System Requirements

### Local Development
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- Docker Desktop 4.0+ (or Docker Engine 20.10+)
- Node.js 18+ (only needed for PurgeCSS CSS optimization during container builds)

### AWS EKS Deployment
- AWS CLI 2.x
- kubectl 1.25+ (matching your EKS cluster version)
- eksctl (for cluster creation)
- Docker Engine (for building images)
- IAM permissions: `ecr:*`, `eks:DescribeCluster`, `eks:UpdateKubeconfig`, `sts:GetCallerIdentity`

### ECR Repository
An ECR repository named `orbita-os-v6` will be created automatically by the build-push script if it does not exist.

## Local Setup with Docker Compose

```bash
# Set required environment variables
export DB_CONNECTION_STRING="Server=your-sql-server;Database=OrbitAOS;User Id=sa;Password=YourStrong@Pass1;TrustServerCertificate=true"

# Build and run locally
docker-compose up --build

# The application will be available at http://localhost:8080
# Health check endpoint: http://localhost:8080/health
```

## Building and Pushing the Container Image

### AWS ECR (Recommended for EKS)

```bash
# Linux/macOS
./scripts/build-push.sh

# Windows
scripts\build-push.bat
```

The script will prompt you for:
1. AWS region (e.g., `us-east-1`)
2. ECR repository name (defaults to `orbita-os-v6`)
3. Image tag (defaults to `latest`)

It will then:
- Log in to AWS ECR using `aws ecr get-login-password`
- Create the ECR repository if it does not exist
- Build the Docker image (multi-stage: PurgeCSS → .NET SDK → .NET runtime)
- Push the image to ECR

### Docker Hub

```bash
# Linux/macOS
./scripts/build-push.sh

# Windows
scripts\build-push.bat
```

Select option 2 when prompted. The script will ask for Docker Hub credentials and repository name.

### Image Name Convention
- Project name: `OrbitAOS.V6`
- Container image name: `orbita-os-v6` (lowercase, hyphens for separators)
- Full ECR image path: `<account>.dkr.ecr.<region>.amazonaws.com/orbita-os-v6:<tag>`

## Docker Build Architecture

The Dockerfile uses a multi-stage build:

1. **PurgeCSS Stage** (`node:18-alpine`): Runs PurgeCSS to strip unused CSS rules from Bootstrap library files, reducing image size. Content files scanned: all `.cshtml` Razor views, `.html` files, `.css` files, and `.js` files in `wwwroot/`.

2. **Build Stage** (`mcr.microsoft.com/dotnet/sdk:6.0`): Restores NuGet packages, copies purged CSS files, and publishes the application as a self-contained deployment for `linux-x64`.

3. **Runtime Stage** (`mcr.microsoft.com/dotnet/runtime:6.0`): Runs the self-contained application executable as a non-root user on port 8080.

### Self-Contained Deployment
Since the runtime stage uses `mcr.microsoft.com/dotnet/runtime:6.0` (which does not include the ASP.NET Core shared framework), the build publishes as a **self-contained** deployment (`--self-contained true -r linux-x64`). This bundles the .NET runtime and ASP.NET Core libraries into the image, making it self-contained and not dependent on the shared framework.

## Kubernetes Deployment on AWS EKS

### Cluster Setup

```bash
# Create EKS cluster (if not already existing)
eksctl create cluster \
  --name orbita-os-v6-cluster \
  --region us-east-1 \
  --nodegroup-name orbita-os-v6-nodes \
  --node-type t3.medium \
  --nodes 2 \
  --nodes-max 4

# Configure kubectl
aws eks update-kubeconfig --region us-east-1 --name orbita-os-v6-cluster
```

### Deploy the Application

```bash
# Linux/macOS
./scripts/deploy-image.sh

# Windows
scripts\deploy-image.bat
```

The deployment script will prompt you for:
1. AWS region
2. EKS cluster name
3. Image URI (full path with registry and tag from the build-push step)
4. Environment variables: `DB_HOST`, `DB_PORT`, `DB_NAME`, `DocsUrl`
5. Credentials: `ConnectionStrings__DefaultConnection` (entered without echoing)

The script will then:
- Configure kubectl for the EKS cluster
- Apply the namespace manifest
- Create a Kubernetes Secret with credentials
- Replace placeholders in the deployment manifest
- Apply deployment, service, and ingress manifests
- Wait for the deployment to roll out
- Display pod, service, and ingress status

### Kubernetes Manifests

| File | Description |
|------|-------------|
| `kubernetes/namespace.yaml` | Namespace `orbita-os-v6` |
| `kubernetes/deployment.yaml` | Deployment with 2 replicas, resource limits, health probes |
| `kubernetes/service.yaml` | ClusterIP service on port 80 → container port 8080 |
| `kubernetes/ingress.yaml` | ALB Ingress with HTTPS, host `orbita-os-v6.example.com` |

### Deployment Configuration

- **Replicas**: 2 (for high availability)
- **Container Port**: 8080 (ASPNETCORE_URLS=http://+:8080)
- **Health Endpoint**: `/health` (both liveness and readiness probes)
- **Resource Requests**: 250m CPU, 512Mi memory
- **Resource Limits**: 500m CPU, 1Gi memory
- **Ingress**: AWS ALB (internet-facing) with HTTPS termination

### Environment Variables

| Variable | Description | Source |
|----------|-------------|--------|
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` | Hardcoded in deployment |
| `ASPNETCORE_URLS` | `http://+:8080` | Hardcoded in deployment |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | Kubernetes Secret |
| `DB_HOST` | Database host | ConfigMap / deployment prompt |
| `DB_PORT` | Database port | ConfigMap / deployment prompt |
| `DB_NAME` | Database name | ConfigMap / deployment prompt |
| `DocsUrl` | Documentation URL shown on home page | Deployment prompt / environment variable |

### Secrets Management

Database credentials are stored in a Kubernetes Secret named `orbita-os-v6-secrets`:
```bash
kubectl get secret orbita-os-v6-secrets -n orbita-os-v6 -o yaml
```

The connection string is stored under the key `connection-string`.

## Configuration Management

- The application reads configuration from `appsettings.json` and environment variables.
- The connection string in `appsettings.json` is left empty and must be provided via the `ConnectionStrings__DefaultConnection` environment variable.
- In .NET, environment variables use `__` (double underscore) as the hierarchy separator. For example, `ConnectionStrings__DefaultConnection` maps to the `ConnectionStrings:DefaultConnection` configuration key.
- For EKS, environment variables are set in the Deployment manifest. Credentials come from Kubernetes Secrets; non-sensitive configuration can come from ConfigMaps.

## Security Considerations and Best Practices

### Container Security
- **Non-root user**: The container runs as `appuser` (non-root) for reduced attack surface.
- **Minimal base image**: The runtime stage uses `mcr.microsoft.com/dotnet/runtime:6.0` with no package manager or unnecessary tools.
- **Self-contained deployment**: The application includes only the .NET runtime components it needs.
- **No HEALTHCHECK instruction**: The platform probes the application's `/health` endpoint directly.

### Application Security
- **HTTPS redirection**: The application uses `UseHttpsRedirection()`. In EKS, HTTPS is terminated at the ALB.
- **Authentication and Authorization**: ASP.NET Core Identity with cookie authentication is configured.
- **Data Protection**: ASP.NET Core's data protection API is used for Identity. In a multi-replica deployment, configure a shared data protection key ring (e.g., via Azure Blob storage or a shared volume).
- **CORS**: No CORS policy is configured by default. If the API is consumed by external frontends, add a CORS policy in `Program.cs`.

### Network Security
- The ALB is internet-facing but only exposes port 443 (HTTPS).
- The Service is ClusterIP (not LoadBalancer), so it is only accessible within the cluster.
- Database connections should use private networking (VPC security groups, private endpoints).

## .NET Notes

### Runtime Settings
- **Framework**: .NET 6.0, published as self-contained for `linux-x64`.
- **GC Settings**: `DOTNET_gcServer=true` enables server garbage collection, optimized for multi-core container environments.
- **Globalization**: `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` enables full ICU globalization support (included in the self-contained publish).
- **Timezone**: `TZ=UTC` for consistent time handling across pods.

### Kestrel Configuration
- The application listens on `http://+:8080` via `ASPNETCORE_URLS`.
- In EKS, HTTPS is terminated at the ALB. The ALB forwards HTTP to the container on port 8080.
- For proper HTTPS redirect handling behind a reverse proxy, consider adding `UseForwardedHeaders()` in `Program.cs` to respect `X-Forwarded-Proto` headers from the ALB.

### Monitoring
- **Logging**: The application uses the default ASP.NET Core logging. In EKS, logs are captured by `kubectl logs`.
- **Health Check**: A `/health` endpoint is configured using `AddHealthChecks()` and `MapHealthChecks("/health")`. Both liveness and readiness probes use this endpoint.
- **Application Insights**: Not configured. If needed, add the `Microsoft.ApplicationInsights.AspNetCore` NuGet package and configure the instrumentation key via the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable.

### Static File Caching
- Static files in `wwwroot/` are served with `Cache-Control: public, max-age=86400, must-revalidate` and `Vary: Accept-Encoding` headers.
- PurgeCSS strips unused CSS from Bootstrap library files during the Docker build, reducing image size.
- For fingerprinted assets (e.g., `site.css` with `asp-append-version="true"`), consider extending the cache duration to `max-age=31536000, immutable`.

### Troubleshooting

#### Pod Fails to Start
```bash
# Check pod status
kubectl get pods -n orbita-os-v6

# Check pod logs
kubectl logs <pod-name> -n orbita-os-v6

# Check events
kubectl describe pod <pod-name> -n orbita-os-v6
```

Common issues:
- **ImagePullBackOff**: Verify the image URI and ECR credentials. Run `aws ecr get-login-password --region <region> | docker login --username AWS --password-stdin <registry>` manually.
- **CrashLoopBackOff**: Check application logs for startup errors. Verify the connection string secret is set correctly.
- **Readiness probe failed**: The application may take time to start. Increase `initialDelaySeconds` in the readiness probe.

#### Service Issues
```bash
# Check service
kubectl get svc -n orbita-os-v6

# Test connectivity from within the cluster
kubectl run debug --image=curlimages/curl -it --rm -- sh
curl http://orbita-os-v6-service:80/health
```

#### Ingress Problems
```bash
# Check ingress
kubectl get ingress -n orbita-os-v6

# Check ALB logs (if enabled)
aws logs tail /aws/eks/cluster-name/alb-ingress --follow
```

Common issues:
- **502 Bad Gateway**: Verify the service is running and the target port matches the container port.
- **404 Not Found**: Check the ingress path rules and host header.
- **SSL certificate errors**: Verify the ACM certificate ARN in the ingress annotations.

### Scaling and Management

#### Horizontal Pod Autoscaler (HPA)
```bash
# Create HPA based on CPU utilization
kubectl autoscale deployment orbita-os-v6 --cpu-percent=60 --min=2 --max=10 -n orbita-os-v6

# Check HPA status
kubectl get hpa -n orbita-os-v6
```

#### Rolling Updates
```bash
# Update the image
kubectl set image deployment/orbita-os-v6 orbita-os-v6=<new-image-uri> -n orbita-os-v6

# Monitor rollout
kubectl rollout status deployment/orbita-os-v6 -n orbita-os-v6

# Rollback if needed
kubectl rollout undo deployment/orbita-os-v6 -n orbita-os-v6
```

#### Rollback
```bash
# Rollback to previous revision
kubectl rollout undo deployment/orbita-os-v6 -n orbita-os-v6

# Rollback to specific revision
kubectl rollout history deployment/orbita-os-v6 -n orbita-os-v6
kubectl rollout undo deployment/orbita-os-v6 --to-revision=<revision> -n orbita-os-v6
```
