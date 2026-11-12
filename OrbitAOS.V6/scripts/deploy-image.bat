@echo off
setlocal enabledelayedexpansion

set PROJECT_NAME=OrbitAOS.V6
for /f "usebackq delims=" %%i in (`powershell -Command "$p='OrbitAOS.V6'; ($p.ToLower() -replace '[^a-z0-9]', '-') -replace '^-+|-+$',''"`) do set IMAGE_NAME=%%i
set NAMESPACE=!IMAGE_NAME!

echo === Deploy to AWS EKS: !IMAGE_NAME! ===
echo.

set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter EKS cluster name: "

echo.
echo === Configuring kubectl for EKS cluster: !CLUSTER_NAME! ===
call aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (
    echo Failed to configure kubectl
    exit /b 1
)

echo.
echo === Verifying cluster connectivity ===
call kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo Cluster connectivity check failed
    exit /b 1
)

set /p IMAGE_URI="Enter the full image URI (e.g., 123456789012.dkr.ecr.us-east-1.amazonaws.com/orbitaos-v6:latest): "

echo.
echo === Enter environment variable values (press Enter to skip) ===
set /p HOME_DOC_URL="Enter value for HOME__DOCUMENTATIONURL (or press Enter to skip): "

echo.
echo === Enter credential values (not echoed) ===
set /p DB_CONNECTION_STRING="Enter SQL Server connection string (or press Enter to skip): "

echo.
echo === Applying namespace ===
call kubectl apply -f kubernetes/namespace.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply namespace
    exit /b 1
)

echo.
echo === Creating Kubernetes Secret ===
set SECRET_ARGS=
if not "!DB_CONNECTION_STRING!"=="" (
    set SECRET_ARGS=--from-literal=connection-string="!DB_CONNECTION_STRING!"
)

if not "!SECRET_ARGS!"=="" (
    call kubectl create secret generic !NAMESPACE!-secrets -n !NAMESPACE! !SECRET_ARGS! --dry-run=client -o yaml | kubectl apply -f -
    if !ERRORLEVEL! neq 0 (
        echo Failed to create secret
        exit /b 1
    )
) else (
    echo No credentials provided. Skipping Secret creation.
)

echo.
echo === Replacing placeholders in deployment.yaml ===
copy /y kubernetes\deployment.yaml kubernetes\deployment.yaml.bak >nul
powershell -Command "(Get-Content kubernetes\deployment.yaml) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content kubernetes\deployment.yaml"
if not "!HOME_DOC_URL!"=="" (
    powershell -Command "(Get-Content kubernetes\deployment.yaml) -replace '{{HOME__DOCUMENTATIONURL}}', '!HOME_DOC_URL!' | Set-Content kubernetes\deployment.yaml"
) else (
    powershell -Command "(Get-Content kubernetes\deployment.yaml) -replace '{{HOME__DOCUMENTATIONURL}}', 'https://docs.microsoft.com/aspnet/core' | Set-Content kubernetes\deployment.yaml"
)
del /q kubernetes\deployment.yaml.bak >nul

echo.
echo === Applying deployment ===
call kubectl apply -f kubernetes/deployment.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply deployment
    exit /b 1
)

echo.
echo === Applying service ===
call kubectl apply -f kubernetes/service.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply service
    exit /b 1
)

echo.
echo === Applying ingress ===
call kubectl apply -f kubernetes/ingress.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply ingress
    exit /b 1
)

echo.
echo === Waiting for deployment rollout ===
call kubectl rollout status deployment/!NAMESPACE! -n !NAMESPACE!
if !ERRORLEVEL! neq 0 (
    echo Rollout failed
    exit /b 1
)

echo.
echo === Deployment status ===
call kubectl get pods,svc,ingress -n !NAMESPACE!

echo.
echo === Application URL ===
echo https://!NAMESPACE!.example.com

echo.
echo === Deployment complete ===
echo If you need to rollback, run:
echo   kubectl rollout undo deployment/!NAMESPACE! -n !NAMESPACE!

exit /b 0
