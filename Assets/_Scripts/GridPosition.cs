using UnityEngine;
using UnityEngine.EventSystems;

public class GridPosition : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private short x;
    [SerializeField] private short y;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Grid position clicked: x: " + x + ", y: " + y);
        GameManager.Instance.ClickedOnGridPosRpc(x, y, GameManager.Instance.GetLocalPlayerType());
    }
}
