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

    private PlayerType localPlayerType;
    private PlayerType currentPlayer;

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
            currentPlayer = PlayerType.Cross;
        }
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPosRpc(short x, short y, PlayerType playerType)
    {
        Debug.Log("Clicked grid pos: x: " + x + ", y: " + y);
        if(playerType != currentPlayer)
        {
            return;
        }

        OnClickedOnGridPosition?.Invoke(
            this,
            new OnClickedOnGridPositionEventArgs
            {
                x = x,
                y = y,
                playerType = currentPlayer
            });

        currentPlayer = currentPlayer switch
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
}
