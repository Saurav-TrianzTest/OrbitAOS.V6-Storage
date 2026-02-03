# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy project files for dependency caching
COPY *.csproj ./
RUN dotnet restore --verbosity minimal

# Copy source code
COPY . .

# Build application
RUN dotnet build -c Release --no-restore -o /app/build

# Publish application
RUN dotnet publish -c Release --no-build -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=builder /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5000 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=UTC

# Expose application port
EXPOSE 5000

# Set entrypoint
ENTRYPOINT ["dotnet", "TestSanity.dll"]