using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObj;
    [SerializeField] private GameObject cricleArrowGameObj;
    [SerializeField] private GameObject crossYouTextGameObj;
    [SerializeField] private GameObject circleYouTextGameObj;

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
