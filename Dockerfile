# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY StrangelOracle.sln ./
COPY src/StrangelOracle.Domain/StrangelOracle.Domain.csproj src/StrangelOracle.Domain/
COPY src/StrangelOracle.Application/StrangelOracle.Application.csproj src/StrangelOracle.Application/
COPY src/StrangelOracle.Infrastructure/StrangelOracle.Infrastructure.csproj src/StrangelOracle.Infrastructure/
COPY src/StrangelOracle.API/StrangelOracle.API.csproj src/StrangelOracle.API/
COPY tests/StrangelOracle.Tests/StrangelOracle.Tests.csproj tests/StrangelOracle.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish - cache bust here
WORKDIR /src/src/StrangelOracle.API
ARG CACHEBUST=3
RUN echo "Build: $CACHEBUST" && dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "StrangelOracle.API.dll"]
