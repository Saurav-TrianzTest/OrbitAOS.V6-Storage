@echo off
setlocal enabledelayedexpansion

echo === OrbitContainerFix Build and Push Script ===

set "PROJECT_NAME=OrbitContainerFix"
REM Sanitize: lowercase and replace non-alphanumeric with hyphen
for /f "usebackq delims=" %%i in (`powershell -command "$p='%PROJECT_NAME%'; ($p.ToLower() -replace '[^a-z0-9]', '-') -replace '^-+|-+$',''"`) do set "IMAGE_NAME=%%i"

echo Select target platform:
echo 1. AWS ECR
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter choice (1 or 2): "

if "!REGISTRY_CHOICE!"=="1" (
    set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
    set /p REGISTRY_URL="Enter AWS ECR registry URL (e.g., 123456789012.dkr.ecr.us-east-1.amazonaws.com): "
    set /p ECR_REPO="Enter ECR repository name: "
    echo IMAGE_TAG first
    set /p IMAGE_TAG="Enter image tag (or press Enter for 'latest'): "
    if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"
    for /f "usebackq delims=" %%i in (`powershell -command "$t='!IMAGE_TAG!'; ($t -replace '[^a-z0-9._-]', '-') -replace '^-+|-+$',''"`) do set "IMAGE_TAG=%%i"
    set "FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!"

    echo Logging in to AWS ECR...
    for /f "usebackq delims=" %%i in (`aws ecr get-login-password --region !AWS_REGION!`) do set "ECR_PASSWORD=%%i"
    echo !ECR_PASSWORD! | docker login --username AWS --password-stdin !REGISTRY_URL!
    if !ERRORLEVEL! neq 0 (echo ECR login failed & exit /b 1)

    echo Creating ECR repository if it does not exist...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository...
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    )

    echo Building Docker image: !FULL_IMAGE_NAME!
    docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .
    if !ERRORLEVEL! neq 0 (echo Docker build failed & exit /b 1)

    echo Pushing Docker image: !FULL_IMAGE_NAME!
    docker push "!FULL_IMAGE_NAME!"
    if !ERRORLEVEL! neq 0 (echo Docker push failed & exit /b 1)

    echo Done! Image pushed to: !FULL_IMAGE_NAME!
) else if "!REGISTRY_CHOICE!"=="2" (
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    set /p "DOCKER_PASSWORD=" < con:
    echo IMAGE_TAG first
    set /p IMAGE_TAG="Enter image tag (or press Enter for 'latest'): "
    if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"
    for /f "usebackq delims=" %%i in (`powershell -command "$t='!IMAGE_TAG!'; ($t -replace '[^a-z0-9._-]', '-') -replace '^-+|-+$',''"`) do set "IMAGE_TAG=%%i"
    set "FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!"

    echo Logging in to Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    if !ERRORLEVEL! neq 0 (echo Docker Hub login failed & exit /b 1)

    echo Building Docker image: !FULL_IMAGE_NAME!
    docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .
    if !ERRORLEVEL! neq 0 (echo Docker build failed & exit /b 1)

    echo Pushing Docker image: !FULL_IMAGE_NAME!
    docker push "!FULL_IMAGE_NAME!"
    if !ERRORLEVEL! neq 0 (echo Docker push failed & exit /b 1)

    echo Done! Image pushed to: !FULL_IMAGE_NAME!
) else (
    echo Invalid choice. Exiting.
    exit /b 1
)

endlocal
exit /b 0
