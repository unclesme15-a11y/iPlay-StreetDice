using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public sealed class StreetDiceVivoxVoiceClient : MonoBehaviour
{
    private string baseUrl, gameId, playerId, seatToken, channelName;
    private bool initialized, signedIn, joined, muted;
    private BackendTokens tokenProvider;
    private readonly Dictionary<VivoxParticipant, Action> speechHandlers = new();

    public string Status { get; private set; } = "Voice idle";
    public bool IsJoined => joined;
    public event Action<VivoxParticipant> ParticipantAdded;
    public event Action<string> SpeechActivity;

    public void Join(string apiBase, string tableId, string seatId, string sessionToken)
    {
        baseUrl = apiBase.TrimEnd('/');
        gameId = tableId;
        playerId = seatId;
        seatToken = sessionToken;
        channelName = "street-dice-" + tableId;
        StartCoroutine(JoinWhenPermitted());
    }

    private IEnumerator JoinWhenPermitted()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            float deadline = Time.realtimeSinceStartup + 15f;
            while (!Permission.HasUserAuthorizedPermission(Permission.Microphone) && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Status = "Microphone permission is required for table voice";
                yield break;
            }
        }
#endif
        _ = JoinAsync();
        yield break;
    }

    private async Task JoinAsync()
    {
        try
        {
            Status = "Connecting voice";
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (tokenProvider == null) tokenProvider = new BackendTokens(this);
            VivoxService.Instance.SetTokenProvider(tokenProvider);
            if (!initialized)
            {
                await VivoxService.Instance.InitializeAsync();
                initialized = true;
                VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
            }
            if (!signedIn)
            {
                await VivoxService.Instance.LoginAsync(new LoginOptions { DisplayName = playerId });
                signedIn = true;
            }
            await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);
            joined = true;
            await ApplyMuteAsync();
            Status = "Voice connected";
        }
        catch (Exception)
        {
            joined = false;
            Status = "Voice unavailable. Check table voice setup or connection.";
        }
    }

    public void SetMuted(bool value)
    {
        muted = value;
        if (joined) _ = ApplyMuteAsync();
    }

    private async Task ApplyMuteAsync()
    {
        try
        {
            await VivoxService.Instance.SetChannelTransmissionModeAsync(
                muted ? TransmissionMode.None : TransmissionMode.Single, channelName);
        }
        catch (Exception) { Status = "Voice control unavailable."; }
    }

    public async void Leave()
    {
        if (!joined) return;
        joined = false;
        try { await VivoxService.Instance.LeaveChannelAsync(channelName); }
        catch (Exception) { Status = "Voice disconnect failed."; }
    }

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        ParticipantAdded?.Invoke(participant);
        if (speechHandlers.ContainsKey(participant)) return;
        Action handler = () =>
        {
            if (participant.SpeechDetected) SpeechActivity?.Invoke(participant.DisplayName);
        };
        participant.ParticipantSpeechDetected += handler;
        participant.ParticipantAudioEnergyChanged += handler;
        speechHandlers[participant] = handler;
    }

    private void OnDestroy()
    {
        if (initialized) VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
        foreach (var entry in speechHandlers)
        {
            entry.Key.ParticipantSpeechDetected -= entry.Value;
            entry.Key.ParticipantAudioEnergyChanged -= entry.Value;
        }
        Leave();
    }

    private Task<string> GetTokenAsync(string action, string fromUserUri, string channelUri)
    {
        var completion = new TaskCompletionSource<string>();
        StartCoroutine(RequestToken(action, fromUserUri, channelUri, completion));
        return completion.Task;
    }

    private IEnumerator RequestToken(string action, string fromUserUri, string channelUri, TaskCompletionSource<string> completion)
    {
        var payload = new VoiceTokenRequest
        {
            playerId = playerId, playerSessionToken = seatToken,
            action = action, fromUserUri = fromUserUri, channelUri = channelUri
        };
        using var request = new UnityWebRequest(baseUrl + "/api/street-dice/" + gameId + "/voice/access-token", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload)));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            completion.TrySetException(new InvalidOperationException("Table voice token is unavailable"));
            yield break;
        }
        var response = JsonUtility.FromJson<VoiceTokenResponse>(request.downloadHandler.text);
        if (response == null || string.IsNullOrWhiteSpace(response.accessToken))
            completion.TrySetException(new InvalidOperationException("Table voice token is empty"));
        else completion.TrySetResult(response.accessToken);
    }

    [Serializable] private sealed class VoiceTokenRequest
    {
        public string playerId, playerSessionToken, action, fromUserUri, channelUri;
    }
    [Serializable] private sealed class VoiceTokenResponse { public string accessToken; }

    private sealed class BackendTokens : IVivoxTokenProvider
    {
        private readonly StreetDiceVivoxVoiceClient owner;
        public BackendTokens(StreetDiceVivoxVoiceClient owner) { this.owner = owner; }
        public Task<string> GetTokenAsync(string issuer = null, TimeSpan? expiration = null,
            string targetUserUri = null, string action = null, string channelUri = null,
            string fromUserUri = null, string realm = null)
            => owner.GetTokenAsync(action ?? "join", fromUserUri ?? targetUserUri, channelUri);
    }
}
