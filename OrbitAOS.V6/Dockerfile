# Stage 1: Build stage with SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the project file first and restore
COPY ["OrbitAOS.V6/OrbitAOS.V6.csproj", "OrbitAOS.V6/"]
RUN dotnet restore "OrbitAOS.V6/OrbitAOS.V6.csproj"

# Copy the source code and publish
COPY . .
WORKDIR "/src/OrbitAOS.V6"
RUN dotnet publish "OrbitAOS.V6.csproj" -c Release -r linux-x64 --self-contained true -o /app/publish

# Stage 2: PurgeCSS stage to strip unused CSS from Bootstrap
FROM node:18-alpine AS purge
WORKDIR /purge
COPY --from=build /app/publish/wwwroot/lib/bootstrap/dist/css/ ./css/
COPY --from=build /app/publish/Views/ ./views/
COPY --from=build /app/publish/wwwroot/js/ ./js/
COPY --from=build /app/publish/wwwroot/css/ ./site-css/
RUN npm init -y && npm install -g purgecss
RUN mkdir -p /purged/css && \
    purgecss --css css/bootstrap.css css/bootstrap.rtl.css css/bootstrap-utilities.css css/bootstrap-utilities.rtl.css \
    --content views/**/*.cshtml js/**/*.js site-css/**/*.css \
    --output /purged/css

# Stage 3: Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app
RUN adduser --disabled-password --disabled-login --uid 1000 appuser
USER appuser

# Environment settings
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_GCServer=1
ENV DOTNET_GCHeapHardLimit=80000000

# Copy published output
COPY --from=build --chown=appuser /app/publish /app/

# Copy purged CSS over the originals
COPY --from=purge --chown=appuser /purged/css /app/wwwroot/lib/bootstrap/dist/css/

EXPOSE 8080

ENTRYPOINT ["./OrbitAOS.V6"]
