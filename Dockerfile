FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Fitnes-Tracker/Fintes-Tracker.csproj", "Fitnes-Tracker/"]
COPY ["Fitnes-Tracker-Application/Fintes-Tracker-Application.csproj", "Fitnes-Tracker-Application/"]
COPY ["Fitnes-Tracker-Domain/Fintes-Tracker-Domain.csproj", "Fitnes-Tracker-Domain/"]
COPY ["Fitnes-Tracker-Infrastructure/Fintes-Tracker-Infrastructure.csproj", "Fitnes-Tracker-Infrastructure/"]

RUN dotnet restore "Fitnes-Tracker/Fintes-Tracker.csproj"

COPY . .
WORKDIR "/src/Fitnes-Tracker"

RUN dotnet build "Fintes-Tracker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fintes-Tracker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fintes-Tracker.dll"]