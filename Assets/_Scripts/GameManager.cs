using Mono.Cecil;
using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }

    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public short x;
        public short y;
        public PlayerType playerType;
    }
    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChange;

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayer = new NetworkVariable<PlayerType>(
        PlayerType.None, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server); // default parameters

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log("OnNetworkSpawn localID: " + NetworkManager.Singleton.LocalClientId);
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayer.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) => 
        {
            OnCurrentPlayablePlayerTypeChange?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if(NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayer.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPosRpc(short x, short y, PlayerType playerType)
    {
        Debug.Log("Clicked grid pos: x: " + x + ", y: " + y);
        if(playerType != currentPlayer.Value)
        {
            return;
        }

        OnClickedOnGridPosition?.Invoke(
            this,
            new OnClickedOnGridPositionEventArgs
            {
                x = x,
                y = y,
                playerType = currentPlayer.Value
            });

        currentPlayer.Value = currentPlayer.Value switch
        {
            PlayerType.Circle => PlayerType.Cross,
            PlayerType.Cross => PlayerType.Circle,
            _ => PlayerType.None
        };
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayerType()
    {
        return currentPlayer.Value;
    }
}
