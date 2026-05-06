# =========================
# Base runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar csproj (cache de restore)
COPY Payments.Microservice.API/Payments.Microservice.API.csproj Payments.Microservice.API/
COPY Payments.Microservice.Application/Payments.Microservice.Application.csproj Payments.Microservice.Application/
COPY Payments.Microservice.Domain/Payments.Microservice.Domain.csproj Payments.Microservice.Domain/
COPY Payments.Microservice.Infrastructure/Payments.Microservice.Infrastructure.csproj Payments.Microservice.Infrastructure/
COPY Payments.Microservice.shared/Payments.Microservice.shared.csproj Payments.Microservice.shared/
COPY Payments.Contracts/Payments.Contracts.csproj Payments.Contracts/

RUN dotnet restore Payments.Microservice.API/Payments.Microservice.API.csproj

# Copiar todo o código
COPY . .

# Build
WORKDIR /src/Payments.Microservice.API
RUN dotnet build Payments.Microservice.API.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish Payments.Microservice.API.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# =========================
# Migrator (opcional)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS migrator
WORKDIR /src

COPY . .
WORKDIR /src/Payments.Microservice.API

RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "ef", "database", "update", "--no-build"]

# =========================
# Runtime final
# =========================
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Payments.Microservice.API.dll"]
