using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public short x;
        public short y;
    }

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

    public void ClickedOnGridPos(short x, short y)
    {
        OnClickedOnGridPosition?.Invoke(
            this,
            new OnClickedOnGridPositionEventArgs
            {
                x = x,
                y = y
            });
    }
}
