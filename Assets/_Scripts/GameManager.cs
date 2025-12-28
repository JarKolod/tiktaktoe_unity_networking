using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }

    public struct Line
    {
        public List<Vector2Int> gridList;
        public Vector2Int centerGridPos;
        public Orientation orientation;
    }

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public short x;
        public short y;
        public PlayerType playerType;
    }
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }


    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public event EventHandler OnGameStarted;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public event EventHandler OnCurrentPlayablePlayerTypeChange;
    public event EventHandler OnRemtach;
    public event EventHandler OnGameTie;
    public event EventHandler OnScoreChange;
    public event EventHandler OnObjPlaced;


    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayer = new NetworkVariable<PlayerType>(
        PlayerType.None, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server); // default parameters
    private PlayerType[,] playerTypeArray;
    private List<Line> Lines;

    private NetworkVariable<short> crossPlayerScore = new(0);
    private NetworkVariable<short> circlePlayerScore = new(0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;

        playerTypeArray = new PlayerType[3,3];
        Lines = new List<Line>
        {
            // Horizontal
            new Line
            {
                gridList = new List<Vector2Int>{new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
                centerGridPos = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridList = new List<Vector2Int>{new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                centerGridPos = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridList = new List<Vector2Int>{new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) },
                centerGridPos = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal
            },
            //Vertical
            new Line
            {
                gridList = new List<Vector2Int>{new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
                centerGridPos = new Vector2Int(0, 1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridList = new List<Vector2Int>{new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
                centerGridPos = new Vector2Int(1, 1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridList = new List<Vector2Int>{new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2) },
                centerGridPos = new Vector2Int(2, 1),
                orientation = Orientation.Vertical
            },
            //Diagonals
            new Line
            {
                gridList = new List<Vector2Int>{new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) },
                centerGridPos = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA
            },
            new Line
            {
                gridList = new List<Vector2Int>{new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) },
                centerGridPos = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB
            },
        };
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

        crossPlayerScore.OnValueChanged += (short prevScore, short newScore) =>
        {
            OnScoreChange?.Invoke(this, EventArgs.Empty);
        };

        circlePlayerScore.OnValueChanged += (short prevScore, short newScore) =>
        {
            OnScoreChange?.Invoke(this, EventArgs.Empty);
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

        if(playerTypeArray[x,y] != PlayerType.None)
        {
            // Occupied
            return;
        }
        playerTypeArray[x, y] = playerType;
        TriggerOnObjPlacedRpc();

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

        TestWinner();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnObjPlacedRpc()
    {
        OnObjPlaced?.Invoke(this, EventArgs.Empty);
    }

    private bool TestWinnerLineABC(PlayerType a, PlayerType b, PlayerType c)
    {
        return a != PlayerType.None && a == b && b == c;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLineABC(
                playerTypeArray[line.gridList[0].x, line.gridList[0].y],
                playerTypeArray[line.gridList[1].x, line.gridList[1].y],
                playerTypeArray[line.gridList[2].x, line.gridList[2].y]
            );
    }

    private void TestWinner()
    {
        for(short i = 0; i < Lines.Count; i++)
        {
            Line line = Lines[i];
            if(TestWinnerLine(line))
            {
                currentPlayer.Value = PlayerType.None;
                switch(playerTypeArray[line.centerGridPos.x, line.centerGridPos.y])
                {
                    case PlayerType.Cross:
                    {
                        crossPlayerScore.Value += 1;
                        break;
                    }
                    case PlayerType.Circle:
                    {
                        circlePlayerScore.Value += 1;
                        break;
                    }
                }

                TriggerOnGameWinRpc(i, playerTypeArray[line.centerGridPos.x, line.centerGridPos.y]);

                return;
            }
        }

        // Check tie
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                    return;
            } 
        }

        TriggerOnGameTiedRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTiedRpc()
    {
        OnGameTie?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType playerType)
    {
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = Lines[lineIndex],
            winPlayerType = playerType
        });
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for(int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for(int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }
        currentPlayer.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRemtach?.Invoke(this, EventArgs.Empty);
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayerType()
    {
        return currentPlayer.Value;
    }

    public void GetScores(out int crossScore, out int circleScore)
    {
        print("cross: " + crossPlayerScore.Value + ", circle: " + circlePlayerScore.Value);
        crossScore = crossPlayerScore.Value;
        circleScore = circlePlayerScore.Value;
    }
}
