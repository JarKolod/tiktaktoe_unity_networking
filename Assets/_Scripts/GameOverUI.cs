using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winTextColor;
    [SerializeField] private Color loseTextColor;


    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;

        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            resultTextMesh.text = "YOU WIN!";
            resultTextMesh.color = winTextColor;
        }
        else
        {
            resultTextMesh.text = "YOU LOSE!";
            resultTextMesh.color = loseTextColor;
        }
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
