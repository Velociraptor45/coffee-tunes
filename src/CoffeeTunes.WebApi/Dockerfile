# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["CoffeeTunes.WebApi/CoffeeTunes.WebApi.csproj", "CoffeeTunes.WebApi/"]
RUN dotnet restore "CoffeeTunes.WebApi/CoffeeTunes.WebApi.csproj"

# Copy source code and build
COPY CoffeeTunes.WebApi/ CoffeeTunes.WebApi/
WORKDIR "/src/CoffeeTunes.WebApi"
RUN dotnet build "CoffeeTunes.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CoffeeTunes.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeeTunes.WebApi.dll"]
