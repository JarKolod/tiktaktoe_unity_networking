using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineWinnerPrefab;

    private List<GameObject> visualGameObjectList;

    private void Awake()
    {
        visualGameObjectList = new();
    }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRemtach += GameManager_OnRemtach;
    }

    private void GameManager_OnRemtach(object sender, EventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        foreach(GameObject visualObj in visualGameObjectList)
        {
            Destroy(visualObj); 
        }
        visualGameObjectList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        float eulerZ = 0f;
        switch(e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal: eulerZ = 0f; break;
            case GameManager.Orientation.Vertical: eulerZ = 90f; break;
            case GameManager.Orientation.DiagonalA: eulerZ = 45f; break;
            case GameManager.Orientation.DiagonalB: eulerZ = -45f; break;
        }
        Transform lineWinnerTransform =
            Instantiate(lineWinnerPrefab, GetGridWorldPos((short)e.line.centerGridPos.x, (short)e.line.centerGridPos.y),
            Quaternion.Euler(0, 0, eulerZ)
            );
        lineWinnerTransform.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(lineWinnerTransform.gameObject);
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        Debug.Log("GameManager_OnClickedOnGridPosition");
        SpawnObjRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjRpc(short x, short y, GameManager.PlayerType playerType)
    {
        Debug.Log("Spawn Obj");

        Transform prefab = playerType switch
        {
            GameManager.PlayerType.Circle => circlePrefab,
            GameManager.PlayerType.Cross => crossPrefab,
            _ => crossPrefab
        };

        Transform spawnedCrossTrans = Instantiate(prefab, GetGridWorldPos(x, y), Quaternion.identity);
        spawnedCrossTrans.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(spawnedCrossTrans.gameObject);
    }

    private Vector2 GetGridWorldPos(short x, short y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
