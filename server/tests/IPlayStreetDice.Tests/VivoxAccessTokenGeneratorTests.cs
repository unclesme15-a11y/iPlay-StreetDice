using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IPlayStreetDice.Server.Core.Voice;

namespace IPlayStreetDice.Tests;

public class VivoxAccessTokenGeneratorTests
{
    [Fact]
    public void TokenHasThreeBase64UrlSegments()
    {
        var token = VivoxAccessTokenGenerator.GenerateJoinToken(
            issuer: "iplay-streetdice-dev",
            key: "test-secret-key",
            domain: "tla.vivox.com",
            username: "player-1",
            channelName: "iplay.streetdice.table.abc123",
            lifetime: TimeSpan.FromSeconds(90));

        var segments = token.Split('.');
        Assert.Equal(3, segments.Length);
        foreach (var segment in segments)
        {
            Assert.False(string.IsNullOrEmpty(segment));
            Assert.DoesNotContain('+', segment);
            Assert.DoesNotContain('/', segment);
            Assert.DoesNotContain('=', segment);
        }
    }

    [Fact]
    public void HeaderDecodesToEmptyJsonObject()
    {
        var token = VivoxAccessTokenGenerator.GenerateJoinToken(
            "iplay-streetdice-dev", "test-secret-key", "tla.vivox.com", "player-1", "table-1", TimeSpan.FromSeconds(90));

        var header = Base64UrlDecodeToString(token.Split('.')[0]);
        Assert.Equal("{}", header);
    }

    [Fact]
    public void PayloadContainsExpectedClaims()
    {
        var before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var token = VivoxAccessTokenGenerator.GenerateJoinToken(
            issuer: "iplay-streetdice-dev",
            key: "test-secret-key",
            domain: "tla.vivox.com",
            username: "player-1",
            channelName: "iplay.streetdice.table.abc123",
            lifetime: TimeSpan.FromSeconds(90));
        var after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var payloadJson = Base64UrlDecodeToString(token.Split('.')[1]);
        using var payload = JsonDocument.Parse(payloadJson);
        var root = payload.RootElement;

        Assert.Equal("iplay-streetdice-dev", root.GetProperty("iss").GetString());
        Assert.Equal("join", root.GetProperty("vxa").GetString());
        Assert.Equal("sip:.iplay-streetdice-dev.player-1.@tla.vivox.com", root.GetProperty("f").GetString());
        Assert.Equal("sip:confctl-g-iplay-streetdice-dev.iplay_streetdice_table_abc123@tla.vivox.com", root.GetProperty("t").GetString());
        Assert.True(root.GetProperty("vxi").GetInt64() > 0);

        var exp = root.GetProperty("exp").GetInt64();
        Assert.InRange(exp, before + 89, after + 91);
    }

    [Fact]
    public void SignatureVerifiesAgainstTheSameKey()
    {
        const string key = "test-secret-key";
        var token = VivoxAccessTokenGenerator.GenerateJoinToken(
            "iplay-streetdice-dev", key, "tla.vivox.com", "player-1", "table-1", TimeSpan.FromSeconds(90));

        var segments = token.Split('.');
        var signingInput = $"{segments[0]}.{segments[1]}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var expectedSignature = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput)));

        Assert.Equal(expectedSignature, segments[2]);
    }

    [Fact]
    public void SignatureDoesNotVerifyAgainstADifferentKey()
    {
        var token = VivoxAccessTokenGenerator.GenerateJoinToken(
            "iplay-streetdice-dev", "correct-key", "tla.vivox.com", "player-1", "table-1", TimeSpan.FromSeconds(90));

        var segments = token.Split('.');
        var signingInput = $"{segments[0]}.{segments[1]}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("wrong-key"));
        var wrongSignature = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput)));

        Assert.NotEqual(wrongSignature, segments[2]);
    }

    [Fact]
    public void UsernameWithUnsafeCharactersIsSanitizedIntoTheUri()
    {
        var token = VivoxAccessTokenGenerator.GenerateJoinToken(
            "iplay-streetdice-dev", "test-secret-key", "tla.vivox.com", "Player One!", "table-1", TimeSpan.FromSeconds(90));

        var payloadJson = Base64UrlDecodeToString(token.Split('.')[1]);
        using var payload = JsonDocument.Parse(payloadJson);

        Assert.Equal("sip:.iplay-streetdice-dev.Player_One_.@tla.vivox.com", payload.RootElement.GetProperty("f").GetString());
    }

    private static string Base64UrlDecodeToString(string segment)
    {
        var padded = segment.Replace('-', '+').Replace('_', '/');
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
