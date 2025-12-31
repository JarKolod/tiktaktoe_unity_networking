using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button startHostBtn;
    [SerializeField] private Button startClientBtn;
    [SerializeField] private TMP_InputField joinCodeInputField;

    private void Awake()
    {
        startHostBtn.onClick.AddListener(() =>
        {
            // Use session manager to create a Relay-backed session before starting the host
            if (NetworkSessionManager.Instance != null)
            {
                NetworkSessionManager.Instance.StartHost();
            }
            else
            {
                Debug.LogWarning("NetworkSessionManager.Instance is null. Falling back to direct StartHost().");
                // NetworkManager.Singleton.StartHost();
            }

            Hide();
        });

        startClientBtn.onClick.AddListener(() =>
        {
            if (NetworkSessionManager.Instance != null)
            {
                NetworkSessionManager.Instance.JoinPlayer(joinCodeInputField.text);
            }
            else
            {
                Debug.LogWarning("NetworkSessionManager.Instance is null. Falling back to direct StartClient().");
                // NetworkManager.Singleton.StartClient();
            }

            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
