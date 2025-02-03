# Use official ASP.NET runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file
COPY MLModelAPI/MLModelAPI.csproj MLModelAPI/
WORKDIR /src/MLModelAPI
RUN dotnet restore

# Copy everything and build the application
COPY MLModelAPI/. .
RUN dotnet publish -c Release -o /app

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MLModelAPI.dll"]


