@echo off
setlocal enabledelayedexpansion

REM build-push.bat - builds and pushes the OrbitContainer image to AWS ECR or Docker Hub

set "PROJECT_NAME=OrbitContainer"
for /f "delims=" %%i in ('powershell -command "$n = '%PROJECT_NAME%'; $n.ToLower() -replace '[^a-z0-9]+', '-'"') do set "IMAGE_NAME=%%i"
REM trim leading/trailing hyphens via PowerShell
for /f "delims=" %%i in ('powershell -command "$n = '%IMAGE_NAME%'; $n.Trim('-')"') do set "IMAGE_NAME=%%i"

echo === OrbitContainer Image Build ^& Push ===
echo.

REM Ask for the target platform
echo Choose the target registry:
echo 1. AWS ECR
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter choice (1 or 2): "

if "!REGISTRY_CHOICE!"=="1" (
    set /p AWS_REGION="Enter AWS region (e.g. us-east-1): "
    set /p AWS_ACCOUNT_ID="Enter AWS Account ID: "
    set "REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com"
    set "ECR_REPO=!IMAGE_NAME!"

    echo Logging in to AWS ECR...
    aws ecr get-login-password --region !AWS_REGION! | docker login --username AWS --password-stdin !REGISTRY_URL!
    if !ERRORLEVEL! neq 0 (
        echo ECR login failed
        exit /b 1
    )

    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository: !ECR_REPO!
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    ) else (
        echo ECR repository already exists: !ECR_REPO!
    )

    set "FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!"
) else if "!REGISTRY_CHOICE!"=="2" (
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    echo Logging in to Docker Hub...
    docker login --username !DOCKER_USERNAME!
    if !ERRORLEVEL! neq 0 (
        echo Docker Hub login failed
        exit /b 1
    )
    set "FULL_IMAGE_NAME=docker.io/!DOCKER_USERNAME!/!IMAGE_NAME!"
) else (
    echo Invalid choice. Exiting.
    exit /b 1
)

REM Ask for the image tag
set /p IMAGE_TAG="Enter IMAGE_TAG (leave empty for 'latest'): "
if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"

REM Sanitize the tag with PowerShell
for /f "delims=" %%i in ('powershell -command "'!IMAGE_TAG!' -replace '[^a-z0-9.-]+', '-' | ForEach-Object { $_.Trim('-') }"') do set "IMAGE_TAG=%%i"

set "FULL_IMAGE_NAME=!FULL_IMAGE_NAME!:!IMAGE_TAG!"

echo.
echo Building image: !FULL_IMAGE_NAME!
docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .
if !ERRORLEVEL! neq 0 (
    echo Docker build failed
    exit /b 1
)

echo.
echo Pushing image: !FULL_IMAGE_NAME!
docker push "!FULL_IMAGE_NAME!"
if !ERRORLEVEL! neq 0 (
    echo Docker push failed
    exit /b 1
)

echo.
echo === Build and push complete ===
echo Image: !FULL_IMAGE_NAME!

endlocal
