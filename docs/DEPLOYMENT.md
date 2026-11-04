# OrbitContainer — Deployment Guide

## Overview

OrbitContainer is an ASP.NET Core 6.0 web application deployed as a container on **AWS Elastic Kubernetes Service (EKS)**. The application uses a self-contained .NET 6.0 publish (the runtime base image is `mcr.microsoft.com/dotnet/runtime:6.0`, which does not include the ASP.NET Core shared framework, so the application is published with `--self-contained true` for `linux-x64`).

## Prerequisites

| Tool | Version |
|------|---------|
| Docker | 20.10+ |
| .NET SDK | 6.0 |
| AWS CLI | 2.0+ |
| kubectl | 1.21+ |
| eksctl | 0.80+ |

### AWS Permissions

The IAM user or role used for deployment must have:
- `AmazonEC2ContainerRegistryFullAccess` (for ECR push/pull)
- `AmazonEKSClusterPolicy` and `AmazonEKSWorkerNodePolicy` (for cluster management)
- `AmazonEKS_CNI_Policy` (for networking)
- `AmazonEKSALBIngressControllerPolicy` (for ALB ingress)

## Local Setup with Docker Compose

1. Set the required environment variable:
   ```bash
   export ConnectionStrings__DefaultConnection="Server=your-sql-server;Database=OrbitContainer;User Id=sa;Password=YourStrong!Passw0rd;"
   ```
2. Build and run locally:
   ```bash
   docker compose up --build
   ```
3. The application is available at `http://localhost:8080`.

## Building and Pushing the Image

### Linux / macOS

```bash
./scripts/build-push.sh
```

### Windows

```cmd
scripts\build-push.bat
```

The scripts will:
1. Ask you to choose the target registry (AWS ECR or Docker Hub).
2. Collect the registry credentials (AWS region + account ID for ECR).
3. Log in to the chosen registry.
4. Create the ECR repository if it does not exist.
5. Build the Docker image with `docker build -f Dockerfile -t <registry>/<repo>:<tag> .`.
6. Push the image.

## Deploying to AWS EKS

### Cluster Setup

1. Create an EKS cluster (if not already created):
   ```bash
   eksctl create cluster --name orbitcontainer-cluster --region us-east-1
   ```
2. Ensure the AWS Load Balancer Controller is installed on your cluster:
   ```bash
   eksctl utils associate-iam-oidc-provider --cluster orbitcontainer-cluster --region us-east-1 --approve
   ```

### kubectl Configuration

```bash
aws eks update-kubeconfig --region us-east-1 --name orbitcontainer-cluster
kubectl cluster-info
```

### Deployment Walkthrough

The deployment consists of four Kubernetes manifests under `kubernetes/`:

1. **namespace.yaml** — Creates the `orbitcontainer` namespace.
2. **deployment.yaml** — Defines a Deployment with 2 replicas, resource requests and limits, liveness and readiness probes on `/health` (port 8080), and environment variables. The database connection string is sourced from a Kubernetes Secret (`orbitcontainer-secrets`).
3. **service.yaml** — Creates a ClusterIP Service exposing port 80 → container port 8080.
4. **ingress.yaml** — Creates an ALB Ingress for `orbitcontainer.example.com` with internet-facing scheme.

### Deploy the Image

```bash
./scripts/deploy-image.sh
```

### Windows

```cmd
scripts\deploy-image.bat
```

The script will:
1. Ask for the AWS region, EKS cluster name, and the full image URI.
2. Configure kubectl for the EKS cluster.
3. Ask for non-credential environment variables (e.g., `DocsUrl`).
4. Ask for credentials (the database connection string, entered without echo).
5. Create a Kubernetes Secret (`orbitcontainer-secrets`) with the credentials.
6. Replace placeholders (`{{IMAGE_URI}}`, `{{DOCS_URL}}`, `{{NAMESPACE}}`) in `deployment.yaml`.
7. Apply namespace, deployment, service, and ingress manifests in order.
8. Wait for the rollout to complete.
9. Show the deployed pods, services, and ingress resources.
10. Display the application URL.

