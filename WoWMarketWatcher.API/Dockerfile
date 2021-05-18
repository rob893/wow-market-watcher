FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5003

ENV ASPNETCORE_URLS=http://*:5003
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["WoWMarketWatcher.API/WoWMarketWatcher.API.csproj", "WoWMarketWatcher.API/"]
RUN dotnet restore "WoWMarketWatcher.API/WoWMarketWatcher.API.csproj"
COPY . .
WORKDIR "/src/WoWMarketWatcher.API"
RUN dotnet build "WoWMarketWatcher.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WoWMarketWatcher.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WoWMarketWatcher.API.dll"]

# FROM mcr.microsoft.com/dotnet/aspnet:5.0

# COPY bin/Release/net5.0/publish/ App/
# WORKDIR /App

# ENV ASPNETCORE_ENVIRONMENT=Production
# ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false

# ENTRYPOINT ["dotnet", "WoWMarketWatcher.API.dll"]
