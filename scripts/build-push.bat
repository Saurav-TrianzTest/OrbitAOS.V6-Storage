@echo off
setlocal enabledelayedexpansion

echo ========================================
echo    Docker Build and Push Script
echo ========================================
echo.

REM Project configuration
set "PROJECT_NAME=TestSanity"
set "SANITIZED_NAME=testsanity"
echo Project: %PROJECT_NAME%
echo Sanitized name: %SANITIZED_NAME%
echo.

REM Prompt for image tag
set /p "IMAGE_TAG=Enter image tag (default: latest): "
if "!IMAGE_TAG!"==" " set "IMAGE_TAG=latest"
if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"
echo Using tag: !IMAGE_TAG!
echo.

REM Registry selection
echo Select Docker Registry:
echo   1. AWS ECR (Elastic Container Registry)
echo   2. Docker Hub
set /p "REGISTRY_CHOICE=Enter choice (1 or 2): "
echo.

if "!REGISTRY_CHOICE!"=="1" (
    echo === AWS ECR Configuration ===
    set /p "AWS_REGION=Enter AWS Region (e.g., us-east-1): "
    set /p "AWS_ACCOUNT_ID=Enter AWS Account ID: "
    set /p "ECR_REPO=Enter ECR Repository Name (default: %SANITIZED_NAME%): "
    if "!ECR_REPO!"=="" set "ECR_REPO=%SANITIZED_NAME%"
    
    set "REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com"
    set "FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!"
    
    echo.
    echo Authenticating with AWS ECR...
    for /f "delims=" %%p in ('aws ecr get-login-password --region !AWS_REGION!') do set "ECR_PASSWORD=%%p"
    echo !ECR_PASSWORD! | docker login --username AWS --password-stdin !REGISTRY_URL!
    
    if !ERRORLEVEL! neq 0 (
        echo ERROR: ECR authentication failed
        exit /b 1
    )
    
    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Repository does not exist. Creating ECR repository: !ECR_REPO!
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
        if !ERRORLEVEL! neq 0 (
            echo ERROR: Failed to create ECR repository
            exit /b 1
        )
    )
    
) else if "!REGISTRY_CHOICE!"=="2" (
    echo === Docker Hub Configuration ===
    set /p "DOCKER_USERNAME=Enter Docker Hub Username: "
    set /p "DOCKER_PASSWORD=Enter Docker Hub Password or Token: "
    set /p "DOCKER_REPO=Enter Repository Name (default: %SANITIZED_NAME%): "
    if "!DOCKER_REPO!"=="" set "DOCKER_REPO=%SANITIZED_NAME%"
    
    set "FULL_IMAGE_NAME=!DOCKER_USERNAME!/!DOCKER_REPO!:!IMAGE_TAG!"
    
    echo.
    echo Authenticating with Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Docker Hub authentication failed
        exit /b 1
    )
) else (
    echo ERROR: Invalid choice. Please select 1 or 2.
    exit /b 1
)

echo.
echo === Building Docker Image ===
echo Image: !FULL_IMAGE_NAME!
echo.

docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .

if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker build failed
    exit /b 1
)

echo.
echo === Pushing Docker Image ===
docker push "!FULL_IMAGE_NAME!"

if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker push failed
    exit /b 1
)

echo.
echo ========================================
echo    Build and Push Completed Successfully
echo ========================================
echo Image: !FULL_IMAGE_NAME!
echo.

endlocal