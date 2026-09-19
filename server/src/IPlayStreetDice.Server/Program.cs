using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using IPlayStreetDice.Server.Core;
using IPlayStreetDice.Server.Core.Persistence;
using IPlayStreetDice.Server.Core.Voice;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<StreetDiceTableStore>();
builder.Services.AddHostedService<StreetDiceStatePersistenceService>();

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
    var sessionToken = store.CreateOrReplacePlayerSession(gameId, player.Id);
    return Results.Ok(new { playerId = player.Id, playerSessionToken = sessionToken, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/dice-color", (string gameId, DiceColorRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    engine.SelectDiceColor(request.PlayerId, request.Color);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/shot", (string gameId, OpenShotRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.ShooterId, request.ShooterSessionToken)) return Results.Unauthorized();
    engine.OpenShot(request.ShooterId, request.CatcherId, request.Amount);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/side-bet", (string gameId, SideBetRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    var bet = engine.PlaceSideBet(request.PlayerId, request.Type, request.Amount, request.TargetPointNumber);
    return Results.Ok(new { sideBetId = bet.Id, state = engine.State });
});

app.MapPost("/api/cee-lo/evaluate", (CeeLoRollRequest request) =>
{
    var result = CeeLoRules.Evaluate(new CeeLoRoll(request.Die1, request.Die2, request.Die3));
    return Results.Ok(new { result });
});

