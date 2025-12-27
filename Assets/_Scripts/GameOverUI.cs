using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winTextColor;
    [SerializeField] private Color loseTextColor;
    [SerializeField] private Color tiedTextColor;
    [SerializeField] private Button restartBtn;

    private void Awake()
    {
        restartBtn.onClick.AddListener(() => {
            GameManager.Instance.RematchRpc();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRemtach += GameManager_OnRemtach;
        GameManager.Instance.OnGameTie += GameManager_OnGameTie;

        Hide();
    }

    private void GameManager_OnGameTie(object sender, EventArgs e)
    {
        resultTextMesh.text = "TIE!";
        resultTextMesh.color = tiedTextColor;

        Show();
    }

    private void GameManager_OnRemtach(object sender, System.EventArgs e)
    {
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
