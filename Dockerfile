# Build stage: compile the .NET application and purge unused CSS
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY OrbitAOS.V6/OrbitAOS.V6.csproj OrbitAOS.V6/
RUN dotnet restore OrbitAOS.V6/OrbitAOS.V6.csproj

# Copy the rest of the source code
COPY . .

# CSS purge stage: strip unused Bootstrap CSS using PurgeCSS
# (cz-css-1005: Unused CSS in Container Images)
FROM node:18-alpine AS css-purge
WORKDIR /purge

# Copy package files and install PurgeCSS
COPY --from=build /src/OrbitAOS.V6/package.json .
COPY --from=build /src/OrbitAOS.V6/purgecss.config.js .
RUN npm install

# Copy the CSS and content files from the build stage
COPY --from=build /src/OrbitAOS.V6/wwwroot/lib/bootstrap/dist/css ./wwwroot/lib/bootstrap/dist/css
COPY --from=build /src/OrbitAOS.V6/Views ./Views
COPY --from=build /src/OrbitAOS.V6/Areas ./Areas

# Run PurgeCSS to strip unused CSS
RUN npx purgecss --config purgecss.config.js

# Publish stage: build the .NET application (self-contained, since runtime:6.0 lacks ASP.NET Core shared framework)
FROM build AS publish
RUN dotnet publish OrbitAOS.V6/OrbitAOS.V6.csproj -c Release -r linux-x64 --self-contained true -o /app/publish

# Runtime stage: minimal image with the published application
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app

# Copy the purged CSS files from the CSS purge stage
COPY --from=css-purge /purge/wwwroot/lib/bootstrap/dist/css ./wwwroot/lib/bootstrap/dist/css

# Copy the published application
COPY --from=publish /app/publish .

# Copy remaining wwwroot assets (non-CSS files like JS, images, favicon)
COPY --from=build /src/OrbitAOS.V6/wwwroot/js ./wwwroot/js
COPY --from=build /src/OrbitAOS.V6/wwwroot/css ./wwwroot/css
COPY --from=build /src/OrbitAOS.V6/wwwroot/favicon.ico ./wwwroot/favicon.ico
COPY --from=build /src/OrbitAOS.V6/wwwroot/lib/jquery ./wwwroot/lib/jquery
COPY --from=build /src/OrbitAOS.V6/wwwroot/lib/bootstrap/dist/js ./wwwroot/lib/bootstrap/dist/js

# Enable non-root execution
RUN groupadd -g 10001 appuser && useradd -u 10001 -g 10001 -m -d /home/appuser appuser
RUN chown -R appuser:appuser /app
USER 10001

# Environment settings
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_gcServer=1
ENV DOTNET_gcConcurrent=1
ENV DOTNET_GCHeapHardLimit=40000000
ENV DOTNET_ThreadPool_ForceMinThreadPoolSize=32
ENV TZ=UTC

# Expose the port the application listens on
EXPOSE 8080

# Entry point (self-contained executable)
ENTRYPOINT ["./OrbitAOS.V6"]
