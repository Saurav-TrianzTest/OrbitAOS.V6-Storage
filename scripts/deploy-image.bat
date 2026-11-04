@echo off
setlocal enabledelayedexpansion
set "PROJECT_NAME=OrbitContainer"
for /f "delims=" %%i in ('powershell -command "$n = '%PROJECT_NAME%'; $n.ToLower() -replace '[^a-z0-9]+', '-'"') do set "IMAGE_NAME=%%i"
for /f "delims=" %%i in ('powershell -command "$n = '%IMAGE_NAME%'; $n.Trim('-')"') do set "IMAGE_NAME=%%i"
set "NAMESPACE=!IMAGE_NAME!"

echo === OrbitContainer Deployment to AWS EKS ===
echo.

REM Ask for AWS region and EKS cluster name
set /p AWS_REGION="Enter AWS region (e.g. us-east-1): "
set /p CLUSTER_NAME="Enter EKS cluster name: "
set /p IMAGE_URI="Enter full image URI (e.g. 123456789.dkr.ecr.us-east-1.amazonaws.com/orbitcontainer:v1.0.0): "

echo.
echo Configuring kubectl for EKS cluster: !CLUSTER_NAME! in !AWS_REGION!
aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (
    echo kubectl configuration failed
    exit /b 1
)

echo.
echo Verifying cluster connectivity...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo Cluster connection failed. Exiting.
    exit /b 1
)

REM Ask for environment variables (non-credential)
echo.
echo Enter values for non-credential environment variables (or press Enter to skip):
set /p DOCS_URL="Enter value for DocsUrl (or press Enter to skip): "

REM Ask for credentials
echo.
echo Enter values for credentials (not echoed):
set /p DB_CONNECTION="Enter ConnectionStrings__DefaultConnection (or press Enter to skip): "

REM Apply namespace
echo.
echo Applying namespace...
kubectl apply -f kubernetes/namespace.yaml
if !ERRORLEVEL! neq 0 (
    echo Namespace application failed
    exit /b 1
)

REM Create Secret for credentials
echo.
echo Creating Secret for credentials...
set "SECRET_ARGS="
if not "!DB_CONNECTION!"=="" (
    set "SECRET_ARGS=--from-literal=ConnectionStrings__DefaultConnection=\"!DB_CONNECTION!\""
)

if not "!SECRET_ARGS!"=="" (
    kubectl create secret generic "!NAMESPACE!-secrets" -n "!NAMESPACE!" !SECRET_ARGS! --dry-run=client -o yaml | kubectl apply -f -
    if !ERRORLEVEL! neq 0 (
        echo Secret creation failed
        exit /b 1
    )
) else (
    echo No credentials provided. Skipping Secret creation.
)

REM Prepare deployment.yaml with placeholders
echo.
echo Preparing deployment manifests...
powershell -Command "(gc kubernetes/deployment.yaml) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Out-File -Encoding UTF8 kubernetes/deployment.yaml"
powershell -Command "(gc kubernetes/deployment.yaml) -replace '{{DOCS_URL}}', '!DOCS_URL!' | Out-File -Encoding UTF8 kubernetes/deployment.yaml"
powershell -Command "(gc kubernetes/deployment.yaml) -replace '{{NAMESPACE}}', '!NAMESPACE!' | Out-File -Encoding UTF8 kubernetes/deployment.yaml"

REM Apply deployment, service, and ingress
echo.
echo Applying deployment...
kubectl apply -f kubernetes/deployment.yaml
if !ERRORLEVEL! neq 0 (
    echo Deployment failed
    exit /b 1
)

echo.
echo Applying service...
kubectl apply -f kubernetes/service.yaml
if !ERRORLEVEL! neq 0 (
    echo Service application failed
    exit /b 1
)

echo.
echo Applying ingress...
kubectl apply -f kubernetes/ingress.yaml
if !ERRORLEVEL! neq 0 (
    echo Ingress application failed
    exit /b 1
)

REM Wait for rollout
echo.
echo Waiting for deployment rollout...
kubectl rollout status deployment/!NAMESPACE! -n !NAMESPACE!
if !ERRORLEVEL! neq 0 (
    echo Rollout failed
    exit /b 1
)

REM Show resources
echo.
echo === Deployed resources ===
kubectl get pods,svc,ingress -n !NAMESPACE!

REM Show application URL
echo.
echo === Application URL ===
echo https://!NAMESPACE!.example.com

echo.
echo === Deployment complete ===
echo To rollback: kubectl rollout undo deployment/!NAMESPACE! -n !NAMESPACE!

endlocal
