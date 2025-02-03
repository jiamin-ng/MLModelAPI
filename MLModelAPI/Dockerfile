# Use official ASP.NET runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MLModelAPI.csproj", "./"]
RUN dotnet restore "./MLModelAPI.csproj"

# Copy everything and build the application
COPY . .
RUN dotnet publish -c Release -o /app

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MLModelAPI.dll"]

