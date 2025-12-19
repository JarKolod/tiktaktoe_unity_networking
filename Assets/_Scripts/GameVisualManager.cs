using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameVisualManager : MonoBehaviour
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
        Transform spawnedCrossTrans = Instantiate(crossPrefab, GetGridWorldPos(e.x, e.y), Quaternion.identity);

        NetworkObject netObj = spawnedCrossTrans.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        spawnedCrossTrans.position = GetGridWorldPos(e.x, e.y);
    }

    private Vector2 GetGridWorldPos(short x, short y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
