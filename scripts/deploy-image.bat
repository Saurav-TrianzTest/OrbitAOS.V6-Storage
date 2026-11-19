@echo off
setlocal enabledelayedexpansion

echo === OrbitContainerFix Deploy to AWS EKS ===

set "PROJECT_NAME=OrbitContainerFix"
for /f "usebackq delims=" %%i in (`powershell -command "$p='%PROJECT_NAME%'; ($p.ToLower() -replace '[^a-z0-9]', '-') -replace '^-+|-+$',''"`) do set "IMAGE_NAME=%%i"
set "NAMESPACE=!IMAGE_NAME!"

set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter EKS cluster name: "
set /p IMAGE_URI="Enter the full image URI (registry/repo:tag): "

echo.
echo Enter values for environment variables (or press Enter to skip):
set /p DB_CONNECTION_STRING="Enter value for DB_CONNECTION_STRING (or press Enter to skip): "
set /p DOCS_URL="Enter value for DOCS_URL (or press Enter to skip): "

echo Creating Secret with DB connection string...
if "!DB_CONNECTION_STRING!"=="" (
    kubectl create secret generic !NAMESPACE!-secrets -n !NAMESPACE! --from-literal=DB_CONNECTION_STRING="" --dry-run=client -o yaml | kubectl apply -f -
) else (
    kubectl create secret generic !NAMESPACE!-secrets -n !NAMESPACE! --from-literal=DB_CONNECTION_STRING="!DB_CONNECTION_STRING!" --dry-run=client -o yaml | kubectl apply -f -
)
if !ERRORLEVEL! neq 0 (echo Failed to create Secret & exit /b 1)

echo Configuring kubectl for EKS cluster: !CLUSTER_NAME! in region !AWS_REGION!
aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (echo Failed to configure kubectl & exit /b 1)

echo Verifying cluster connectivity...
kubectl cluster-info
if !ERRORLEVEL! neq 0 exit /b 1

echo Applying namespace...
kubectl apply -f kubernetes/namespace.yaml
if !ERRORLEVEL! neq 0 (echo Failed to apply namespace & exit /b 1)

echo Replacing placeholders in deployment.yaml...
powershell -Command "(gc kubernetes/deployment.yaml) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Out-File -encoding UTF8 kubernetes/deployment.yaml"
powershell -Command "(gc kubernetes/deployment.yaml) -replace '{{DOCS_URL}}', '!DOCS_URL!' | Out-File -encoding UTF8 kubernetes/deployment.yaml"

echo Applying deployment...
kubectl apply -f kubernetes/deployment.yaml
if !ERRORLEVEL! neq 0 (echo Failed to apply deployment & exit /b 1)

echo Applying service...
kubectl apply -f kubernetes/service.yaml
if !ERRORLEVEL! neq 0 (echo Failed to apply service & exit /b 1)

echo Applying ingress...
kubectl apply -f kubernetes/ingress.yaml
if !ERRORLEVEL! neq 0 (echo Failed to apply ingress & exit /b 1)

echo Waiting for deployment rollout...
kubectl rollout status deployment/!IMAGE_NAME! -n !NAMESPACE!
if !ERRORLEVEL! neq 0 (echo Deployment rollout failed & exit /b 1)

echo Deployment complete. Current resources:
kubectl get pods,svc,ingress -n !NAMESPACE!

echo.
echo Application URL: https://!NAMESPACE!.example.com
echo.
echo Rollback instructions:
echo   kubectl rollout undo deployment/!IMAGE_NAME! -n !NAMESPACE!
echo   kubectl rollout status deployment/!IMAGE_NAME! -n !NAMESPACE!

endlocal
exit /b 0
