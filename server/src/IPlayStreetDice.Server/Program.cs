using System.Collections.Concurrent;
using System.Security.Cryptography;
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

app.MapPost("/api/street-dice/{gameId}/roll", (string gameId, RollRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.ShooterId, request.PlayerSessionToken)) return Results.Unauthorized();
    if (!string.Equals(engine.State.ShooterId, request.ShooterId, StringComparison.OrdinalIgnoreCase)) return Results.BadRequest(new { error = "Only the current Shooter can roll." });
    var result = engine.Roll(new DiceRoll(request.Die1, request.Die2));
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
    var allowDevToken = string.Equals(
        config["StreetDice:AllowDevVoiceToken"] ?? Environment.GetEnvironmentVariable("STREET_DICE_ALLOW_DEV_VOICE_TOKEN"),
        "true",
        StringComparison.OrdinalIgnoreCase);
    if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(domain))
    {
        return Results.Json(new
        {
            configured = false,
            requiresVivoxAccessTokenConfiguration = true,
            channel = $"street-dice-{gameId}",
            message = "Vivox issuer, key, and domain are required before voice tokens can be issued."
        }, statusCode: StatusCodes.Status501NotImplemented);
    }

    if (!allowDevToken)
    {
        return Results.Json(new
        {
            configured = true,
            requiresVivoxTokenSigner = true,
            channel = $"street-dice-{gameId}",
            message = "Vivox configuration is present, but production token signing is not enabled in this prototype."
        }, statusCode: StatusCodes.Status501NotImplemented);
    }

    return Results.Ok(new
    {
        configured = true,
        channel = $"street-dice-{gameId}",
        participant = request.PlayerId,
        token = $"dev-token:{issuer}:{gameId}:{request.PlayerId}",
        expiresInSeconds = 300
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

public sealed record JoinRequest(string PlayerName, string? PlayerId = null);
public sealed record DiceColorRequest(string PlayerId, string PlayerSessionToken, DiceColor Color);
public sealed record OpenShotRequest(string ShooterId, string ShooterSessionToken, string CatcherId, int Amount);
public sealed record SideBetRequest(string PlayerId, string PlayerSessionToken, SideBetType Type, int Amount, int? TargetPointNumber = null);
public sealed record FadeRequest(string CatcherId, string PlayerSessionToken);
public sealed record RollRequest(string ShooterId, string PlayerSessionToken, int Die1, int Die2);
public sealed record CeeLoRollRequest(int Die1, int Die2, int Die3);
public sealed record ShooterDecisionRequest(string ShooterId, string PlayerSessionToken);
public sealed record BotFillRequest(int TargetPlayers = 5);
public sealed record VoiceAccessRequest(string PlayerId, string PlayerSessionToken);
