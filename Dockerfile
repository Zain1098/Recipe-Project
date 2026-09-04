# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Recipe Project/Recipe Project.csproj", "Recipe Project/"]
RUN dotnet restore "Recipe Project/Recipe Project.csproj"

# Copy the remaining project files
COPY . .
WORKDIR "/src/Recipe Project"

# Build and publish release output
RUN dotnet publish "Recipe Project.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose standard container port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published files from build stage
COPY --from=build /app/publish .

# Create directory for SQLite database storage
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "Recipe Project.dll"]
