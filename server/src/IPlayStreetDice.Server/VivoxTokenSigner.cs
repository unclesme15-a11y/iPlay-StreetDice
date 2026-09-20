using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public sealed class VivoxTokenSigner
{
    private readonly string _issuer;
    private readonly string _domain;
    private readonly string _key;
    private readonly string _environmentId;

    private VivoxTokenSigner(string issuer, string domain, string key, string environmentId)
    {
        _issuer = issuer;
        _domain = domain.TrimStart('@');
        _key = key;
        _environmentId = environmentId;
    }

    public static VivoxTokenSigner? FromConfiguration(IConfiguration configuration)
    {
        var issuer = configuration["Vivox:Issuer"] ?? Environment.GetEnvironmentVariable("IPLAY_VIVOX_ISSUER");
        var domain = configuration["Vivox:Domain"] ?? Environment.GetEnvironmentVariable("IPLAY_VIVOX_DOMAIN");
        var key = configuration["Vivox:SigningKey"] ?? Environment.GetEnvironmentVariable("IPLAY_VIVOX_SIGNING_KEY");
        var environmentId = configuration["Vivox:UnityEnvironmentId"] ?? Environment.GetEnvironmentVariable("IPLAY_UNITY_ENVIRONMENT_ID");
        return new[] { issuer, domain, key, environmentId }.Any(string.IsNullOrWhiteSpace)
            ? null : new VivoxTokenSigner(issuer!.Trim(), domain!.Trim(), key!.Trim(), environmentId!.Trim());
    }

    public SignedVivoxToken Sign(string gameId, string playerId, string action, DateTimeOffset? now = null,
        string? fromUserUri = null, string? channelUri = null)
    {
        if (action is not ("login" or "join" or "join_muted"))
            throw new ArgumentException("Unsupported voice action.", nameof(action));
        if (!IsSafeSegment(gameId) || !IsSafeSegment(playerId))
            throw new ArgumentException("Invalid table or participant.");

        var channelName = $"street-dice-{gameId}";
        var expectedChannelUri = $"sip:confctl-g-{_issuer}.{channelName}.{_environmentId}@{_domain}";
        if (!string.IsNullOrWhiteSpace(channelUri)
            && !string.Equals(channelUri, expectedChannelUri, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Requested voice channel does not match this table.");
        var fromUri = string.IsNullOrWhiteSpace(fromUserUri)
            ? $"sip:.{_issuer}.{playerId}.{_environmentId}.@{_domain}"
            : fromUserUri.Trim();
        if (!fromUri.StartsWith($"sip:.{_issuer}.", StringComparison.OrdinalIgnoreCase)
            || !fromUri.EndsWith($".{_environmentId}.@{_domain}", StringComparison.OrdinalIgnoreCase)
            || fromUri.Length > 256)
            throw new ArgumentException("Requested voice identity does not belong to this Vivox environment.");
        var payload = new Dictionary<string, object>
        {
            ["iss"] = _issuer,
            ["exp"] = (now ?? DateTimeOffset.UtcNow).AddSeconds(300).ToUnixTimeSeconds(),
            ["vxa"] = action,
            ["vxi"] = Guid.NewGuid().ToString("N"),
            ["f"] = fromUri
        };
        if (action != "login") payload["t"] = expectedChannelUri;
        var unsigned = Encode("{}") + "." + Encode(JsonSerializer.Serialize(payload));
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_key));
        var signature = Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsigned)));
        return new SignedVivoxToken(channelName, expectedChannelUri, unsigned + "." + signature);
    }

    private static bool IsSafeSegment(string value) => !string.IsNullOrWhiteSpace(value)
        && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');

    private static string Encode(string value) => Encode(Encoding.UTF8.GetBytes(value));

    private static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

public sealed record SignedVivoxToken(string ChannelName, string ChannelUri, string Token);
