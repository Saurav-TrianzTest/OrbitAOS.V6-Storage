# Stage 1: PurgeCSS — strip unused CSS from bootstrap library files (cz-css-1005)
FROM node:18-alpine AS purgecss
WORKDIR /purgecss
COPY OrbitAOS.V6/wwwroot/lib/bootstrap/dist/css/ ./css/
COPY OrbitAOS.V6/Views/ ./views/
COPY OrbitAOS.V6/Areas/ ./areas/
COPY OrbitAOS.V6/wwwroot/css/ ./wwwroot-css/
COPY OrbitAOS.V6/wwwroot/js/ ./wwwroot-js/
COPY OrbitAOS.V6/wwwroot/lib/bootstrap/dist/js/ ./bootstrap-js/
RUN npx purgecss --css ./css/*.css --content "./views/**/*.cshtml" "./areas/**/*.cshtml" "./wwwroot-css/**/*.css" "./wwwroot-js/**/*.js" "./bootstrap-js/**/*.js" --output ./output/

# Stage 2: Build — .NET 6.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY OrbitAOS.V6.sln ./
COPY OrbitAOS.V6/*.csproj ./OrbitAOS.V6/
RUN dotnet restore
COPY . .
# Overwrite CSS files with purged versions from the PurgeCSS stage
COPY --from=purgecss /purgecss/output/ ./OrbitAOS.V6/wwwroot/lib/bootstrap/dist/css/
WORKDIR /src/OrbitAOS.V6
RUN dotnet publish -c Release -r linux-x64 --self-contained true -o /app/publish

# Stage 3: Runtime — .NET 6.0 runtime (self-contained, no shared framework needed)
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app/publish .
# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser
# Environment settings
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_gcServer=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV TZ=UTC
EXPOSE 8080
ENTRYPOINT ["./OrbitAOS.V6"]
