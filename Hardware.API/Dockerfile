# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["NuGet.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
COPY ["Hardware.Domain/Hardware.Domain.csproj", "Hardware.Domain/"]
COPY ["Hardware.Shared/Hardware.Shared.csproj", "Hardware.Shared/"]
COPY ["Hardware.Application/Hardware.Application.csproj", "Hardware.Application/"]
COPY ["Hardware.Infrastructure/Hardware.Infrastructure.csproj", "Hardware.Infrastructure/"]
COPY ["Hardware.ServiceDefaults/Hardware.ServiceDefaults.csproj", "Hardware.ServiceDefaults/"]
COPY ["Hardware.API/Hardware.API.csproj", "Hardware.API/"]
RUN dotnet restore "Hardware.API/Hardware.API.csproj"

COPY . .
WORKDIR /src/Hardware.API
RUN dotnet publish "Hardware.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && groupadd --gid 1001 appgroup \
    && useradd --uid 1001 --gid appgroup --shell /bin/bash --no-create-home appuser

COPY --from=build /app/publish .
RUN mkdir -p /app/Logs && chown -R appuser:appgroup /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

USER appuser

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -sf http://localhost:8080/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "Hardware.API.dll"]
