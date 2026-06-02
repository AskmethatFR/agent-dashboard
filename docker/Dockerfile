# ============================================================================
# Multi-stage Docker build for agent-dashboard
# ============================================================================

# ---- Build Stage ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

# Copy solution and project files first for better caching
COPY ["AgentDashboard.sln", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/AgentDashboard.Web/AgentDashboard.Web.csproj", "src/AgentDashboard.Web/"]
COPY ["src/AgentDashboard.TicketTracking.Domain/AgentDashboard.TicketTracking.Domain.csproj", "src/AgentDashboard.TicketTracking.Domain/"]
COPY ["src/AgentDashboard.TicketTracking.Application/AgentDashboard.TicketTracking.Application.csproj", "src/AgentDashboard.TicketTracking.Application/"]
COPY ["src/AgentDashboard.TicketTracking.Infrastructure/AgentDashboard.TicketTracking.Infrastructure.csproj", "src/AgentDashboard.TicketTracking.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/AgentDashboard.Web/AgentDashboard.Web.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR /src/AgentDashboard.Web
RUN dotnet publish "AgentDashboard.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=true

# ---- Runtime Stage --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

# Install wget for healthcheck (alpine base doesn't include it)
RUN apk add --no-cache wget

# Create non-root group and user
RUN addgroup -S appuser -g 1001 && \
    adduser -S appuser -u 1001 -G appuser -D -s /bin/false

RUN mkdir /app && chown appuser:appuser /app

WORKDIR /app

# Copy published app from build stage
COPY --from=build --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/healthz || exit 1

# Expose port
EXPOSE 8080

# Entrypoint
ENTRYPOINT ["dotnet", "AgentDashboard.Web.dll"]
