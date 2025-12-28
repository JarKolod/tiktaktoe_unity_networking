using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private GameObject placeSfxPrefab;
    [SerializeField] private GameObject winSfxPrefab;
    [SerializeField] private GameObject loseSfxPrefab;

    private void Start()
    {
        GameManager.Instance.OnObjPlaced += GameManager_OnObjPlaced;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(GameManager.Instance.GetLocalPlayerType() == e.winPlayerType)
        {
            GameObject sfxGO = Instantiate(winSfxPrefab);
            Destroy(sfxGO, 5f);
        }
        else
        {
            GameObject sfxGO = Instantiate(loseSfxPrefab);
            Destroy(sfxGO, 5f);
        }
    }

    private void GameManager_OnObjPlaced(object sender, System.EventArgs e)
    {
        GameObject sfxGO = Instantiate(placeSfxPrefab);
        Destroy(sfxGO, 5f);
    }
}
