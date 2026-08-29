# syntax=docker/dockerfile:1

# --- Frontend build ---
FROM node:22-alpine AS frontend-build
WORKDIR /frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build -- --configuration production

# --- Backend build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY HouseholdPanel.slnx NuGet.Config ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet restore src/HouseholdPanel.Api/HouseholdPanel.Api.csproj
RUN dotnet publish src/HouseholdPanel.Api/HouseholdPanel.Api.csproj -c Release -o /app/publish --no-restore

# --- Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /frontend/dist/frontend/browser ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HouseholdPanel.Api.dll"]
