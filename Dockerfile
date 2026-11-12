# Stage 1: Build and publish with .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["OrbitAOS.V6/OrbitAOS.V6.csproj", "OrbitAOS.V6/"]
RUN dotnet restore "OrbitAOS.V6/OrbitAOS.V6.csproj"

# Copy everything else and publish self-contained
# (runtime:6.0 lacks the ASP.NET Core shared framework, so self-contained includes it)
COPY . .
WORKDIR "/src/OrbitAOS.V6"
RUN dotnet publish "OrbitAOS.V6.csproj" -c Release -r linux-x64 --self-contained true -o /app/publish

# Stage 2: PurgeCSS — strip unused CSS from the published wwwroot
# Implements the remediation for cz-css-1005 (Unused CSS in Container Images)
FROM node:18-alpine AS purgecss-stage
WORKDIR /purgecss
COPY --from=build /app/publish /app-publish/
COPY ["OrbitAOS.V6/purgecss.config.js", "./"]
COPY ["OrbitAOS.V6/package.json", "./"]
RUN npm install --no-save purgecss@5.0.0 && \
    npx purgecss --config purgecss.config.js --css /app-publish/wwwroot/lib/bootstrap/dist/css/*.css --output /app-publish/wwwroot/lib/bootstrap/dist/css/ && \
    npx purgecss --config purgecss.config.js --css /app-publish/wwwroot/css/*.css --output /app-publish/wwwroot/css/

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app
RUN addgroup --gid 1000 appgroup && \
    adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser
USER appuser

# Environment settings for container
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV TZ=UTC

# Copy self-contained published output (includes ASP.NET Core shared framework and purged CSS)
COPY --from=purgecss-stage /app-publish /app/

# Expose the port the application listens on
EXPOSE 8080

# Graceful shutdown via the published self-contained executable
ENTRYPOINT ["./OrbitAOS.V6"]
