using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Network;
using TMPro;
using UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
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
    [SerializeField] private string sceneName;

    private bool _isReady;
    private bool _isClientReady;
    private static string joinCode;

    const int m_MaxConnections = 2;
    [SerializeField] private TextMeshProUGUI relayJoinCodeHost;
    [SerializeField] private TMP_InputField nameTextHost;
    [SerializeField] private TMP_InputField relayJoinCodeClient;
    [SerializeField] private TMP_InputField nameTextClient;

    private NetworkManager _networkManager;

    private void Start()
    {
        //_networkManager = FindObjectOfType<NetworkManager>();
        //_networkManager = NetworkManager.Singleton;

        hostButton.onClick.AddListener(OnHostButtonClick);
        clientButton.onClick.AddListener(OnClientButtonClick);
        cancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GetComponent<NetworkObject>().DestroyWithScene = true;

        if (IsHost)
        {
            HostReadyClientRpc();
        }
        else
        {
            ClientReadyServerRpc();
        }
    }

    async void Example_AuthenticatingAPlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            var playerID = AuthenticationService.Instance.PlayerId;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private void OnClientButtonClick()
    {
        /*var confirmed = NetworkManager.Singleton.StartClient();
        if (!confirmed) return;*/

        Example_AuthenticatingAPlayer();

        //Update UI
        //clientButton.image.color = onButtonColor;
        logText.text = "Log: you are a client";
        hostButton.enabled = false;
        clientButton.enabled = false;
        relayJoinCodeClient.text = "";

        //Make ready buttons appear
        clientReadyButton.gameObject.SetActive(true);
        hostReadyButton.gameObject.SetActive(true);

        //Only client can click on ClientReadyButton
        clientReadyButton.onClick.AddListener(OnClientReadyButtonClick);
    }

    private IEnumerator Example_ConfigureTransportAndStartNgoAsConnectingPlayer()
    {
        // Populate RelayJoinCode beforehand through the UI
        if (string.IsNullOrEmpty(relayJoinCodeClient.text) || relayJoinCodeClient.text.Length < 6)
        {
            _isReady = false;
            clientReadyButton.image.color = offButtonColor;
            yield break;
        }

        joinCode = relayJoinCodeClient.text.Substring(0, 6);
        Debug.Log("Join code is " + joinCode);

        Task<RelayServerData> clientRelayUtilityTask;
        try
        {
            clientRelayUtilityTask = JoinRelayServerFromJoinCode(joinCode);
        }
        catch (JoinCodeException e)
        {
            Debug.LogWarning(e);
            _isReady = false;
            clientReadyButton.image.color = offButtonColor;
            yield break;
        }

        while (!clientRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogWarning("Exception thrown when attempting to connect to Relay Server. Exception: " +
                           clientRelayUtilityTask.Exception?.Message);
            clientReadyButton.image.color = offButtonColor;
            yield break;
        }

        var relayServerData = clientRelayUtilityTask.Result;
        cancelButton.onClick.RemoveListener(OnCancelButtonClick);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        StartCoroutine(WaitForLoadOrCancel());

        yield return null;
    }

    private bool waitingHost = false;
    private IEnumerator WaitForLoadOrCancel()
    {
        waitingHost = true;
        yield return new WaitForSecondsRealtime(8);

        if (waitingHost)
        {
            OnCancelButtonClick();
        }
    }

    private static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            throw new JoinCodeException("Wrong join code or something went wrong");
        }

        Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"client: {allocation.AllocationId}");

        return new RelayServerData(allocation, "dtls");
    }

    private void OnHostButtonClick()
    {
        /*var confirmed = NetworkManager.Singleton.StartHost();
        if (!confirmed) return;*/

        Example_AuthenticatingAPlayer();

        //Register to client connection events
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        //Update UI
        //hostButton.image.color = onButtonColor;
        logText.text = "Log: you are a host";
        hostButton.enabled = false;
        clientButton.enabled = false;
        relayJoinCodeHost.text = "";

        //Make ready buttons appear
        clientReadyButton.gameObject.SetActive(true);
        hostReadyButton.gameObject.SetActive(true);

        //Only host can click on HostReadyButton
        hostReadyButton.onClick.AddListener(OnHostReadyButtonClick);
    }

    private IEnumerator Example_ConfigureTransportAndStartNgoAsHost()
    {
        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(m_MaxConnections);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " +
                           serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = serverRelayUtilityTask.Result;

        // Display the joinCode to the user.
        relayJoinCodeHost.text = joinCode;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();
        yield return null;
    }

    private static async Task<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxConnections,
        string region = null)
    {
        Allocation allocation;
        joinCode = null;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.AllocationId}");

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    }

    private void OnCancelButtonClick()
    {
        if (IsHost)
        {
            _isReady = false;
            hostReadyButton.image.color = offButtonColor;
            nameTextHost.text = "";
            relayJoinCodeHost.text = "";

            //Unregister to client connection events
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            //Warn client
            HostDisconnectedClientRpc();
        }
        else if (!IsHost)
        {
            if (_isReady)
            {
                //ClientNotReadyServerRpc();
            }
            
            _isReady = false;
            clientReadyButton.image.color = offButtonColor;
            nameTextClient.text = "";
            relayJoinCodeClient.text = "";
        }

        clientReadyButton.onClick.RemoveAllListeners();
        hostReadyButton.onClick.RemoveAllListeners();

        //Update UI
        /*hostReadyButton.image.color = offButtonColor;*/
        clientReadyButton.image.color = offButtonColor;
        logText.text = "Log: shut down";
        hostButton.enabled = true;
        clientButton.enabled = true;

        //Make ready buttons disappear
        clientReadyButton.gameObject.SetActive(false);
        hostReadyButton.gameObject.SetActive(false);
        
        NetworkManager.Singleton.Shutdown();
        
        MenuManager.Instance.BackToPlayAsScreen();
        
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
            PlayerPrefs.SetString("playerName", string.IsNullOrEmpty(nameTextHost.text) ? "Host" : nameTextHost.text);

            /*HostReadyClientRpc();
            //PlayerPrefs.SetString("playerName", "host");
            if (hostName.text == "") PlayerPrefs.SetString("playerName", "host");

            if (!_isClientReady) return;

            OnBothPlayersReady();*/
            StartCoroutine(Example_ConfigureTransportAndStartNgoAsHost());
        }
    }

    private void OnClientReadyButtonClick()
    {
        /*if (_isReady)
        {
            _isReady = false;
            clientReadyButton.image.color = offButtonColor;
            ClientNotReadyServerRpc();
        }
        else
        {*/
        _isReady = true;
        clientReadyButton.image.color = onButtonColor;
        //NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject.name = "Client";
        PlayerPrefs.SetString("playerName",
            string.IsNullOrEmpty(nameTextClient.text) ? "Client" : nameTextClient.text);

        StartCoroutine(Example_ConfigureTransportAndStartNgoAsConnectingPlayer());

        //}
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
        if (IsHost)
        {
            OnBothPlayersReadyClientRpc();
            //GetComponent<NetworkObject>().Despawn();
        }

        logText.text = "Log: NEXT SCENE";

        SceneManager.instance.LoadSceneSingle(sceneName);

        if (IsHost)
        {
            ShowUI();
            ShowUIClientRpc();
        }
    }

    [ClientRpc]
    private void OnBothPlayersReadyClientRpc()
    {
        if (IsHost) return;

        OnBothPlayersReady();
    }

    private void ShowUI()
    {
        var playerUI = PlayerUI.instance;
        if (playerUI)
        {
            playerUI.ShowUI(true);
        }
    }

    [ClientRpc]
    private void ShowUIClientRpc()
    {
        ShowUI();
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
        Debug.Log("HostReadyClientRpc");
        waitingHost = false;
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
        Debug.Log("ClientReadyServerRpc, Am I ready? " + _isReady);
        //if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        //{
        //    clientReadyButton.image.color = onButtonColor;
        _isClientReady = true;

        if (_isReady)
            OnBothPlayersReady();
        //}
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