using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObj;
    [SerializeField] private GameObject cricleArrowGameObj;
    [SerializeField] private GameObject crossYouTextGameObj;
    [SerializeField] private GameObject circleYouTextGameObj;
    [SerializeField] private TextMeshProUGUI circleScoreTextMesh;
    [SerializeField] private TextMeshProUGUI crossScoreTextMesh;

    private void Awake()
    {
        crossArrowGameObj.SetActive(false);
        cricleArrowGameObj.SetActive(false);
        crossYouTextGameObj.SetActive(false);
        circleYouTextGameObj.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChange += GameManager_OnCurrentPlayablePlayerTypeChange;
        GameManager.Instance.OnScoreChange += GameManager_OnScoreChange;
    }

    private void GameManager_OnScoreChange(object sender, EventArgs e)
    {
        GameManager.Instance.GetScores(out int crossScore, out int circleScore);

        crossScoreTextMesh.SetText(crossScore.ToString());
        circleScoreTextMesh.SetText(circleScore.ToString());
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChange(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        if(GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossYouTextGameObj.SetActive(true);
        }
        else
        {
            circleYouTextGameObj.SetActive(true);
        }

        circleScoreTextMesh.SetText("0");
        crossScoreTextMesh.SetText("0");

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if(GameManager.Instance.GetCurrentPlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowGameObj.SetActive(true);
            cricleArrowGameObj.SetActive(false);
        }
        else
        {
            crossArrowGameObj.SetActive(false);
            cricleArrowGameObj.SetActive(true);
        }
    }
}
