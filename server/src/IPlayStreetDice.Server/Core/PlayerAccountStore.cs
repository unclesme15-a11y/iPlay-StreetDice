using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;

namespace IPlayStreetDice.Server.Core;

/// <summary>
/// Registration/login for persistent PlayerAccounts, plus session tokens
/// scoped to the account itself (separate from a table's PlayerSessionToken
/// -- an account outlives any one table). Password hashing and the
/// fixed-time token check mirror StreetDiceTableStore's existing session
/// pattern in Program.cs.
/// </summary>
public sealed class PlayerAccountStore
{
    private const int Pbkdf2Iterations = 210_000;
    private const int HashSizeBytes = 32;

    private readonly ConcurrentDictionary<string, PlayerAccount> _accountsById = new();
    private readonly ConcurrentDictionary<string, string> _idByUsername = new(StringComparer.OrdinalIgnoreCase);
    // accountId -> current session token. One active session per account,
    // same "latest login wins" shape as StreetDiceTableStore's per-seat
    // player sessions.
    private readonly ConcurrentDictionary<string, string> _accountSessions = new();

    public PlayerAccount Register(string username, string password)
    {
        username = (username ?? "").Trim();
        if (username.Length is < 3 or > 24) throw new ArgumentException("Username must be 3-24 characters.");
        if (string.IsNullOrEmpty(password) || password.Length < 6) throw new ArgumentException("Password must be at least 6 characters.");
        if (_idByUsername.ContainsKey(username)) throw new InvalidOperationException("Username is already taken.");

        var (hash, salt) = HashPassword(password);
        var account = new PlayerAccount(Guid.NewGuid().ToString("N"), username, hash, salt);
        if (!_idByUsername.TryAdd(username, account.Id)) throw new InvalidOperationException("Username is already taken.");
        _accountsById[account.Id] = account;
        return account;
    }

    public (PlayerAccount Account, string Token) Login(string username, string password)
    {
        username = (username ?? "").Trim();
        if (!_idByUsername.TryGetValue(username, out var accountId) || !_accountsById.TryGetValue(accountId, out var account))
            throw new InvalidOperationException("Incorrect username or password.");
        if (!VerifyPassword(password, account.PasswordHash, account.PasswordSalt))
            throw new InvalidOperationException("Incorrect username or password.");

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        _accountSessions[account.Id] = token;
        return (account, token);
    }

    public bool TryGet(string accountId, out PlayerAccount account) => _accountsById.TryGetValue(accountId, out account!);

    public bool ValidateSession(string accountId, string accountSessionToken)
    {
        if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(accountSessionToken)) return false;
        if (accountSessionToken.Length != 64) return false;
        try
        {
            return _accountSessions.TryGetValue(accountId, out var expected)
                && CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(expected), Convert.FromHexString(accountSessionToken));
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>Credits one finished shot to one seated player. Only the shooter and catcher
    /// can win; opponentKey (the other side of the shot) limits how many win bonuses one
    /// opponent can hand out per day. The first DailyBonusShots shots each day are multiplied.
    /// There is no daily cap. Returns the XP granted.</summary>
    public int RecordShot(string accountId, bool shooterOrCatcher, bool won, string? opponentKey, int amountWon, DateOnly today)
    {
        if (!_accountsById.TryGetValue(accountId, out var account)) return 0;
        lock (account)
        {
            if (account.DailyDay != today)
            {
                account.DailyDay = today;
                account.DailyShots = 0;
                account.DailyWinBonusesByOpponent.Clear();
            }
            if (shooterOrCatcher) account.ShotsPlayed++;
            var winBonusAvailable = false;
            if (won && opponentKey is not null)
            {
                account.Wins++;
                account.DailyWinBonusesByOpponent.TryGetValue(opponentKey, out var bonusesToday);
                winBonusAvailable = bonusesToday < RankLadder.WinBonusesPerOpponentPerDay;
                if (winBonusAvailable) account.DailyWinBonusesByOpponent[opponentKey] = bonusesToday + 1;
            }
            var xp = RankLadder.XpForShot(shooterOrCatcher, won, winBonusAvailable, amountWon);
            if (account.DailyShots < RankLadder.DailyBonusShots) xp *= RankLadder.DailyBonusMultiplier;
            account.DailyShots++;
            account.Xp += xp;
            return xp;
        }
    }

    public IReadOnlyCollection<PlayerAccount> All => _accountsById.Values.ToList();

    // Session tokens are persisted alongside the account: without them, every server restart
    // silently invalidated every saved login, and players dropped to Level 1 without being told.
    public List<PersistedAccount> Snapshot() => _accountsById.Values.Select(a => new PersistedAccount(
        a.Id, a.Username, a.PasswordHash, a.PasswordSalt, a.Wins, a.TrophyNotes.ToList(),
        _accountSessions.TryGetValue(a.Id, out var token) ? token : null,
        a.Xp, a.ShotsPlayed, a.DailyShots, a.DailyDay,
        new Dictionary<string, int>(a.DailyWinBonusesByOpponent))).ToList();

    public void Restore(IEnumerable<PersistedAccount> accounts)
    {
        foreach (var persisted in accounts)
        {
            var account = new PlayerAccount(persisted.Id, persisted.Username, persisted.PasswordHash, persisted.PasswordSalt)
            {
                Wins = persisted.Wins,
                Xp = persisted.Xp,
                ShotsPlayed = persisted.ShotsPlayed,
                DailyShots = persisted.DailyShots,
                DailyDay = persisted.DailyDay
            };
            foreach (var (opponent, bonuses) in persisted.DailyWinBonusesByOpponent ?? new())
                account.DailyWinBonusesByOpponent[opponent] = bonuses;
            foreach (var note in persisted.TrophyNotes) account.TrophyNotes.Add(note);
            _accountsById[account.Id] = account;
            _idByUsername[account.Username] = account.Id;
            if (!string.IsNullOrEmpty(persisted.SessionToken)) _accountSessions[account.Id] = persisted.SessionToken;
        }
    }

    private static (string Hash, string Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, HashSizeBytes);
        return (Convert.ToHexString(hash), Convert.ToHexString(salt));
    }

    private static bool VerifyPassword(string password, string expectedHash, string saltHex)
    {
        var salt = Convert.FromHexString(saltHex);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, HashSizeBytes);
        return CryptographicOperations.FixedTimeEquals(hash, Convert.FromHexString(expectedHash));
    }
}

// Everything after TrophyNotes is optional so account files saved by earlier
// builds still load (they come back at 0 XP -- pre-launch test accounts only).
public sealed record PersistedAccount(string Id, string Username, string PasswordHash, string PasswordSalt, int Wins,
    List<int> TrophyNotes, string? SessionToken = null, int Xp = 0, int ShotsPlayed = 0,
    int DailyShots = 0, DateOnly? DailyDay = null, Dictionary<string, int>? DailyWinBonusesByOpponent = null);
