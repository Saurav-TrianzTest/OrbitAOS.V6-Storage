@echo off
setlocal enabledelayedexpansion

echo === OrbitAOS.V6 Container Build and Push ===
echo.

set "PROJECT_NAME=OrbitAOS.V6"
REM Sanitize: lower-case, non-alphanumeric to hyphen, no leading/trailing hyphen
for /f "delims=" %%i in ('powershell -NoProfile -Command "(echo %PROJECT_NAME% | ForEach-Object { $_.ToLower() }) -creplace '[^a-z0-9]', '-'"') do set "IMAGE_NAME_TEMP=%%i"
set "IMAGE_NAME=!IMAGE_NAME_TEMP:"-=!"
REM Remove leading/trailing hyphens
powershell -NoProfile -Command "$n='%IMAGE_NAME%'; $n=$n.Trim('-'); Write-Output $n" > "%TEMP%\imagename.txt"
set /p IMAGE_NAME=<"%TEMP%\imagename.txt"
del "%TEMP%\imagename.txt"

echo Choose the target platform:
echo   1^) AWS ECR
echo   2^) Docker Hub
echo.
set /p PLATFORM_CHOICE="Enter choice (1 or 2): "

if "!PLATFORM_CHOICE!"=="1" (
    echo.
    echo === AWS ECR ===
    set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
    set /p AWS_ACCOUNT_ID="Enter AWS account ID (e.g., 123456789012): "
    set "REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com"
    set "ECR_REPO=!IMAGE_NAME!"
    set "FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!"

    set /p IMAGE_TAG="Enter image tag (press Enter for 'latest'): "
    if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"

    REM Sanitize tag
    powershell -NoProfile -Command "$t='!IMAGE_TAG!'; $t=$t -creplace '[^a-z0-9.\-]', '-'; $t=$t.Trim('-'); Write-Output $t" > "%TEMP%\imagetag.txt"
    set /p IMAGE_TAG=<"%TEMP%\imagetag.txt"
    del "%TEMP%\imagetag.txt"
    if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"

    set "FULL_IMAGE_NAME=!FULL_IMAGE_NAME!:!IMAGE_TAG!"

    echo.
    echo Logging in to AWS ECR...
    call aws ecr get-login-password --region !AWS_REGION! | docker login --username AWS --password-stdin !REGISTRY_URL!
    if !ERRORLEVEL! neq 0 (echo ECR login failed & exit /b 1)

    echo Creating ECR repository if it does not exist...
    call aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository...
        call aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
        if !ERRORLEVEL! neq 0 (echo Failed to create ECR repository & exit /b 1)
    )

    echo.
    echo Building Docker image...
    docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .
    if !ERRORLEVEL! neq 0 (echo Docker build failed & exit /b 1)

    echo.
    echo Pushing image to ECR...
    docker push "!FULL_IMAGE_NAME!"
    if !ERRORLEVEL! neq 0 (echo Docker push failed & exit /b 1)

    echo.
    echo === Success ===
    echo Image pushed: !FULL_IMAGE_NAME!
    exit /b 0
)

if "!PLATFORM_CHOICE!"=="2" (
    echo.
    echo === Docker Hub ===
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    call cmd /c "powershell -NoProfile -Command "$p = Read-Host 'Enter Docker Hub password (or access token)'; Write-Output $p"" > "%TEMP%\dockerpw.txt"
    set /p DOCKER_PASSWORD=<"%TEMP%\dockerpw.txt"
    del "%TEMP%\dockerpw.txt"

    set /p IMAGE_TAG="Enter image tag (press Enter for 'latest'): "
    if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"

    REM Sanitize tag
    powershell -NoProfile -Command "$t='!IMAGE_TAG!'; $t=$t -creplace '[^a-z0-9.\-]', '-'; $t=$t.Trim('-'); Write-Output $t" > "%TEMP%\imagetag.txt"
    set /p IMAGE_TAG=<"%TEMP%\imagetag.txt"
    del "%TEMP%\imagetag.txt"
    if "!IMAGE_TAG!"=="" set "IMAGE_TAG=latest"

    set "FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!"

    echo.
    echo Logging in to Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    if !ERRORLEVEL! neq 0 (echo Docker Hub login failed & exit /b 1)

    echo.
    echo Building Docker image...
    docker build -f Dockerfile -t "!FULL_IMAGE_NAME!" .
    if !ERRORLEVEL! neq 0 (echo Docker build failed & exit /b 1)

    echo.
    echo Pushing image to Docker Hub...
    docker push "!FULL_IMAGE_NAME!"
    if !ERRORLEVEL! neq 0 (echo Docker push failed & exit /b 1)

    echo.
    echo === Success ===
    echo Image pushed: !FULL_IMAGE_NAME!
    exit /b 0
)

echo Invalid choice. Exiting.
exit /b 1
