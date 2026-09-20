using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using IPlayStreetDice.Server.Core;
using IPlayStreetDice.Server.Core.Persistence;

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

app.Use(async (context, next) =>
{
    SemaphoreSlim? gate = null;
    bool acquired = false;
    try
    {
        var parts = context.Request.Path.Value?.Split('/') ?? Array.Empty<string>();
        if (parts.Length > 3 && parts[1] == "api" && parts[2] == "street-dice" && parts[3] != "create")
        {
            gate = context.RequestServices.GetRequiredService<StreetDiceTableStore>().Gate(parts[3]);
            await gate.WaitAsync(context.RequestAborted);
            acquired = true;
        }
        await next(context);
    }
    catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = exception.Message });
    }
    finally { if (acquired) gate!.Release(); }
});

app.MapPost("/api/street-dice/{gameId}/pass", (string gameId, PlayerActionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound();
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    lock (engine) { engine.PassDice(request.PlayerId); }
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/sell", (string gameId, PlayerActionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound();
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    var now = DateTimeOffset.UtcNow;
    var sale = engine.SellDice(request.PlayerId, now);
    return Results.Ok(new { sale, remainingMilliseconds = 5000, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/sell/bid", (string gameId, DiceSaleBidRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound();
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    var bid = engine.BidForDice(request.PlayerId, request.Amount, DateTimeOffset.UtcNow);
    return Results.Ok(new { bid, sale = engine.State.DiceSale, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/leave", (string gameId, PlayerActionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound();
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    lock (engine) { engine.LeaveGame(request.PlayerId); }
    store.RevokePlayerSession(gameId, request.PlayerId);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/presence/disconnect", (string gameId, PlayerActionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out _)) return Results.NotFound();
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    store.MarkDisconnected(gameId, request.PlayerId, DateTimeOffset.UtcNow);
    return Results.Ok(new { reconnectGraceSeconds = 20 });
});

app.MapPost("/api/street-dice/{gameId}/presence/reconnect", (string gameId, PlayerActionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound();
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    if (!store.TryReconnect(gameId, request.PlayerId, DateTimeOffset.UtcNow))
        return Results.Json(new { error = "Reconnect window expired." }, statusCode: StatusCodes.Status409Conflict);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/presence/heartbeat", (string gameId, PlayerActionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out _)) return Results.NotFound();
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    return store.TryReconnect(gameId, request.PlayerId, DateTimeOffset.UtcNow)
        ? Results.Ok(new { connected = true })
        : Results.Json(new { error = "Reconnect window expired." }, statusCode: StatusCodes.Status409Conflict);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "iPlay Street Dice Backend",
    serverAuthoritativeRules = true,
    serverAuthoritativeRolls = true,
    supportedPlayers = new { min = 2, max = 5 },
    supportedDiceColors = Enum.GetNames<DiceColor>(),
    reservedHotDiceColors = new[] { "Red", "Orange" }
}));

app.MapPost("/api/street-dice/create", (StreetDiceTableStore store) =>
{
    var engine = store.CreateGame();
    return Results.Ok(new { gameId = engine.State.GameId, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/join", (string gameId, JoinRequest request, StreetDiceTableStore store, IHostEnvironment environment) =>
{
    if (!environment.IsDevelopment()) return Results.StatusCode(StatusCodes.Status403Forbidden);
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    var playerId = string.IsNullOrWhiteSpace(request.PlayerId) ? Guid.NewGuid().ToString("N") : request.PlayerId.Trim();
    var player = engine.AddPlayer(playerId, request.PlayerName.Trim());
    var sessionToken = store.CreateOrReplacePlayerSession(gameId, player.Id);
    return Results.Ok(new { playerId = player.Id, playerSessionToken = sessionToken, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/join-real", (string gameId, JoinRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out _)) return Results.NotFound(new { error = "Game not found." });
    var joined = store.JoinRealPlayer(gameId, request.PlayerName);
    return Results.Ok(new { playerId = joined.Player.Id, playerSessionToken = joined.Token, state = joined.State });
});

app.MapPost("/api/street-dice/{gameId}/dice-color", (string gameId, DiceColorRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    engine.SelectDiceColor(request.PlayerId, request.Color);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/wallet", (string gameId, PlayerActionRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();
    var player = engine.State.FindPlayer(request.PlayerId);
    return player == null ? Results.NotFound(new { error = "Player not found." })
        : Results.Ok(new { playerId = player.Id, balance = player.Balance,
            availableBalance = engine.AvailableBalance(player.Id) });
});

app.MapPost("/api/street-dice/{gameId}/shot", (string gameId, OpenShotRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.ShooterId, request.ShooterSessionToken)) return Results.Unauthorized();
    engine.OpenShot(request.ShooterId, request.CatcherId, request.Amount);
    return Results.Ok(new { state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/side-bet", () => Results.Json(
    new { error = "Use paired /wager/offer and /wager/accept before the shooter deadline." },
    statusCode: StatusCodes.Status410Gone));

app.MapPost("/api/street-dice/{gameId}/wager/offer", (string gameId, PeerWagerOfferRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.FromId, request.PlayerSessionToken)) return Results.Unauthorized();
    var now = DateTimeOffset.UtcNow;
    var offer = engine.OfferPeerWager(request.FromId, request.ToId, request.Outcome, request.Number, request.Amount, now);
    return Results.Ok(new { offer, wagers = engine.PeerWagers, bettingWindow = engine.CurrentBettingWindow(now), state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/wager/accept", (string gameId, PeerWagerAcceptRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.RecipientId, request.PlayerSessionToken)) return Results.Unauthorized();
    var now = DateTimeOffset.UtcNow;
    var offer = engine.AcceptPeerWager(request.RecipientId, request.OfferId, now);
    return Results.Ok(new { offer, wagers = engine.PeerWagers, bettingWindow = engine.CurrentBettingWindow(now), state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/wager/add-on", (string gameId, PeerWagerAddOnRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.BettorId, request.PlayerSessionToken)) return Results.Unauthorized();
    var now = DateTimeOffset.UtcNow;
    var offer = engine.OfferPeerAddOn(request.BettorId, request.SourceOfferId, request.Kind, now, request.Amount);
    return Results.Ok(new { offer, wagers = engine.PeerWagers, bettingWindow = engine.CurrentBettingWindow(now), state = engine.State });
});

app.MapPost("/api/cee-lo/evaluate", (CeeLoRollRequest request) =>
{
    var result = CeeLoRules.Evaluate(new CeeLoRoll(request.Die1, request.Die2, request.Die3));
    return Results.Ok(new { result });
});

app.MapPost("/api/street-dice/{gameId}/fade", () => Results.Json(
    new { error = "Use /roll/fade with the pending roll ID." }, statusCode: StatusCodes.Status410Gone));

app.MapPost("/api/street-dice/{gameId}/roll", () => Results.Json(
    new { error = "Client-selected dice faces are no longer accepted. Use /roll/prepare and /roll/commit." },
    statusCode: StatusCodes.Status410Gone));

app.MapPost("/api/street-dice/{gameId}/roll/prepare", (string gameId, PhysicalRollPrepareRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.ShooterId, request.PlayerSessionToken)) return Results.Unauthorized();
    var seed = RandomNumberGenerator.GetInt32(int.MaxValue);
    var launchSeed = RandomNumberGenerator.GetInt32(int.MaxValue);
    var now = DateTimeOffset.UtcNow;
    var prepared = engine.PreparePhysicalRoll(request.ShooterId, new DiceGesture(request.Power, request.Aim, request.LeftHanded),
        now, seed, launchSeed);
    return Results.Ok(new { prepared.RollId, prepared.FadeDeadline,
        remainingFadeMilliseconds = Math.Max(0, (prepared.FadeDeadline - DateTimeOffset.UtcNow).TotalMilliseconds),
        launchPoses = PhysicalRollTransport.Launch(prepared.LaunchPoses) });
});

app.MapPost("/api/street-dice/{gameId}/roll/fade", (string gameId, PhysicalRollFadeRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.CatcherId, request.PlayerSessionToken)) return Results.Unauthorized();
    var resolution = engine.FadePhysicalRoll(request.CatcherId, request.RollId, DateTimeOffset.UtcNow);
    return Results.Ok(new { result = resolution, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/roll/commit", (string gameId, PhysicalRollCommitRequest request, StreetDiceTableStore store) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (!store.ValidatePlayerSession(gameId, request.ShooterId, request.PlayerSessionToken)) return Results.Unauthorized();
    var committed = engine.CommitPhysicalRoll(request.ShooterId, request.RollId, DateTimeOffset.UtcNow);
    store.RecordCommittedRoll(gameId, request.ShooterId, committed);
    return Results.Ok(new { committed.RollId, result = committed.Resolution,
        faces = committed.Throw.Faces, frames = PhysicalRollTransport.Frames(committed.Throw), state = engine.State });
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

app.MapPost("/api/street-dice/{gameId}/bots/fill", (string gameId, BotFillRequest request, StreetDiceTableStore store, IHostEnvironment environment) =>
{
    if (!environment.IsDevelopment()) return Results.StatusCode(StatusCodes.Status403Forbidden);
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    var bots = engine.FillBots(request.TargetPlayers);
    var sessions = bots.Select(bot => new
    {
        playerId = bot.Id,
        playerSessionToken = store.CreateOrReplacePlayerSession(gameId, bot.Id)
    });
    return Results.Ok(new { bots = sessions, state = engine.State });
});

app.MapPost("/api/street-dice/{gameId}/bots/advance", () => Results.Json(
    new { error = "Bot rolls must use the server-owned physical roll lifecycle." },
    statusCode: StatusCodes.Status410Gone));

app.MapPost("/api/street-dice/{gameId}/voice/access-token", (string gameId, VoiceAccessRequest request, StreetDiceTableStore store, IConfiguration config) =>
{
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    if (engine.State.FindPlayer(request.PlayerId) is not { HasLeft: false })
        return Results.NotFound(new { error = "Player is not seated at this table." });
    if (!store.ValidatePlayerSession(gameId, request.PlayerId, request.PlayerSessionToken)) return Results.Unauthorized();

    var signing = VivoxTokenSigner.FromConfiguration(config);
    if (signing == null)
    {
        return Results.Json(new
        {
            configured = false,
            requiresVivoxAccessTokenConfiguration = true,
            channel = $"street-dice-{gameId}",
            message = "Vivox issuer, key, and domain are required before voice tokens can be issued."
        }, statusCode: StatusCodes.Status501NotImplemented);
    }

    var signed = signing.Sign(gameId, request.PlayerId, request.Action,
        fromUserUri: request.FromUserUri, channelUri: request.ChannelUri);
    return Results.Ok(new
    {
        configured = true,
        channel = signed.ChannelName,
        channelUri = signed.ChannelUri,
        participant = request.PlayerId,
        token = signed.Token,
        accessToken = signed.Token,
        expiresInSeconds = 300
    });
});

app.MapGet("/api/street-dice/{gameId}", (string gameId, int? afterRoll, StreetDiceTableStore store) =>
{
    var now = DateTimeOffset.UtcNow;
    store.ExpireDisconnected(gameId, now);
    var committed = store.LastCommittedRoll(gameId);
    if (!store.TryGet(gameId, out var engine)) return Results.NotFound(new { error = "Game not found." });
    engine.ResolveDiceSale(now);
    var sale = engine.State.DiceSale;
    double saleRemainingMilliseconds = sale is { IsOpen: true }
        ? Math.Max(0, sale.EndsAtUnixMilliseconds - now.ToUnixTimeMilliseconds()) : 0;
    return Results.Ok(new { state = engine.State, pendingRoll = engine.CurrentPhysicalRoll(now),
        wagers = engine.PeerWagers, bettingWindow = engine.CurrentBettingWindow(now), saleRemainingMilliseconds,
        lastCommittedRoll = committed?.Sequence > (afterRoll ?? 0) ? committed : null });
});

app.Run();

public sealed class StreetDiceTableStore
{
    private readonly ConcurrentDictionary<string, StreetDiceGameEngine> _games = new();
    private readonly ConcurrentDictionary<string, string> _playerSessions = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _disconnected = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastSeen = new();
    private readonly ConcurrentDictionary<string, CommittedRollBroadcast> _committedRolls = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new();
    public SemaphoreSlim Gate(string gameId) => _gates.GetOrAdd(gameId, _ => new SemaphoreSlim(1, 1));

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

    public RealPlayerJoin JoinRealPlayer(string gameId, string playerName, DateTimeOffset? now = null)
    {
        if (!_games.TryGetValue(gameId, out var engine)) throw new InvalidOperationException("Game not found.");
        if (string.IsNullOrWhiteSpace(playerName)) throw new ArgumentException("Player name is required.");
        var playerId = Enumerable.Range(1, 5).Select(index => $"p{index}")
            .FirstOrDefault(id => engine.State.FindPlayer(id) == null)
            ?? throw new InvalidOperationException("Table is full.");
        var player = engine.AddPlayer(playerId, playerName.Trim());
        engine.State.ShooterId ??= playerId;
        var token = CreateOrReplacePlayerSession(gameId, playerId);
        _lastSeen[SessionKey(gameId, playerId)] = now ?? DateTimeOffset.UtcNow;
        return new RealPlayerJoin(player, token, engine.State);
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

    public void RevokePlayerSession(string gameId, string playerId)
    {
        var key = SessionKey(gameId, playerId);
        _playerSessions.TryRemove(key, out _);
        _lastSeen.TryRemove(key, out _);
        _disconnected.TryRemove(key, out _);
    }

    public void RecordCommittedRoll(string gameId, string shooterId, CommittedPhysicalRoll committed)
    {
        var previous = LastCommittedRoll(gameId);
        if (previous?.RollId == committed.RollId) return;
        _committedRolls[gameId] = new CommittedRollBroadcast((previous?.Sequence ?? 0) + 1,
            shooterId, committed.RollId, committed.Throw.Faces.ToArray(),
            PhysicalRollTransport.Frames(committed.Throw));
    }

    public CommittedRollBroadcast? LastCommittedRoll(string gameId) =>
        _committedRolls.TryGetValue(gameId, out var roll) ? roll : null;

    public void MarkDisconnected(string gameId, string playerId, DateTimeOffset now)
    {
        if (!_games.TryGetValue(gameId, out var engine) || engine.State.FindPlayer(playerId) is not { HasLeft: false })
            throw new InvalidOperationException("Player is not seated at this table.");
        _disconnected.TryAdd(SessionKey(gameId, playerId), now);
    }

    public bool TryReconnect(string gameId, string playerId, DateTimeOffset now)
    {
        if (!_games.TryGetValue(gameId, out var engine)) return false;
        var player = engine.State.FindPlayer(playerId);
        if (player is not { HasLeft: false }) return false;
        var key = SessionKey(gameId, playerId);
        if (_lastSeen.TryGetValue(key, out var lastSeen) && now - lastSeen > TimeSpan.FromSeconds(20))
        {
            ExpireDisconnected(gameId, now);
            return false;
        }
        if (!_disconnected.TryGetValue(key, out var disconnectedAt))
        {
            if (_lastSeen.ContainsKey(key)) _lastSeen[key] = now;
            return true;
        }
        if (now - disconnectedAt > TimeSpan.FromSeconds(20))
        {
            ExpireDisconnected(gameId, now);
            return false;
        }
        if (!_disconnected.TryRemove(key, out _)) return false;
        _lastSeen[key] = now;
        return true;
    }

    public void ExpireDisconnected(string gameId, DateTimeOffset now)
    {
        if (!_games.TryGetValue(gameId, out var engine)) return;
        foreach (var entry in _lastSeen)
        {
            if (!entry.Key.StartsWith(gameId + ":", StringComparison.OrdinalIgnoreCase)
                || now - entry.Value <= TimeSpan.FromSeconds(20)) continue;
            _disconnected.TryAdd(entry.Key, entry.Value);
        }
        foreach (var entry in _disconnected)
        {
            if (!entry.Key.StartsWith(gameId + ":", StringComparison.OrdinalIgnoreCase)
                || now - entry.Value <= TimeSpan.FromSeconds(20)
                || !_disconnected.TryRemove(entry.Key, out _)) continue;
            var playerId = entry.Key[(gameId.Length + 1)..];
            engine.LeaveGame(playerId);
            _playerSessions.TryRemove(entry.Key, out _);
            _lastSeen.TryRemove(entry.Key, out _);
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
public sealed record PlayerActionRequest(string PlayerId, string PlayerSessionToken);
public sealed record DiceSaleBidRequest(string PlayerId, string PlayerSessionToken, int Amount);
public sealed record PeerWagerAddOnRequest(string BettorId, string PlayerSessionToken, int SourceOfferId, IPlay.Demo.WagerAddOnKind Kind, int Amount = 0);
public sealed record DiceColorRequest(string PlayerId, string PlayerSessionToken, DiceColor Color);
public sealed record OpenShotRequest(string ShooterId, string ShooterSessionToken, string CatcherId, int Amount);
public sealed record SideBetRequest(string PlayerId, string PlayerSessionToken, SideBetType Type, int Amount, int? TargetPointNumber = null);
public sealed record PeerWagerOfferRequest(string FromId, string PlayerSessionToken, string ToId,
    IPlay.Demo.WagerOutcome Outcome, int Number, int Amount);
public sealed record PeerWagerAcceptRequest(string RecipientId, string PlayerSessionToken, int OfferId);
public sealed record PhysicalRollPrepareRequest(string ShooterId, string PlayerSessionToken, float Power, float Aim, bool LeftHanded = false);
public sealed record PhysicalRollFadeRequest(string CatcherId, string PlayerSessionToken, string RollId);
public sealed record PhysicalRollCommitRequest(string ShooterId, string PlayerSessionToken, string RollId);
public sealed record CeeLoRollRequest(int Die1, int Die2, int Die3);
public sealed record ShooterDecisionRequest(string ShooterId, string PlayerSessionToken);
public sealed record BotFillRequest(int TargetPlayers = 5);
public sealed record VoiceAccessRequest(string PlayerId, string PlayerSessionToken, string Action = "join",
    string? FromUserUri = null, string? ChannelUri = null);
public sealed record CommittedRollBroadcast(int Sequence, string ShooterId, string RollId,
    int[] Faces, IReadOnlyList<PhysicalRollFrame> Frames);
public sealed record RealPlayerJoin(StreetDicePlayer Player, string Token, StreetDiceGameState State);
