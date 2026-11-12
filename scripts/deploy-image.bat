@echo off
setlocal enabledelayedexpansion

set NAMESPACE=orbitaaos-v6
set APP_NAME=orbitaaos-v6

echo === Deploy %APP_NAME% to AWS EKS ===
echo.

set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter EKS cluster name: "
set /p IMAGE_URI="Enter full image URI (registry/repo:tag): "

echo.
echo === Configuring kubectl for cluster %CLUSTER_NAME% in %AWS_REGION% ===
aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (
    echo Failed to configure kubectl
    exit /b 1
)

echo Verifying cluster connectivity...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo Cluster connectivity check failed
    exit /b 1
)

echo.
echo === Collecting environment variable values ===
echo Enter values for environment variables (or press Enter to skip):
set /p DB_CONN_VAL="Enter value for ConnectionStrings__DefaultConnection (or press Enter to skip): "
set /p DOCS_URL_VAL="Enter value for DocsUrl (or press Enter to skip): "

echo.
echo === Collecting credential values (not echoed) ===
set /p DB_PASSWORD_VAL="Enter value for DB_PASSWORD (or press Enter to skip): "

REM Build secret arguments
set SECRET_ARGS=
if not "!DB_PASSWORD_VAL!"=="" (
    set SECRET_ARGS=!SECRET_ARGS! --from-literal=DB_PASSWORD=!DB_PASSWORD_VAL!
)

echo.
echo === Creating Kubernetes Secret ===
if not "!SECRET_ARGS!"=="" (
    kubectl create secret generic !APP_NAME!-secrets -n !NAMESPACE! !SECRET_ARGS! --dry-run=client -o yaml | kubectl apply -f -
    if !ERRORLEVEL! neq 0 (
        echo Failed to create secret
        exit /b 1
    )
    echo Secret !APP_NAME!-secrets created/updated.
) else (
    echo No credentials provided. Skipping secret creation.
)

echo.
echo === Applying Kubernetes manifests ===

echo Applying namespace...
kubectl apply -f kubernetes\namespace.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply namespace
    exit /b 1
)

echo Applying deployment...
REM Replace placeholders in deployment.yaml using PowerShell
powershell -Command "(Get-Content kubernetes\deployment.yaml) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content kubernetes\deployment.yaml"
powershell -Command "(Get-Content kubernetes\deployment.yaml) -replace '{{NAMESPACE}}', '!NAMESPACE!' | Set-Content kubernetes\deployment.yaml"
if not "!DB_CONN_VAL!"=="" (
    powershell -Command "(Get-Content kubernetes\deployment.yaml) -replace '{{DB_CONNECTION_STRING}}', '!DB_CONN_VAL!' | Set-Content kubernetes\deployment.yaml"
)
if not "!DOCS_URL_VAL!"=="" (
    powershell -Command "(Get-Content kubernetes\deployment.yaml) -replace '{{DOCS_URL}}', '!DOCS_URL_VAL!' | Set-Content kubernetes\deployment.yaml"
)

kubectl apply -f kubernetes\deployment.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply deployment
    exit /b 1
)

echo Applying service...
kubectl apply -f kubernetes\service.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply service
    exit /b 1
)

echo Applying ingress...
kubectl apply -f kubernetes\ingress.yaml
if !ERRORLEVEL! neq 0 (
    echo Failed to apply ingress
    exit /b 1
)

echo.
echo === Waiting for deployment rollout ===
kubectl rollout status deployment\!APP_NAME! -n !NAMESPACE!
if !ERRORLEVEL! neq 0 (
    echo Deployment rollout failed
    exit /b 1
)

echo.
echo === Deployment Status ===
kubectl get pods,svc,ingress -n !NAMESPACE!

echo.
echo === Application URL ===
for /f "delims=" %%i in ('kubectl get ingress !APP_NAME!-ingress -n !NAMESPACE! -o jsonpath="{.status.loadBalancer.ingress[0].hostname}" 2^>nul') do set INGRESS_HOST=%%i
if "!INGRESS_HOST!"=="" (
    for /f "delims=" %%i in ('kubectl get ingress !APP_NAME!-ingress -n !NAMESPACE! -o jsonpath="{.status.loadBalancer.ingress[0].ip}" 2^>nul') do set INGRESS_HOST=%%i
)
if "!INGRESS_HOST!"=="" set INGRESS_HOST=pending
echo Application URL: http://!INGRESS_HOST!

echo.
echo === Deployment Complete ===
echo If the deployment fails, rollback with:
echo   kubectl rollout undo deployment\!APP_NAME! -n !NAMESPACE!

endlocal
