using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityUtils;
using static GameManager;


public class NetworkSessionManager : Singleton<NetworkSessionManager>
{
    ISession activeSession;

    public event EventHandler<OnCodeGeneratedEventArgs> OnCodeGeneratedEvent;
    public class OnCodeGeneratedEventArgs : EventArgs
    {
        public string code;
    }

    const string playerNamePropertKey = "playerName";

    public ISession ActiveSession
    {
        get => activeSession;
        set
        {
            activeSession = value;
            Debug.Log($"Active session : {activeSession}");
        }
    }

    private bool isBusy = false;


    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync(); // Init Unity Gaming Services (UGS) SDKs
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Annonymusly authenticate palyer
            }
            Debug.Log($"Sign in anonymously succeded. PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void OnDestroy()
    {
        _ = LeaveSession();
    }

    private async UniTask<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    {
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerNamePropert = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);

        return new Dictionary<string, PlayerProperty> { { playerNamePropertKey, playerNamePropert } };
    }

    private async void StartSessionAsHost()
    {
        if (!IsReadyToStartHost())
        {
            return;
        }

        isBusy = true;
        try
        {
            var playerProperties = await GetPlayerProperties();

            var options = new SessionOptions
            {
                MaxPlayers = 2,
                IsLocked = false,
                IsPrivate = false,
                PlayerProperties = playerProperties
            }.WithRelayNetwork(); // or withDisctributed... instead of relay

            ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            Debug.Log($"Session {ActiveSession.Id} created. Join code: {ActiveSession.Code}");

            OnCodeGeneratedEvent?.Invoke(this, new OnCodeGeneratedEventArgs
            {
                code = ActiveSession.Code
            });

            // MultiplayerService / NetworkModule will start the network/transport when using WithRelayNetwork().
            Debug.Log("Network startup is handled by MultiplayerService; no manual StartHost() required.");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            isBusy = false;
        }
    }

    private async UniTaskVoid JoinSessionById(string sessionId)
    {
        if (isBusy)
        {
            Debug.LogWarning("JoinSessionById called while another session operation is in progress.");
            return;
        }

        if (ActiveSession != null)
        {
            Debug.LogWarning("A session is already active.");
            return;
        }

        isBusy = true;
        try
        {
            ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
            Debug.Log($"Session {ActiveSession.Id} joined.");

            // Network startup is handled by MultiplayerService when joining a session with relay.
            Debug.Log("Network startup is handled by MultiplayerService; no manual StartClient() required.");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            isBusy = false;
        }
    }

    private async UniTaskVoid JoinSessionByCode(string sessionCode)
    {
        if (!IsReadyToJoinSessionByCode())
        {
            return;
        }

        isBusy = true;
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log($"Attempting to join session code: \"{sessionCode}\"");
            ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);

            Debug.Log($"Joined Session {ActiveSession.Id}");

            // Network startup is handled by MultiplayerService when joining a session with relay.
            Debug.Log("Network startup is handled by MultiplayerService; no manual StartClient() required.");
        }
        catch (SessionException se)
        {
            Debug.LogWarning($"Join failed (session error): {se.Message}");
            // show UI to user: invalid/expired code
        }
        catch (Exception ex)
        {
            Debug.LogError($"Join failed: {ex}");
        }
        finally
        {
            isBusy = false;
        }
    }


    private async UniTaskVoid KickPlayer(string playerId)
    {
        if (!ActiveSession.IsHost)
            return;

        await ActiveSession.AsHost().RemovePlayerAsync(playerId);
        Debug.Log($"player: {ActiveSession.GetPlayer(playerId).GetPlayerName() ?? "no name"} has been kicked!");
    }

    async UniTask<IList<ISessionInfo>> QuerySessions()
    {
        var sessionQueryOptions = new QuerySessionsOptions();
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
        return results.Sessions;
    }

    async UniTaskVoid LeaveSession()
    {
        if (ActiveSession != null)
        {
            try
            {
                await ActiveSession.LeaveAsync();
            }
            catch
            {
                // Ignore as we are leaving the game
            }
            finally
            {
                ActiveSession = null;
            }
        }
    }

    private bool IsReadyToStartHost()
    {
        if (isBusy)
        {
            Debug.LogWarning("StartSessionAsHost called while another session operation is in progress.");
            return false;
        }

        if (ActiveSession != null)
        {
            Debug.LogWarning("A session is already active.");
            return false;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager already running. Not starting host.");
            return false;
        }

        return true;
    }

    private bool IsReadyToJoinSessionByCode()
    {
        if (isBusy)
        {
            Debug.LogWarning("JoinSessionByCode called while another session operation is in progress.");
            return false;
        }

        if (ActiveSession != null)
        {
            Debug.LogWarning("A session is already active.");
            return false;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager already running; skipping Join.");
            return false;
        }

        return true;
    }


    public void StartHost() => StartSessionAsHost();
    public UniTaskVoid JoinPlayer(string code) => JoinSessionByCode(code);
}
