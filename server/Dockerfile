# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/IPlayStreetDice.Server/IPlayStreetDice.Server.csproj src/IPlayStreetDice.Server/
RUN dotnet restore src/IPlayStreetDice.Server/IPlayStreetDice.Server.csproj

COPY src/IPlayStreetDice.Server/ src/IPlayStreetDice.Server/
RUN dotnet publish src/IPlayStreetDice.Server/IPlayStreetDice.Server.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Game/session state is snapshotted here every few seconds and on shutdown
# (see StreetDiceStatePersistenceService). Mount a volume onto this path so
# restarts and redeploys don't lose it - see docs/post-production-handoff.md.
ENV STREET_DICE_PERSISTENCE_PATH=/app/data/street-dice-state.json
RUN mkdir -p /app/data

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "IPlayStreetDice.Server.dll"]
