using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
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
        NetworkObject netObj = spawnedCrossTrans.GetComponent<NetworkObject>();
        netObj.Spawn(true);
    }

    private Vector2 GetGridWorldPos(short x, short y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
