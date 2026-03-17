FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CheckInSystem.sln", "./"]
COPY ["src/backend/CheckInSystem.Api/CheckInSystem.Api.csproj", "src/backend/CheckInSystem.Api/"]
COPY ["src/backend/CheckInSystem.Application/CheckInSystem.Application.csproj", "src/backend/CheckInSystem.Application/"]
COPY ["src/backend/CheckInSystem.Domain/CheckInSystem.Domain.csproj", "src/backend/CheckInSystem.Domain/"]
COPY ["src/backend/CheckInSystem.Infrastructure/CheckInSystem.Infrastructure.csproj", "src/backend/CheckInSystem.Infrastructure/"]

RUN dotnet restore "src/backend/CheckInSystem.Api/CheckInSystem.Api.csproj"

COPY . .
RUN dotnet publish "src/backend/CheckInSystem.Api/CheckInSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CheckInSystem.Api.dll"]
