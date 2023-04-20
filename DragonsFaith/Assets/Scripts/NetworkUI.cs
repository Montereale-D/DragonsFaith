using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private bool _isReady;
    private bool _isClientReady;

    private void Awake()
    {
        hostButton.onClick.AddListener(OnHostButtonClick);
        clientButton.onClick.AddListener(OnClientButtonClick);
        cancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    private void OnCancelButtonClick()
    {
        hostButton.image.color = offButtonColor;
        clientButton.image.color = offButtonColor;
        logText.text = "Log: shutted down";

        if (IsOwner)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            HostDisconnectedClientRpc();
        }
        
        hostButton.enabled = true;
        clientButton.enabled = true;
        clientReadyButton.gameObject.SetActive(false);
        hostReadyButton.gameObject.SetActive(false);

        clientReadyButton.onClick.RemoveAllListeners();
        hostReadyButton.onClick.RemoveAllListeners();

        NetworkManager.Singleton.Shutdown();
    }

    private void OnClientButtonClick()
    {
        var confirmed = NetworkManager.Singleton.StartClient();
        if (!confirmed) return;
        
        clientButton.image.color = onButtonColor;
        logText.text = "Log: you are a client";

        hostButton.enabled = false;
        clientButton.enabled = false;
        clientReadyButton.gameObject.SetActive(true);
        hostReadyButton.gameObject.SetActive(true);
        
        clientReadyButton.onClick.AddListener(OnClientReadyButtonClick);
    }

    private void OnHostButtonClick()
    {
        var confirmed = NetworkManager.Singleton.StartHost();
        if (!confirmed) return;
        
        logText.text = "Log: you are a host";

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        hostButton.image.color = onButtonColor;

        hostButton.enabled = false;
        clientButton.enabled = false;
        clientReadyButton.gameObject.SetActive(true);
        hostReadyButton.gameObject.SetActive(true);
        
        hostReadyButton.onClick.AddListener(OnHostReadyButtonClick);
        
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

            if (_isClientReady)
            {
                logText.text = "Log: NEXT SCENE";
                sceneManager.LoadSceneSingle();
            }
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

    [ClientRpc]
    private void HostDisconnectedClientRpc()
    {
        if (IsOwner) return;
        logText.text = "Log: ATTENTION: Host disconnected";
    }
    
    [ClientRpc]
    private void HostReadyClientRpc()
    {
        if (IsOwner) return;
        hostReadyButton.image.color = onButtonColor;
    }
    
    [ClientRpc]
    private void HostNotReadyClientRpc()
    {
        if (IsOwner) return;
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
                logText.text = "Log: NEXT SCENE";
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
