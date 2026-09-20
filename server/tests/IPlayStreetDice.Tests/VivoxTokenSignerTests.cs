using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace IPlayStreetDice.Tests;

public sealed class VivoxTokenSignerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DoesNotSignUntilAllServerConfigurationIsPresent()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Vivox:Issuer"] = "issuer",
            ["Vivox:Domain"] = "example.test",
            ["Vivox:SigningKey"] = "test-secret"
        }).Build();
        Assert.Null(VivoxTokenSigner.FromConfiguration(configuration));
    }

    [Theory]
    [InlineData("login", false)]
    [InlineData("join", true)]
    [InlineData("join_muted", true)]
    public void SignsOnlyThePlayersTableAndAllowedAction(string action, bool hasChannel)
    {
        const string key = "test-secret";
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Vivox:Issuer"] = "issuer",
            ["Vivox:Domain"] = "@example.test",
            ["Vivox:SigningKey"] = key,
            ["Vivox:UnityEnvironmentId"] = "environment"
        }).Build();
        var signer = VivoxTokenSigner.FromConfiguration(configuration)!;
        var signed = signer.Sign("table1", "player1", action, Now);
        var pieces = signed.Token.Split('.');
        Assert.Equal(3, pieces.Length);
        using var payload = JsonDocument.Parse(Decode(pieces[1]));
        Assert.Equal(action, payload.RootElement.GetProperty("vxa").GetString());
        Assert.Equal("sip:.issuer.player1.environment.@example.test", payload.RootElement.GetProperty("f").GetString());
        Assert.Equal(hasChannel, payload.RootElement.TryGetProperty("t", out _));
        Assert.Equal(Now.AddMinutes(5).ToUnixTimeSeconds(), payload.RootElement.GetProperty("exp").GetInt64());
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        Assert.Equal(pieces[2], Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(pieces[0] + "." + pieces[1]))));
        Assert.Throws<ArgumentException>(() => signer.Sign("table1", "player1", "delete", Now));
        Assert.Throws<ArgumentException>(() => signer.Sign("table/other", "player1", "join", Now));
        Assert.Throws<ArgumentException>(() => signer.Sign("table1", "player1", "join", Now,
            fromUserUri: "sip:.other.player1.environment.@example.test"));
        Assert.Throws<ArgumentException>(() => signer.Sign("table1", "player1", "join", Now,
            channelUri: "sip:confctl-g-issuer.other-table.environment@example.test"));
    }

    private static byte[] Decode(string value) => Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/')
        .PadRight((value.Length + 3) / 4 * 4, '='));

    private static string Encode(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
