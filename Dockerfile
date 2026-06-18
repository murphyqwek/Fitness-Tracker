FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Fintes-Tracker/Fintes-Tracker.csproj", "Fintes-Tracker/"]
COPY ["Fintes-Tracker-Application/Fintes-Tracker-Application.csproj", "Fintess-Tracker-Application/"]
COPY ["Fintes-Tracker-Domain/Fintes-Tracker-Domain.csproj", "Fintess-Tracker-Domain/"]
COPY ["Fintes-Tracker-Infrastructure/Fintes-Tracker-Infrastructure.csproj", "Fintess-Tracker-Infrastructure/"]

RUN dotnet restore "Fintes-Tracker/Fintes-Tracker.csproj"

COPY . .
WORKDIR "/src/Fintes-Tracker"

RUN dotnet build "Fintes-Tracker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fintes-Tracker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fintes-Tracker.dll"]