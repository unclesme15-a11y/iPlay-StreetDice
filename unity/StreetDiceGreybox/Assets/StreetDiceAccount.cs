using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// ─────────────────────────────────────────────────────────────────────────
//  A real player account, separate from the per-table identity everything
//  else in this file (localPlayerId, playerTokens) already uses. This is
//  what the rank ladder (server-side: RankLadder.cs, PlayerAccountStore.cs)
//  needs to mean anything -- without a persistent login, "Level 3" would
//  reset every time you closed the app.
//
//  Register/login hit the server directly (POST /api/accounts/register,
//  /login); once logged in, accountId + accountSessionToken are saved to
//  PlayerPrefs (same pattern baseUrl already uses) so you're still signed
//  in next time the app opens, and are sent along with join-real so the
//  server can link this table seat to the account -- that link is what
//  actually turns on the bet cap and prestige-bill unlocks for this table.
// ─────────────────────────────────────────────────────────────────────────

public sealed partial class StreetDiceGreyboxController
{
    private string accountId = "";
    private string accountSessionToken = "";
    private string accountUsername = "";
    private int accountLevel = 1;
    private int accountWins;
    private int accountXp;
    private int accountXpUntilNextLevel;
    private int accountShotsPlayed;
    private int accountLevelStep = 1;
    private float accountLevelProgress;
    private int accountMaxBet = 100;
    private int[] accountTrophyNotes = Array.Empty<int>();
    private bool accountPrestigeUnlocked;
    private bool accountLoggedIn;

    private string statsUsernameInput = "";
    private string statsPasswordInput = "";
    private bool statsBusy;
    private string statsError = "";

    [Serializable] private sealed class RegisterAccountRequestDto { public string username = "", password = ""; }
    [Serializable] private sealed class LoginAccountRequestDto { public string username = "", password = ""; }

    [Serializable]
    private sealed class AccountResponseDto
    {
        public string accountId = "", accountSessionToken = "", username = "";
        public int level = 1, wins, xp, xpUntilNextLevel, shotsPlayed, levelStep = 1, maxBetAtLevel = 100;
        public float levelProgress;
        public bool prestigeBillsUnlocked;
        public int[] trophyNotes;
    }

    [Serializable] private sealed class AccountErrorDto { public string error = ""; }

    /// <summary>Standalone from Post/PostOpen elsewhere in this file -- those two share
    /// gameId with every other request and only ever hand the caller a success body,
    /// falling back to the shared "result" status line on failure. Account calls need to
    /// work with no table in play at all, and need the server's actual error text (e.g.
    /// "Incorrect username or password.") surfaced on the sign-in form itself, not lost
    /// into that shared status line.</summary>
    private IEnumerator PostAccount(string path, string json, Action<string> onSuccess, Action<string> onError)
    {
        using var request = new UnityWebRequest(baseUrl + path, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        var text = request.downloadHandler.text;
        if (request.result != UnityWebRequest.Result.Success)
        {
            string message = request.error;
            if (!string.IsNullOrEmpty(text))
            {
                try { message = JsonUtility.FromJson<AccountErrorDto>(text)?.error ?? message; }
                catch (ArgumentException) { /* not JSON -- keep the network error text */ }
            }
            onError(message);
            yield break;
        }
        onSuccess(text);
    }

    private void LoadAccountFromPrefs()
    {
        accountId = PlayerPrefs.GetString("StreetDice.AccountId", "");
        accountSessionToken = PlayerPrefs.GetString("StreetDice.AccountToken", "");
        accountUsername = PlayerPrefs.GetString("StreetDice.AccountUsername", "");
        if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(accountSessionToken)) return;
        accountLoggedIn = true;
        StartCoroutine(RefreshAccountProfile());
    }

    private void SaveAccountToPrefs()
    {
        PlayerPrefs.SetString("StreetDice.AccountId", accountId);
        PlayerPrefs.SetString("StreetDice.AccountToken", accountSessionToken);
        PlayerPrefs.SetString("StreetDice.AccountUsername", accountUsername);
    }

