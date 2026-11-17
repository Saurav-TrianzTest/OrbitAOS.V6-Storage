@echo off
setlocal enabledelayedexpansion

echo === OrbitAOS.V6 Build and Push Script ===
echo.

set "PROJECT_NAME=OrbitAOS.V6"
REM Convert to lowercase and replace non-alphanumeric with hyphens
for /f "delims=" %%i in ('powershell -Command "$n='%PROJECT_NAME%'; ($n.ToLower() -replace '[^a-z0-9]+', '-').Trim('-')"') do set "IMAGE_NAME=%%i"

echo Project name: %PROJECT_NAME%
echo Image name: %IMAGE_NAME%
echo.

echo Choose target platform:
echo 1. AWS ECR
echo 2. Docker Hub
echo.
set /p PLATFORM_CHOICE="Enter choice (1 or 2): "

if "%PLATFORM_CHOICE%"=="1" (
    set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
    set /p ECR_REPO_INPUT="Enter ECR repository name (or press Enter for default): "
    if "!ECR_REPO_INPUT!"=="" (
        set "ECR_REPO=%IMAGE_NAME%"
    ) else (
        set "ECR_REPO=!ECR_REPO_INPUT!"
    )
    set "REGISTRY_URL=!AWS_REGION!.dkr.ecr.!AWS_REGION!.amazonaws.com"
    set "FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!"

    set /p IMAGE_TAG="Enter image tag (or press Enter for latest): "
    if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"

    echo.
    echo Building and pushing to AWS ECR...
    echo Registry: !REGISTRY_URL!
    echo Repository: !ECR_REPO!
    echo Full image name: !FULL_IMAGE_NAME!:!IMAGE_TAG!
    echo.

    echo Logging in to AWS ECR...
    call aws ecr get-login-password --region !AWS_REGION! | docker login --username AWS --password-stdin !REGISTRY_URL!
    if !ERRORLEVEL! neq 0 (
        echo ECR login failed
        exit /b 1
    )

    echo Checking if ECR repository exists...
    call aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository...
        call aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    )

    echo Building Docker image...
    docker build -f Dockerfile -t "!FULL_IMAGE_NAME!:!IMAGE_TAG!" .
    if !ERRORLEVEL! neq 0 (
        echo Docker build failed
        exit /b 1
    )

    echo Pushing Docker image...
    docker push "!FULL_IMAGE_NAME!:!IMAGE_TAG!"
    if !ERRORLEVEL! neq 0 (
        echo Docker push failed
        exit /b 1
    )

    echo.
    echo === Build and push complete ===
    echo Image: !FULL_IMAGE_NAME!:!IMAGE_TAG!
) else if "%PLATFORM_CHOICE%"=="2" (
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub password: "
    set /p DOCKER_REPO_INPUT="Enter Docker Hub repository name (or press Enter for default): "
    if "!DOCKER_REPO_INPUT!"=="" (
        set "DOCKER_REPO=%IMAGE_NAME%"
    ) else (
        set "DOCKER_REPO=!DOCKER_REPO_INPUT!"
    )
    set "FULL_IMAGE_NAME=docker.io/%DOCKER_USERNAME%/%DOCKER_REPO%"

    set /p IMAGE_TAG="Enter image tag (or press Enter for latest): "
    if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"

    echo.
    echo Building and pushing to Docker Hub...
    echo Repository: !FULL_IMAGE_NAME!
    echo Tag: !IMAGE_TAG!
    echo.

    echo Logging in to Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    if !ERRORLEVEL! neq 0 (
        echo Docker Hub login failed
        exit /b 1
    )

    echo Building Docker image...
    docker build -f Dockerfile -t "!FULL_IMAGE_NAME!:!IMAGE_TAG!" .
    if !ERRORLEVEL! neq 0 (
        echo Docker build failed
        exit /b 1
    )

    echo Pushing Docker image...
    docker push "!FULL_IMAGE_NAME!:!IMAGE_TAG!"
    if !ERRORLEVEL! neq 0 (
        echo Docker push failed
        exit /b 1
    )

    echo.
    echo === Build and push complete ===
    echo Image: !FULL_IMAGE_NAME!:!IMAGE_TAG!
) else (
    echo Invalid choice. Exiting.
    exit /b 1
)

exit /b 0
