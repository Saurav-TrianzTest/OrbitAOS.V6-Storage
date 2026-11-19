# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project file and restore as distinct layers
COPY ["OrbitAOS.V6/OrbitAOS.V6.csproj", "OrbitAOS.V6/"]
RUN dotnet restore "OrbitAOS.V6/OrbitAOS.V6.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/OrbitAOS.V6"
RUN dotnet publish "OrbitAOS.V6.csproj" -c Release -r linux-x64 --self-contained true -o /app/publish /p:PublishReadyToRun=true --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd --gid 1000 appgroup && \
    useradd --uid 1000 --gid 1000 --disabled-password --disabled-login appuser && \
    chown -R appuser:appgroup /app

# Environment settings
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_GC_CONCURRENT=1 \
    DOTNET_GC_SERVER=1 \
    TZ=UTC

# Copy published output
COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

# Expose the port the app listens on
EXPOSE 8080

# Enable graceful shutdown
ENTRYPOINT ["./OrbitAOS.V6"]
