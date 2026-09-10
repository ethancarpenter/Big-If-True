# syntax=docker/dockerfile:1

# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# The SDK comes from the base image (latest .NET 8), which satisfies the
# repo-root global.json (rollForward: latestFeature). global.json itself is
# outside this build context (backend/) and is not needed for the container
# build.

# Restore against the project files alone first, so this layer stays cached
# unless a .csproj actually changes.
COPY CampaignApp.Api/CampaignApp.Api.csproj                       CampaignApp.Api/
COPY CampaignApp.Application/CampaignApp.Application.csproj        CampaignApp.Application/
COPY CampaignApp.Domain/CampaignApp.Domain.csproj                 CampaignApp.Domain/
COPY CampaignApp.Infrastructure/CampaignApp.Infrastructure.csproj CampaignApp.Infrastructure/
RUN dotnet restore CampaignApp.Api/CampaignApp.Api.csproj

# Sources for the four runtime projects only. CampaignApp.Tests is not part
# of the deployed image and is never copied.
COPY CampaignApp.Api/            CampaignApp.Api/
COPY CampaignApp.Application/     CampaignApp.Application/
COPY CampaignApp.Domain/         CampaignApp.Domain/
COPY CampaignApp.Infrastructure/ CampaignApp.Infrastructure/

RUN dotnet publish CampaignApp.Api/CampaignApp.Api.csproj \
    -c Release --no-restore -o /app /p:UseAppHost=false

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Render routes external traffic to the container on the port in its PORT
# env var (10000 by default). Kestrel must be told explicitly and must bind
# all interfaces - not localhost - so the platform proxy can reach it. If
# PORT is changed on the Render service, ASPNETCORE_URLS must be changed to
# match (set it as a Render env var to override this default).
ENV ASPNETCORE_HTTP_PORTS=
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# ASPNETCORE_ENVIRONMENT is deliberately left unset => Production. That is
# what keeps the dev/demo data seeders and Swagger switched off.

# Run as the non-root 'app' user the aspnet image already provides.
USER app

COPY --from=build /app ./

ENTRYPOINT ["dotnet", "CampaignApp.Api.dll"]
