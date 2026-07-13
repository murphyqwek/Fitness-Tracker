FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Fitness-Tracker/Fitness-Tracker.csproj", "Fitness-Tracker/"]
COPY ["Fitness-Tracker-Application/Fitness-Tracker-Application.csproj", "Fitness-Tracker-Application/"]
COPY ["Fitness-Tracker-Domain/Fitness-Tracker-Domain.csproj", "Fitness-Tracker-Domain/"]
COPY ["Fitness-Tracker-Infrastructure/Fitness-Tracker-Infrastructure.csproj", "Fitness-Tracker-Infrastructure/"]

RUN dotnet restore "Fitness-Tracker/Fitness-Tracker.csproj"

COPY . .
WORKDIR "/src/Fitness-Tracker"

RUN dotnet build "Fitness-Tracker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fitness-Tracker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fitness-Tracker.dll"]