app.MapPost("/api/street-dice/{gameId}/fade", (string gameId, FadeRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.CatcherId, request.PlayerSessionToken)) return Results.Unauthorized();
    var result = engine.FadeCatch(request.CatcherId);
    return Results.Ok(new { result, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/roll", (string gameId, RollRequest request, StreetDiceTableStore store, IConfiguration config) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.ShooterId, request.PlayerSessionToken)) return Results.Unauthorized();
    if (!string.Equals(engine.State.ShooterId, request.ShooterId, StringComparison.OrdinalIgnoreCase)) return Results.BadRequest(new { error = "Only the current Shooter can roll." });

    var allowClientSuppliedRoll = string.Equals(
        config["StreetDice:AllowClientSuppliedRoll"] ?? Environment.GetEnvironmentVariable("STREET_DICE_ALLOW_CLIENT_SUPPLIED_ROLL"),
        "true",
        StringComparison.OrdinalIgnoreCase);
    var roll = DiceRollFairness.ResolveRoll(allowClientSuppliedRoll, request.Die1, request.Die2);
    var result = engine.Roll(roll);
    return Results.Ok(new { result, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/decision/run-same", (string gameId, ShooterDecisionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.ShooterId, request.PlayerSessionToken)) return Results.Unauthorized();
    engine.RunSame(request.ShooterId);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/decision/double-up", (string gameId, ShooterDecisionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.ShooterId, request.PlayerSessionToken)) return Results.Unauthorized();
    engine.DoubleUp(request.ShooterId);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/bots/fill", (string gameId, BotFillRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    var bots = engine.FillBots(request.TargetPlayers);
    var sessions = bots.Select(bot => new
    {
        playerId = bot.Id,
        playerSessionToken = store.CreateOrReplacePlayerSession(gameId, bot.Id)
    });
    return Results.Ok(new { bots = sessions, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/bots/advance", (string gameId, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    var result = engine.AdvanceBotAction(Random.Shared);
    return Results.Ok(new { result, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/voice/access-token", (string gameId, VoiceAccessRequest request, StreetDiceTableStore store, IConfiguration config) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (engine.State.FindPlayer(request.PlayerId) == null) return Results.NotFound(new { error = "Player not found." });
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();

    var issuer = config["Vivox:Issuer"] ?? Environment.GetEnvironmentVariable("VIVOX_ISSUER");
    var key = config["Vivox:Key"] ?? Environment.GetEnvironmentVariable("VIVOX_KEY");
    var domain = config["Vivox:Domain"] ?? Environment.GetEnvironmentVariable("VIVOX_DOMAIN");
    var channel = $"iplay.streetdice.table.{gameId}";

    if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(domain))
    {
        return Results.Json(new
        {
            configured = false,
            requiresVivoxAccessTokenConfiguration = true,
            channel,
            message = "Vivox issuer, key, and domain are required before voice tokens can be issued."
        }, statusCode: StatusCodes.Status501NotImplemented);
    }

    var lifetime = TimeSpan.FromSeconds(90);
    var token = VivoxAccessTokenGenerator.GenerateJoinToken(issuer, key, domain, request.PlayerId, channel, lifetime);

    return Results.Ok(new
    {
        configured = true,
        channel,
        participant = request.PlayerId,
        token,
        expiresInSeconds = (int)lifetime.TotalSeconds
    });
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
    private readonly ConcurrentDictionary<string, string> _playerSessions = new();

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

    public PersistedStoreState CreateSnapshot()
    {
        return new PersistedStoreState
        {
            Games = _games.Values.Select(e => e.State.ToSnapshot()).ToList(),
            PlayerSessions = new Dictionary<string, string>(_playerSessions)
        };
    }

    public void PersistTo(string filePath) => StreetDiceStatePersistence.Save(filePath, CreateSnapshot());

    public void RestoreFrom(string filePath)
    {
        var snapshot = StreetDiceStatePersistence.Load(filePath);
        if (snapshot is null) return;

        foreach (var gameSnapshot in snapshot.Games)
        {
            _games[gameSnapshot.GameId] = gameSnapshot.ToEngine();
        }

        foreach (var (sessionKey, token) in snapshot.PlayerSessions)
        {
            _playerSessions[sessionKey] = token;
        }
    }

    public string CreateOrReplacePlayerSession(string gameId, string playerId)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        _playerSessions[SessionKey(gameId, playerId)] = token;
        return token;
    }

    public bool ValidatePlayerSession(string gameId, string playerId, string playerSessionToken)
    {
        if (string.IsNullOrWhiteSpace(playerSessionToken)) return false;
        if (playerSessionToken.Length != 64) return false;
        try
        {
            return _playerSessions.TryGetValue(SessionKey(gameId, playerId), out var expected)
                && CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(expected),
                    Convert.FromHexString(playerSessionToken));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string SessionKey(string gameId, string playerId) => $"{gameId}:{playerId}".ToLowerInvariant();
}

/// <summary>Loads persisted state on startup, snapshots it periodically, and flushes on shutdown.</summary>
public sealed class StreetDiceStatePersistenceService : IHostedService, IDisposable
{
    private static readonly TimeSpan SaveInterval = TimeSpan.FromSeconds(5);

    private readonly StreetDiceTableStore _store;
    private readonly string _filePath;
    private Timer? _timer;

    public StreetDiceStatePersistenceService(StreetDiceTableStore store, IConfiguration config)
    {
        _store = store;
        _filePath = config["StreetDice:PersistencePath"]
            ?? Environment.GetEnvironmentVariable("STREET_DICE_PERSISTENCE_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "data", "street-dice-state.json");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _store.RestoreFrom(_filePath);
        _timer = new Timer(_ => _store.PersistTo(_filePath), null, SaveInterval, SaveInterval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        _store.PersistTo(_filePath);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}

public sealed record JoinRequest(string PlayerName, string? PlayerId = null);
public sealed record DiceColorRequest(string PlayerId, string PlayerSessionToken, DiceColor Color);
public sealed record OpenShotRequest(string ShooterId, string ShooterSessionToken, string CatcherId, int Amount);
public sealed record SideBetRequest(string PlayerId, string PlayerSessionToken, SideBetType Type, int Amount, int? TargetPointNumber = null);
public sealed record FadeRequest(string CatcherId, string PlayerSessionToken);
public sealed record RollRequest(string ShooterId, string PlayerSessionToken, int? Die1 = null, int? Die2 = null);
public sealed record CeeLoRollRequest(int Die1, int Die2, int Die3);
public sealed record ShooterDecisionRequest(string ShooterId, string PlayerSessionToken);
public sealed record BotFillRequest(int TargetPlayers = 5);
public sealed record VoiceAccessRequest(string PlayerId, string PlayerSessionToken);
