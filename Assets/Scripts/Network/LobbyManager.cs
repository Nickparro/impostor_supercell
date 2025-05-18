using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject cameraMenu, cameraLobby;

    [SerializeField] private RelayManager relayManager;

    private void Start()
    {
        hostButton.onClick.AddListener(CreateLobby);
        clientButton.onClick.AddListener(JoinLobby);
        startGameButton.onClick.AddListener(StartGame);

        menuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        startGameButton.gameObject.SetActive(false);
        cameraMenu.SetActive(true);
        cameraLobby.SetActive(false);
    }

    private async void CreateLobby()
    {
        hostButton.interactable = false;

        string joinCode = await relayManager.CreateRelay();

        if (!string.IsNullOrEmpty(joinCode))
        {
            NetworkManager.Singleton.StartHost();
            menuPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            cameraMenu.SetActive(false);
            cameraLobby.SetActive(true);

            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("No se pudo crear la sala Relay");
            hostButton.interactable = true;
        }
    }

    private async void JoinLobby()
    {
        string joinCode = joinCodeInput.text.Trim();

        if (string.IsNullOrEmpty(joinCode))
        {
            return;
        }

        clientButton.interactable = false;

        bool success = await relayManager.JoinRelay(joinCode);

        if (success)
        {
            NetworkManager.Singleton.StartClient();
            menuPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            cameraMenu.SetActive(false);
            cameraLobby.SetActive(true);
        }
        else
        {
            clientButton.interactable = true;
        }
    }

    private void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}