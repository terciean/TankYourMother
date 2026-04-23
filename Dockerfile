# Base image for the runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7860
ENV ASPNETCORE_URLS=http://+:7860

# Image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["GasStationApp.csproj", "./"]
RUN dotnet restore "GasStationApp.csproj"

# Copy the rest of the files and build
COPY . .
RUN dotnet build "GasStationApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "GasStationApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a directory for logs and set permissions
USER root
RUN mkdir -p Logs && chown -R 1654:1654 Logs
USER 1654

ENTRYPOINT ["dotnet", "GasStationApp.dll"]
