@echo off
setlocal enabledelayedexpansion

echo === OrbitAOS.V6 EKS Deployment ===
echo.

set "PROJECT_NAME=OrbitAOS.V6"
REM Sanitize project name
powershell -NoProfile -Command "$n='%PROJECT_NAME%'; $n=$n.ToLower(); $n=$n -creplace '[^a-z0-9]', '-'; $n=$n.Trim('-'); Write-Output $n" > "%TEMP%\projectname.txt"
set /p NAME=<"%TEMP%\projectname.txt"
del "%TEMP%\projectname.txt"

set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter EKS cluster name: "
set /p IMAGE_URI="Enter full image URI (with registry and tag): "

echo.
echo Configuring kubectl for EKS cluster '%CLUSTER_NAME%' in region '%AWS_REGION%'...
call aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (echo Failed to configure kubectl & exit /b 1)

echo Verifying cluster connectivity...
call kubectl cluster-info
if !ERRORLEVEL! neq 0 (echo Cluster connectivity check failed & exit /b 1)

echo.
echo Enter values for environment variables (press Enter to skip):
set /p APPURLS_VALUE="Enter value for AppUrls__Documentation (or press Enter to skip): "

echo.
echo Enter credential values (not echoed):
call cmd /c "powershell -NoProfile -Command "$p = Read-Host 'Enter value for ConnectionStrings__DefaultConnection (database connection string)'; Write-Output $p"" > "%TEMP%\dbconn.txt"
set /p DB_CONN_VALUE=<"%TEMP%\dbconn.txt"
del "%TEMP%\dbconn.txt"

echo.
echo Applying namespace...
call kubectl apply -f kubernetes/namespace.yaml
if !ERRORLEVEL! neq 0 (echo Failed to apply namespace & exit /b 1)

echo.
echo Creating Kubernetes Secret with credentials...
if not "!DB_CONN_VALUE!"=="" (
    call kubectl create secret generic !NAME!-secrets -n !NAME! --from-literal=connection-string="!DB_CONN_VALUE!" --dry-run=client -o yaml | kubectl apply -f -
    if !ERRORLEVEL! neq 0 (echo Failed to create secret & exit /b 1)
)

echo.
echo Applying deployment...
copy /Y kubernetes\deployment.yaml kubernetes\deployment.yaml.bak >nul
call powershell -NoProfile -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content kubernetes/deployment.yaml"
call powershell -NoProfile -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{APPURLS__DOCUMENTATION}}', '!APPURLS_VALUE!' | Set-Content kubernetes/deployment.yaml"
call powershell -NoProfile -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{NAMESPACE}}', '!NAME!' | Set-Content kubernetes/deployment.yaml"
call kubectl apply -f kubernetes/deployment.yaml
if !ERRORLEVEL! neq 0 (echo Failed to apply deployment & exit /b 1)
del /Q kubernetes\deployment.yaml.bak >nul 2>&1

echo Applying service...
call kubectl apply -f kubernetes/service.yaml
if !ERRORLEVEL! neq 0 (echo Failed to apply service & exit /b 1)

echo Applying ingress...
call kubectl apply -f kubernetes/ingress.yaml
if !ERRORLEVEL! neq 0 (echo Failed to apply ingress & exit /b 1)

echo.
echo Waiting for deployment rollout...
call kubectl rollout status deployment/!NAME! -n !NAME!
if !ERRORLEVEL! neq 0 (echo Rollout failed & exit /b 1)

echo.
echo === Deployment Status ===
call kubectl get pods,svc,ingress -n !NAME!

echo.
echo === Application URL ===
echo https://!NAME!.example.com

echo.
echo === Deployment Complete ===
echo If rollback is needed, run:
echo   kubectl rollout undo deployment/!NAME! -n !NAME!

exit /b 0