## Configuration Management

| Variable | Source | Description |
|----------|--------|-------------|
| `ConnectionStrings__DefaultConnection` | Kubernetes Secret / Environment | SQL Server connection string (credential) |
| `DocsUrl` | ConfigMap / Environment | Documentation URL injected into Razor views |
| `ASPNETCORE_ENVIRONMENT` | Environment | Set to `Production` |
| `ASPNETCORE_URLS` | Environment | Set to `http://+:8080` |

In .NET, environment variables with `__` (double underscore) map to `:` in configuration keys. For example, `ConnectionStrings__DefaultConnection` maps to `ConnectionStrings:DefaultConnection`.

## .NET Notes

### Runtime Settings

- **Base Image**: `mcr.microsoft.com/dotnet/runtime:6.0` — the ASP.NET Core shared framework is not present, so the application is published self-contained (`--self-contained true -r linux-x64`).
- **Kestrel**: Configured via `ASPNETCORE_URLS=http://+:8080` to listen on port 8080, as the container runs as a non-root user.
- **Garbage Collector**: `DOTNET_gcServer=true` enables server GC for containerized workloads. `DOTNET_gcConcurrent=true` enables concurrent GC. `DOTNET_GCHeapHardLimit=40000000` caps the managed heap at 1 GB to respect container memory limits.
- **Globalization**: `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` ensures full ICU globalization support.

### Security

- **Non-root User**: The container runs as `appuser` (UID 1000), not root.
- **HTTPS Redirection**: `app.UseHttpsRedirection()` is configured in `Program.cs`. In production behind an ALB, configure the ALB to terminate TLS and forward the `X-Forwarded-Proto` header.
- **Security Headers**: The application uses ASP.NET Core's built-in HSTS middleware (`app.UseHsts()`) in non-development environments.
- **Authentication and Authorization**: The application uses ASP.NET Core Identity with SQL Server. Authentication is configured via `AddDefaultIdentity` and `UseAuthentication`/`UseAuthorization` middleware.
- **CORS**: No CORS policy is configured; the application is intended to be accessed through the Ingress controller.
- **Data Protection**: ASP.NET Core Identity uses data protection keys stored in the default location. In a multi-replica deployment, configure a shared data protection key repository (e.g., via `PersistKeysToFileSystem` pointing to a shared volume, or use a distributed key ring).

### Health Check

The application exposes a `/health` endpoint via `AddHealthChecks()` and `MapHealthChecks("/health")` in `Program.cs`. The Kubernetes liveness and readiness probes use this endpoint on port 8080.

### Static File Caching

Static file middleware is configured with `StaticFileOptions` that set `Cache-Control` headers:
- Fingerprinted assets (e.g., `site.abc12345.css`): cached for 365 days with `immutable`.
- Non-fingerprinted assets: cached for 1 day with `must-revalidate` and `Vary: Accept-Encoding`.

## Troubleshooting

### Pod Failures

```bash
kubectl describe pod -n orbitcontainer <pod-name>
kubectl logs -n orbitcontainer <pod-name>
```

### Service Issues

```bash
kubectl get svc -n orbitcontainer
kubectl describe svc -n orbitcontainer orbitcontainer-service
```

### Ingress Problems

```bash
kubectl describe ingress -n orbitcontainer
kubectl get ingress -n orbitcontainer -o wide
```

Ensure the ALB Ingress Controller is running and has the correct IAM permissions.

## Scaling and Management

### Horizontal Pod Autoscaler (HPA)

```bash
kubectl autoscale deployment orbitcontainer --cpu-percent=70 --min=2 --max=5 -n orbitcontainer
```

### Rolling Updates

```bash
kubectl set image deployment/orbitcontainer orbitcontainer=<new-image-uri> -n orbitcontainer
kubectl rollout status deployment/orbitcontainer -n orbitcontainer
```

### Rollbacks

```bash
kubectl rollout undo deployment/orbitcontainer -n orbitcontainer
```
