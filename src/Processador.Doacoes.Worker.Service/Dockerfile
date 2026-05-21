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

# Copiar csproj
COPY Processador.Doacoes.Worker.Service.Api/Processador.Doacoes.Worker.Service.Api.csproj Processador.Doacoes.Worker.Service.Api/

RUN dotnet restore Processador.Doacoes.Worker.Service.Api/Processador.Doacoes.Worker.Service.Api.csproj

# Copiar todo o código
COPY . .

# Publish
WORKDIR /src/Processador.Doacoes.Worker.Service.Api
RUN dotnet publish Processador.Doacoes.Worker.Service.Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# =========================
# Runtime final
# =========================
FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Processador.Doacoes.Worker.Service.Api.dll"]