# Deployment Guide — OrbitAOS.V6 on AWS EKS

## Prerequisites

- Docker (for building and pushing images)
- .NET 6.0 SDK (for local development)
- AWS CLI (for ECR and EKS)
- kubectl (for Kubernetes)
- eksctl (for EKS cluster management, optional)
- IAM permissions for ECR and EKS

## System Requirements

- **Runtime**: .NET 6.0 (self-contained deployment, linux-x64)
- **Base image**: `mcr.microsoft.com/dotnet/runtime:6.0`
- **Port**: 8080 (ASPNETCORE_URLS=http://+:8080)
- **Health endpoint**: `/health`
- **Memory**: 512Mi requests, 1Gi limits
- **CPU**: 250m requests, 500m limits

## Local Setup with Docker Compose

1. Ensure Docker is running.
2. Build and run locally:
   ```bash
   docker-compose up --build
   ```
3. Access the application at `http://localhost:8080`
4. Health check: `http://localhost:8080/health`

### Environment Variables

| Variable | Description | Default |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | .NET environment | `Production` |
| `ASPNETCORE_URLS` | Kestrel binding | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | (from appsettings.json) |
| `HOME__DOCUMENTATIONURL` | Documentation URL shown on home page | `https://docs.microsoft.com/aspnet/core` |

## Building and Pushing to AWS ECR

### Using build-push.sh (Linux/macOS)

```bash
./scripts/build-push.sh
```

The script will:
1. Ask you to select the target platform (AWS ECR or Docker Hub)
2. For AWS ECR: ask for AWS region, registry URL, repository name, and image tag
3. Log in to AWS ECR
4. Create the ECR repository if it does not exist
5. Build the Docker image with `docker build -f Dockerfile -t <registry>/<repo>:<tag> .`
6. Push the image to ECR

### Using build-push.bat (Windows)

```cmd
scripts\build-push.bat
```

## Deploying to AWS EKS

### Prerequisites

1. Install the AWS CLI and configure with `aws configure`
2. Install kubectl: `aws eks update-kubeconfig` requires kubectl
3. Ensure your IAM user has permissions for EKS and ECR

### EKS Cluster Setup

If you do not have an EKS cluster:

```bash
eksctl create cluster --name <cluster-name> --region us-east-1 --nodegroup-name standard-workers --node-type t3.medium --nodes 2 --nodes-max 3
```

### Deployment Steps

1. **Configure kubectl**:
   ```bash
   aws eks update-kubeconfig --region <aws-region> --name <cluster-name>
   ```

2. **Run the deployment script**:
   ```bash
   ./scripts/deploy-image.sh
   ```

The script will:
1. Ask for AWS region and EKS cluster name
2. Configure kubectl for the cluster
3. Ask for the full image URI (from ECR)
4. Ask for environment variable values (HOME__DOCUMENTATIONURL)
5. Ask for credential values (connection string, not echoed)
6. Apply the namespace
7. Create a Kubernetes Secret for credentials
8. Replace placeholders in deployment.yaml
9. Apply deployment, service, and ingress
10. Wait for rollout to complete
11. Show pod, service, and ingress status

### Manual Deployment

If you prefer to deploy manually:

```bash
# Configure kubectl
aws eks update-kubeconfig --region us-east-1 --name <cluster-name>

# Apply namespace
kubectl apply -f kubernetes/namespace.yaml

# Create secret for credentials
kubectl create secret generic orbitaos-v6-secrets -n orbitaos-v6 \
  --from-literal=connection-string="Server=..." \
  --dry-run=client -o yaml | kubectl apply -f -

# Replace placeholders in deployment.yaml
sed -i.bak 's|{{IMAGE_URI}}|<your-ecr-image-uri>|g' kubernetes/deployment.yaml
sed -i.bak 's|{{HOME__DOCUMENTATIONURL}}|<your-doc-url>|g' kubernetes/deployment.yaml
rm kubernetes/deployment.yaml.bak

# Apply manifests
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# Wait for rollout
kubectl rollout status deployment/orbitaos-v6 -n orbitaos-v6
```

## Configuration Management

### Kubernetes Secrets

The application's credentials (SQL Server connection string) are stored in a Kubernetes Secret named `orbitaos-v6-secrets` in the `orbitaos-v6` namespace. The secret is created by the deploy script.

### Environment Variables

Non-credential configuration is passed as environment variables in the deployment manifest. The `HOME__DOCUMENTATIONURL` variable uses the `{{HOME__DOCUMENTATIONURL}}` placeholder, which is replaced by the deploy script.

### Connection String

The SQL Server connection string is read from the `ConnectionStrings:DefaultConnection` configuration key. In Kubernetes, this is set via the `ConnectionStrings__DefaultConnection` environment variable, mapped from the Kubernetes Secret.

## Kubernetes Manifests

### namespace.yaml
Creates the `orbitaos-v6` namespace for resource isolation.

### deployment.yaml
Deploys 2 replicas of the application with:
- Resource requests (250m CPU, 512Mi memory) and limits (500m CPU, 1Gi memory)
- Liveness and readiness probes on `/health`
- Environment variables from ConfigMap-style placeholders and Secrets
- Container port 8080

### service.yaml
Exposes the application internally via ClusterIP on port 80.

### ingress.yaml
Creates an AWS ALB ingress with:
- Internet-facing scheme
- Host: `orbitaos-v6.example.com`
- Health check path: `/health`

## Troubleshooting

### Pod Failures

- **ImagePullBackOff**: Verify the image URI is correct and the ECR repository exists. Ensure you are logged in to ECR.
- **CrashLoopBackOff**: Check pod logs with `kubectl logs <pod-name> -n orbitaos-v6`. Common causes include missing environment variables or connection string issues.
- **Pending pods**: Check resource constraints with `kubectl describe pod <pod-name> -n orbitaos-v6`.

### Service Issues

- **Connection refused**: Verify the service selector matches the deployment labels.
- **Port mismatch**: Ensure the service targetPort (8080) matches the container port.

### Ingress Problems

- **502 Bad Gateway**: Wait for the ALB to be provisioned. Check the ingress status with `kubectl describe ingress -n orbitaos-v6`.
- **DNS not resolving**: The ingress host `orbitaos-v6.example.com` is a placeholder. Update it with your actual domain and configure DNS.

### Rollback

To rollback a failed deployment:

```bash
kubectl rollout undo deployment/orbitaos-v6 -n orbitaos-v6
kubectl rollout status deployment/orbitaos-v6 -n orbitaos-v6
```

## Scaling and Management

### Horizontal Pod Autoscaler (HPA)

To enable auto-scaling based on CPU utilization:

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: orbitaos-v6-hpa
  namespace: orbitaos-v6
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: orbitaos-v6
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 60
```

### Rolling Updates

The deployment uses a rolling update strategy by default. To perform a manual update:

```bash
# Update the image
kubectl set image deployment/orbitaos-v6 orbitaos-v6=<new-image-uri> -n orbitaos-v6

# Or edit the deployment
kubectl edit deployment orbitaos-v6 -n orbitaos-v6
```

## .NET Notes

### Self-Contained Deployment

This application is published as a self-contained deployment targeting `linux-x64`. The runtime image `mcr.microsoft.com/dotnet/runtime:6.0` does not include the ASP.NET Core shared framework, so the application bundles its own runtime. This increases image size but ensures compatibility.

### Runtime Settings

- **GC Server mode**: Enabled (`DOTNET_GCServer=1`) for better throughput in containerized environments
- **GC heap hard limit**: Set to 2GB (`DOTNET_GCHeapHardLimit=80000000`) to fit within the 1Gi memory limit
- **Kestrel**: Configured via `ASPNETCORE_URLS=http://+:8080` to listen on all interfaces on port 8080
- **Environment**: `ASPNETCORE_ENVIRONMENT=Production` for production settings

### Security Considerations

- **Non-root user**: The container runs as a non-root user (UID 1000) created during the Docker build
- **HTTPS redirection**: Enabled via `app.UseHttpsRedirection()` in Program.cs
- **Security headers**: HSTS is configured for non-development environments
- **Authentication**: ASP.NET Core Identity is used for authentication and authorization
- **CORS**: Not explicitly configured; the application uses same-origin policies for API calls
- **Data protection**: ASP.NET Core Data Protection is used for Identity; in production, configure a persistent data protection provider (e.g., via Azure Blob Storage or a shared Kubernetes volume)
- **Secrets management**: All credentials are stored in Kubernetes Secrets, never in configuration files or environment variable defaults

### Static File Caching

Static files are served with cache-control headers:
- `Cache-Control: public, max-age=86400, must-revalidate` (1 day)
- `Vary: Accept-Encoding` for proper CDN caching

### CSS Optimization

The Dockerfile includes a PurgeCSS build stage that strips unused CSS from Bootstrap, reducing the container image size and improving pod startup times.

### Health Check Endpoint

The application exposes a health check endpoint at `/health` using ASP.NET Core's built-in health check middleware. This is used by Kubernetes for liveness and readiness probes.
