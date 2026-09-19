using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IPlayStreetDice.Server.Core.Voice;

/// <summary>
/// Signs Vivox unified access tokens per the publicly documented format:
/// base64url(header) + "." + base64url(payload) + "." + base64url(HMAC-SHA256(header + "." + payload, key)).
/// Header is always the static, empty JSON object. Not verified against a live
/// Vivox account (none available in this environment) - verified here means the
/// token structure and signature match the documented spec.
/// </summary>
public static class VivoxAccessTokenGenerator
{
    private const string EncodedEmptyHeader = "e30"; // base64url of "{}"

    public static string GenerateJoinToken(string issuer, string key, string domain, string username, string channelName, TimeSpan lifetime)
    {
        var from = BuildUserUri(issuer, domain, username);
        var to = BuildChannelUri(issuer, domain, channelName);
        var payload = BuildPayload(issuer, "join", from, to, lifetime);
        return Sign(key, payload);
    }

    private static string BuildUserUri(string issuer, string domain, string username)
        => $"sip:.{issuer}.{Sanitize(username)}.@{domain}";

    private static string BuildChannelUri(string issuer, string domain, string channelName)
        => $"sip:confctl-g-{issuer}.{Sanitize(channelName)}@{domain}";

    private static string Sanitize(string value)
    {
        var chars = value.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_').ToArray();
        return new string(chars);
    }

    private static Dictionary<string, object> BuildPayload(string issuer, string action, string from, string to, TimeSpan lifetime)
    {
        return new Dictionary<string, object>
        {
            ["iss"] = issuer,
            ["vxi"] = RandomNumberGenerator.GetInt32(1, int.MaxValue),
            ["vxa"] = action,
            ["f"] = from,
            ["t"] = to,
            ["exp"] = DateTimeOffset.UtcNow.Add(lifetime).ToUnixTimeSeconds()
        };
    }

    private static string Sign(string key, Dictionary<string, object> payload)
    {
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signingInput = $"{EncodedEmptyHeader}.{encodedPayload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
        return $"{signingInput}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