    private void LogOutAccount()
    {
        accountId = accountSessionToken = accountUsername = "";
        accountLevel = 1;
        accountWins = accountXp = accountXpUntilNextLevel = accountShotsPlayed = 0;
        accountLevelStep = 1;
        accountLevelProgress = 0f;
        accountTrophyNotes = Array.Empty<int>();
        accountMaxBet = 100;
        accountPrestigeUnlocked = false;
        accountLoggedIn = false;
        PlayerPrefs.DeleteKey("StreetDice.AccountId");
        PlayerPrefs.DeleteKey("StreetDice.AccountToken");
        PlayerPrefs.DeleteKey("StreetDice.AccountUsername");
    }

    private void SubmitCreateAccount()
    {
        if (statsBusy || string.IsNullOrWhiteSpace(statsUsernameInput) || string.IsNullOrWhiteSpace(statsPasswordInput)) return;
        StartCoroutine(RegisterThenLogIn(statsUsernameInput.Trim(), statsPasswordInput));
    }

    private void SubmitSignIn()
    {
        if (statsBusy || string.IsNullOrWhiteSpace(statsUsernameInput) || string.IsNullOrWhiteSpace(statsPasswordInput)) return;
        StartCoroutine(LogIn(statsUsernameInput.Trim(), statsPasswordInput));
    }

    private IEnumerator RegisterThenLogIn(string username, string password)
    {
        statsBusy = true;
        statsError = "";
        bool registered = false;
        yield return PostAccount("/api/accounts/register",
            JsonUtility.ToJson(new RegisterAccountRequestDto { username = username, password = password }),
            _ => registered = true, error => statsError = error);
        if (!registered) { statsBusy = false; yield break; }
        yield return LogIn(username, password);
    }

    private IEnumerator LogIn(string username, string password)
    {
        statsBusy = true;
        statsError = "";
        AccountResponseDto response = null;
        yield return PostAccount("/api/accounts/login",
            JsonUtility.ToJson(new LoginAccountRequestDto { username = username, password = password }),
            body => response = JsonUtility.FromJson<AccountResponseDto>(body), error => statsError = error);
        statsBusy = false;
        if (response == null) yield break;

        ApplyAccountResponse(response);
        accountLoggedIn = true;
        SaveAccountToPrefs();
        statsUsernameInput = statsPasswordInput = "";
    }

    [Serializable] private sealed class AccountSessionRequestDto { public string accountSessionToken = ""; }

    private const string ExpiredLoginMessage = "Your sign-in expired, so you're playing without your rank. Sign in again from Game Stats.";

    /// <summary>Checks the saved token, not just the account -- a login can expire, or be
    /// replaced by signing in on another phone. A 401 means the token is dead, so sign out
    /// visibly; any other failure (no signal, server down) keeps the saved login as-is.</summary>
    private IEnumerator RefreshAccountProfile()
    {
        if (string.IsNullOrEmpty(accountId)) yield break;
        using var request = new UnityWebRequest(baseUrl + "/api/accounts/" + accountId + "/session", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(
            JsonUtility.ToJson(new AccountSessionRequestDto { accountSessionToken = accountSessionToken })));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.responseCode == 401) { ExpireAccountLogin(); yield break; }
        if (request.result != UnityWebRequest.Result.Success) yield break;
        ApplyAccountResponse(JsonUtility.FromJson<AccountResponseDto>(request.downloadHandler.text));
    }

    private void ExpireAccountLogin()
    {
        LogOutAccount();
        statsError = ExpiredLoginMessage;
        result = ExpiredLoginMessage;
    }

    private void ApplyAccountResponse(AccountResponseDto response)
    {
        if (response == null) return;
        accountId = response.accountId;
        accountUsername = response.username;
        accountLevel = response.level;
        accountWins = response.wins;
        accountXp = response.xp;
        accountXpUntilNextLevel = response.xpUntilNextLevel;
        accountShotsPlayed = response.shotsPlayed;
        accountLevelStep = response.levelStep;
        accountLevelProgress = response.levelProgress;
        accountTrophyNotes = response.trophyNotes ?? Array.Empty<int>();
        accountMaxBet = response.maxBetAtLevel;
        accountPrestigeUnlocked = response.prestigeBillsUnlocked;
        // Login responses carry a fresh session token; a session refresh doesn't, so don't
        // stomp the one already saved with an empty string.
        if (!string.IsNullOrEmpty(response.accountSessionToken)) accountSessionToken = response.accountSessionToken;
    }
}
