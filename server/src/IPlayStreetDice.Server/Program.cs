using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using IPlayStreetDice.Server.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<StreetDiceTableStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "iPlay Street Dice Backend",
    serverAuthoritative = true,
    supportedPlayers = new { min = 2, max = 5 },
    supportedDiceColors = Enum.GetNames<DiceColor>(),
    reservedHotDiceColors = new[] { "Red", "Orange" }
}));

app.MapPost("/api/street-dice/create", (StreetDiceTableStore store) =>
{
    var engine = store.CreateGame();
    return Results.Ok(new { gameId = engine.State.GameId, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/join", (string gameId, JoinRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    var playerId = string.IsNullOrWhiteSpace(request.PlayerId) ? Guid.NewGuid().ToString("N") : request.PlayerId.Trim();
    var player = engine.AddPlayer(playerId, request.PlayerName.Trim());
    return Results.Ok(new { playerId = player.Id, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/dice-color", (string gameId, DiceColorRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    engine.SelectDiceColor(request.PlayerId, request.Color);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/shot", (string gameId, OpenShotRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    engine.OpenShot(request.ShooterId, request.CatcherId, request.Amount);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/side-bet", (string gameId, SideBetRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    var bet = engine.PlaceSideBet(request.PlayerId, request.Type, request.Amount);
    return Results.Ok(new { sideBetId = bet.Id, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/fade", (string gameId, FadeRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    var result = engine.FadeCatch(request.CatcherId);
    return Results.Ok(new { result, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/roll", (string gameId, RollRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    var result = engine.Roll(new DiceRoll(request.Die1, request.Die2));
    return Results.Ok(new { result, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/decision/run-same", (string gameId, ShooterDecisionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    engine.RunSame(request.ShooterId);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/decision/double-up", (string gameId, ShooterDecisionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    engine.DoubleUp(request.ShooterId);
    return Results.Ok(new { state = engine.State });
});

app.MapGet("/api/street-dice/{gameId}", (string gameId, StreetDiceTableStore store) =>
{
    return store.TryGet(gameId, out var engine)
        ? Results.Ok(new { state = engine.State })
        : Results.NotFound(new { error = "Game not found." });
});

app.Run();

public sealed class StreetDiceTableStore
{
    private readonly ConcurrentDictionary<string, StreetDiceGameEngine> _games = new();

    public StreetDiceGameEngine CreateGame()
    {
        var gameId = Guid.NewGuid().ToString("N");
        var engine = new StreetDiceGameEngine(gameId);
        _games[gameId] = engine;
        return engine;
    }

    public bool TryGet(string gameId, out StreetDiceGameEngine engine)
    {
        return _games.TryGetValue(gameId, out engine!);
    }
}

public sealed record JoinRequest(string PlayerName, string? PlayerId = null);
public sealed record DiceColorRequest(string PlayerId, DiceColor Color);
public sealed record OpenShotRequest(string ShooterId, string CatcherId, int Amount);
public sealed record SideBetRequest(string PlayerId, SideBetType Type, int Amount);
public sealed record FadeRequest(string CatcherId);
public sealed record RollRequest(int Die1, int Die2);
public sealed record ShooterDecisionRequest(string ShooterId);
