@echo off
setlocal enabledelayedexpansion

echo === OrbitAOS.V6 EKS Deployment Script ===
echo.

set "NAMESPACE=orbita-os-v6"

set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter EKS cluster name: "
set /p IMAGE_URI="Enter image URI (full path with registry and tag, e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/orbita-os-v6:latest): "

echo.
echo Configuring kubectl for EKS cluster: !CLUSTER_NAME! in region !AWS_REGION!...
call aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (
    echo Failed to configure kubectl
    exit /b 1
)

echo Checking cluster connectivity...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo Cluster connectivity check failed
    exit /b 1
)

echo.
echo Applying namespace...
kubectl apply -f kubernetes/namespace.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply namespace
    exit /b 1
)

echo.
echo Enter environment variable values (press Enter to skip):
set /p DB_HOST_VAL="Enter value for DB_HOST (or press Enter to skip): "
set /p DB_PORT_VAL="Enter value for DB_PORT (or press Enter to skip): "
set /p DB_NAME_VAL="Enter value for DB_NAME (or press Enter to skip): "
set /p DOCS_URL_VAL="Enter value for DocsUrl (or press Enter to skip): "

echo.
echo Enter credential values (not echoed):
set /p CONNECTION_STRING_VAL="Enter value for ConnectionStrings__DefaultConnection: "

echo.
echo Creating Kubernetes Secret...
set "SECRET_ARGS="
if not "!CONNECTION_STRING_VAL!"=="" (
    set "SECRET_ARGS=--from-literal=connection-string=\"!CONNECTION_STRING_VAL!\""
)
if not "!DB_HOST_VAL!"=="" (
    if not "!SECRET_ARGS!"=="" (
        set "SECRET_ARGS=!SECRET_ARGS! --from-literal=db-host=\"!DB_HOST_VAL!\""
    ) else (
        set "SECRET_ARGS=--from-literal=db-host=\"!DB_HOST_VAL!\""
    )
)
if not "!DB_PORT_VAL!"=="" (
    if not "!SECRET_ARGS!"=="" (
        set "SECRET_ARGS=!SECRET_ARGS! --from-literal=db-port=\"!DB_PORT_VAL!\""
    ) else (
        set "SECRET_ARGS=--from-literal=db-port=\"!DB_PORT_VAL!\""
    )
)
if not "!DB_NAME_VAL!"=="" (
    if not "!SECRET_ARGS!"=="" (
        set "SECRET_ARGS=!SECRET_ARGS! --from-literal=db-name=\"!DB_NAME_VAL!\""
    ) else (
        set "SECRET_ARGS=--from-literal=db-name=\"!DB_NAME_VAL!\""
    )
)

if not "!SECRET_ARGS!"=="" (
    kubectl create secret generic orbita-os-v6-secrets -n !NAMESPACE! !SECRET_ARGS! --dry-run=client -o yaml | kubectl apply -f -
    if !ERRORLEVEL! neq 0 (
        echo Failed to create secret
        exit /b 1
    )
)

echo.
echo Deploying application...
copy /Y kubernetes\deployment.yaml kubernetes\deployment.yaml.bak >nul
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Out-File -Encoding UTF8 kubernetes\deployment.yaml.tmp"
move /Y kubernetes\deployment.yaml.tmp kubernetes\deployment.yaml >nul
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{DB_HOST}}', '!DB_HOST_VAL!' | Out-File -Encoding UTF8 kubernetes\deployment.yaml.tmp"
move /Y kubernetes\deployment.yaml.tmp kubernetes\deployment.yaml >nul
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{DB_PORT}}', '!DB_PORT_VAL!' | Out-File -Encoding UTF8 kubernetes\deployment.yaml.tmp"
move /Y kubernetes\deployment.yaml.tmp kubernetes\deployment.yaml >nul
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{DB_NAME}}', '!DB_NAME_VAL!' | Out-File -Encoding UTF8 kubernetes\deployment.yaml.tmp"
move /Y kubernetes\deployment.yaml.tmp kubernetes\deployment.yaml >nul
powershell -Command "(gc kubernetes\deployment.yaml) -replace '{{DOCS_URL}}', '!DOCS_URL_VAL!' | Out-File -Encoding UTF8 kubernetes\deployment.yaml.tmp"
move /Y kubernetes\deployment.yaml.tmp kubernetes\deployment.yaml >nul
del /Q kubernetes\deployment.yaml.bak >nul 2>&1

kubectl apply -f kubernetes/deployment.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply deployment
    exit /b 1
)

kubectl apply -f kubernetes/service.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply service
    exit /b 1
)

kubectl apply -f kubernetes/ingress.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply ingress
    exit /b 1
)

echo.
echo Waiting for deployment to roll out...
kubectl rollout status deployment/orbita-os-v6 -n !NAMESPACE!
if !ERRORLEVEL! neq 0 (
    echo Deployment rollout failed
    echo Rollback: kubectl rollout undo deployment/orbita-os-v6 -n !NAMESPACE!
    exit /b 1
)

echo.
echo === Deployment Complete ===
echo.
echo Pods:
kubectl get pods -n !NAMESPACE!
echo.
echo Services:
kubectl get svc -n !NAMESPACE!
echo.
echo Ingress:
kubectl get ingress -n !NAMESPACE!
echo.
echo Application URL: https://orbita-os-v6.example.com
echo.
echo === Rollback Instructions ===
echo If the deployment fails, run:
echo   kubectl rollout undo deployment/orbita-os-v6 -n !NAMESPACE!
echo.
echo To check logs:
echo   kubectl logs -f deployment/orbita-os-v6 -n !NAMESPACE!

exit /b 0
