using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// This class is used to handle connections in the lobby screen
/// </summary>
public class NetworkUI : NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button hostReadyButton;
    [SerializeField] private Button clientReadyButton;

    [SerializeField] private Color onButtonColor;
    [SerializeField] private Color offButtonColor;

    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private SceneManager sceneManager;
    
    [SerializeField] private string sceneName;

    private bool _isReady;
    private bool _isClientReady;

    private void Awake()
    {
        hostButton.onClick.AddListener(OnHostButtonClick);
        clientButton.onClick.AddListener(OnClientButtonClick);
        cancelButton.onClick.AddListener(OnCancelButtonClick);
    }


    private void OnClientButtonClick()
    {
        var confirmed = NetworkManager.Singleton.StartClient();
        if (!confirmed) return;

        //Update UI
        clientButton.image.color = onButtonColor;
        logText.text = "Log: you are a client";
        hostButton.enabled = false;
        clientButton.enabled = false;

        //Make ready buttons appear
        clientReadyButton.gameObject.SetActive(true);
        hostReadyButton.gameObject.SetActive(true);

        //Only client can click on ClientReadyButton
        clientReadyButton.onClick.AddListener(OnClientReadyButtonClick);
    }

    private void OnHostButtonClick()
    {
        var confirmed = NetworkManager.Singleton.StartHost();
        if (!confirmed) return;

        //Register to client connection events
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        //Update UI
        hostButton.image.color = onButtonColor;
        logText.text = "Log: you are a host";
        hostButton.enabled = false;
        clientButton.enabled = false;

        //Make ready buttons appear
        clientReadyButton.gameObject.SetActive(true);
        hostReadyButton.gameObject.SetActive(true);

        //Only host can click on HostReadyButton
        hostReadyButton.onClick.AddListener(OnHostReadyButtonClick);
    }

    private void OnCancelButtonClick()
    {
        if (IsHost)
        {
            //Unregister to client connection events
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            //Warn client
            HostDisconnectedClientRpc();
        }


        clientReadyButton.onClick.RemoveAllListeners();
        hostReadyButton.onClick.RemoveAllListeners();

        //Update UI
        hostButton.image.color = offButtonColor;
        clientButton.image.color = offButtonColor;
        logText.text = "Log: shut down";
        hostButton.enabled = true;
        clientButton.enabled = true;

        //Make ready buttons disappear
        clientReadyButton.gameObject.SetActive(false);
        hostReadyButton.gameObject.SetActive(false);

        NetworkManager.Singleton.Shutdown();
    }

    private void OnHostReadyButtonClick()
    {
        if (_isReady)
        {
            _isReady = false;
            hostReadyButton.image.color = offButtonColor;
            HostNotReadyClientRpc();
        }
        else
        {
            _isReady = true;
            hostReadyButton.image.color = onButtonColor;
            HostReadyClientRpc();

            if (!_isClientReady) return;

            OnBothPlayersReady();
        }
    }

    private void OnClientReadyButtonClick()
    {
        if (_isReady)
        {
            _isReady = false;
            clientReadyButton.image.color = offButtonColor;
            ClientNotReadyServerRpc();
        }
        else
        {
            _isReady = true;
            clientReadyButton.image.color = onButtonColor;
            ClientReadyServerRpc();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        logText.text = "Log: Client connected";
    }

    private void OnClientDisconnected(ulong clientId)
    {
        logText.text = "Log: Client disconnected";
    }

    private void OnBothPlayersReady()
    {
        GetComponent<NetworkObject>().Despawn();
        logText.text = "Log: NEXT SCENE";
        sceneManager.LoadSceneSingle(sceneName);
    }

    [ClientRpc]
    private void HostDisconnectedClientRpc()
    {
        if (IsHost) return;
        logText.text = "Log: ATTENTION: Host disconnected";
    }

    [ClientRpc]
    private void HostReadyClientRpc()
    {
        if (IsHost) return;
        hostReadyButton.image.color = onButtonColor;
    }

    [ClientRpc]
    private void HostNotReadyClientRpc()
    {
        if (IsHost) return;
        hostReadyButton.image.color = offButtonColor;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientReadyServerRpc()
    {
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            clientReadyButton.image.color = onButtonColor;
            _isClientReady = true;

            if (_isReady)
                OnBothPlayersReady();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientNotReadyServerRpc()
    {
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            clientReadyButton.image.color = offButtonColor;
            _isClientReady = false;
        }
    }
}