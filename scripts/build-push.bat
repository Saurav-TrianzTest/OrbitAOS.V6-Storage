@echo off
setlocal enabledelayedexpansion

set PROJECT_NAME=OrbitAOS.V6
set IMAGE_NAME=%PROJECT_NAME%
REM Sanitize: lowercase and replace non-alphanumeric with hyphen
set IMAGE_NAME=%IMAGE_NAME:a=a%
set IMAGE_NAME=%IMAGE_NAME:b=b%
set IMAGE_NAME=%IMAGE_NAME:c=c%
set IMAGE_NAME=%IMAGE_NAME:d=d%
set IMAGE_NAME=%IMAGE_NAME:e=e%
set IMAGE_NAME=%IMAGE_NAME:f=f%
set IMAGE_NAME=%IMAGE_NAME:g=g%
set IMAGE_NAME=%IMAGE_NAME:h=h%
set IMAGE_NAME=%IMAGE_NAME:i=i%
set IMAGE_NAME=%IMAGE_NAME:j=j%
set IMAGE_NAME=%IMAGE_NAME:k=k%
set IMAGE_NAME=%IMAGE_NAME:l=l%
set IMAGE_NAME=%IMAGE_NAME:m=m%
set IMAGE_NAME=%IMAGE_NAME:n=n%
set IMAGE_NAME=%IMAGE_NAME:o=o%
set IMAGE_NAME=%IMAGE_NAME:p=p%
set IMAGE_NAME=%IMAGE_NAME:q=q%
set IMAGE_NAME=%IMAGE_NAME:r=r%
set IMAGE_NAME=%IMAGE_NAME:s=s%
set IMAGE_NAME=%IMAGE_NAME:t=t%
set IMAGE_NAME=%IMAGE_NAME:u=u%
set IMAGE_NAME=%IMAGE_NAME:v=v%
set IMAGE_NAME=%IMAGE_NAME:w=w%
set IMAGE_NAME=%IMAGE_NAME:x=x%
set IMAGE_NAME=%IMAGE_NAME:y=y%
set IMAGE_NAME=%IMAGE_NAME:z=z%
set IMAGE_NAME=%IMAGE_NAME:.=-%
set IMAGE_NAME=%IMAGE_NAME: =-%

echo === Build and Push Script for %IMAGE_NAME% ===
echo.

echo Select target platform:
echo 1. AWS ECR
echo 2. Docker Hub
set /p PLATFORM_CHOICE="Enter choice (1 or 2): "

if "!PLATFORM_CHOICE!"=="1" (
    set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
    set /p AWS_ACCOUNT_ID="Enter AWS Account ID: "
    set REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com
    set ECR_REPO=%IMAGE_NAME%
    set FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!
) else if "!PLATFORM_CHOICE!"=="2" (
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    set REGISTRY_URL=index.docker.io
    set FULL_IMAGE_NAME=!REGISTRY_URL!/!DOCKER_USERNAME!/%IMAGE_NAME%
) else (
    echo Invalid choice. Exiting.
    exit /b 1
)

set /p IMAGE_TAG="Enter image tag (or press Enter for 'latest'): "
if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest

REM Sanitize tag using PowerShell
for /f "delims=" %%i in ('powershell -Command "('$IMAGE_TAG' -replace '[^a-z0-9\.\-]', '-').TrimStart('-').TrimEnd('-')"') do set IMAGE_TAG=%%i
if "!IMAGE_TAG!"=="" set IMAGE_TAG=latest

set FULL_IMAGE_NAME=!FULL_IMAGE_NAME!:!IMAGE_TAG!

echo.
echo === Building image: !FULL_IMAGE_NAME! ===

if "!PLATFORM_CHOICE!"=="1" (
    echo Logging in to AWS ECR...
    aws ecr get-login-password --region !AWS_REGION! | docker login --username AWS --password-stdin !REGISTRY_URL!
    if !ERRORLEVEL! neq 0 (
        echo ECR login failed
        exit /b 1
    )
    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository...
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    )
) else if "!PLATFORM_CHOICE!"=="2" (
    set /p DOCKER_PASSWORD="Enter Docker Hub password: "
    echo.
    echo Logging in to Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    if !ERRORLEVEL! neq 0 (
        echo Docker login failed
        exit /b 1
    )
)

echo Building Docker image...
docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .
if !ERRORLEVEL! neq 0 (
    echo Docker build failed
    exit /b 1
)

echo Pushing image to registry...
docker push "!FULL_IMAGE_NAME!"
if !ERRORLEVEL! neq 0 (
    echo Docker push failed
    exit /b 1
)

echo.
echo === Success ===
echo Image pushed: !FULL_IMAGE_NAME!

endlocal
