FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HomeManager.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["nuget.config", "./"]
COPY ["HomeManager.Domain/HomeManager.Domain.csproj", "HomeManager.Domain/"]
COPY ["HomeManager.Application/HomeManager.Application.csproj", "HomeManager.Application/"]
COPY ["HomeManager.Infrastructure/HomeManager.Infrastructure.csproj", "HomeManager.Infrastructure/"]
COPY ["HomeManager.Api/HomeManager.Api.csproj", "HomeManager.Api/"]
COPY ["HomeManager.Tests.Unit/HomeManager.Tests.Unit.csproj", "HomeManager.Tests.Unit/"]
COPY ["HomeManager.Tests.Integration/HomeManager.Tests.Integration.csproj", "HomeManager.Tests.Integration/"]

RUN dotnet restore "HomeManager.Api/HomeManager.Api.csproj"

COPY . .
RUN dotnet publish "HomeManager.Api/HomeManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=5 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HomeManager.Api.dll"]
