# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["CoffeeTunes.Frontend/CoffeeTunes.Frontend.csproj", "CoffeeTunes.Frontend/"]
RUN dotnet restore "CoffeeTunes.Frontend/CoffeeTunes.Frontend.csproj"

# Copy source code and build
COPY CoffeeTunes.Frontend/ CoffeeTunes.Frontend/
WORKDIR "/src/CoffeeTunes.Frontend"
RUN dotnet build "CoffeeTunes.Frontend.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CoffeeTunes.Frontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - using nginx to serve the static files
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY CoffeeTunes.Frontend/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
