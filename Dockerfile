# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY FinanzasPersonales.Api/FinanzasPersonales.Api.csproj FinanzasPersonales.Api/
RUN dotnet restore FinanzasPersonales.Api/FinanzasPersonales.Api.csproj

# Copy everything and build
COPY . .
WORKDIR /src/FinanzasPersonales.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/uploads

COPY --from=build /app/publish .

# Railway sets PORT env var
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

EXPOSE 8080

ENTRYPOINT ["dotnet", "FinanzasPersonales.Api.dll"]
