# syntax=docker/dockerfile:1.6
# ---- Build stage ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, leveraging Docker layer cache.
COPY NogoYa.sln ./
COPY src/NogoYa.Domain/NogoYa.Domain.csproj          src/NogoYa.Domain/
COPY src/NogoYa.Application/NogoYa.Application.csproj src/NogoYa.Application/
COPY src/NogoYa.Infrastructure/NogoYa.Infrastructure.csproj src/NogoYa.Infrastructure/
COPY src/NogoYa.API/NogoYa.API.csproj                src/NogoYa.API/
RUN dotnet restore NogoYa.sln

# Copy the rest and publish.
COPY . .
RUN dotnet publish src/NogoYa.API/NogoYa.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---- Runtime stage ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Render injects PORT; bind ASP.NET Core to it.
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Run as non-root.
RUN useradd -u 1001 -m appuser
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NogoYa.API.dll"]
