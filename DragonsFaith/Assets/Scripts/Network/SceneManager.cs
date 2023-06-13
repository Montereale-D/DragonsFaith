using System;
using Grid;
using Inventory;
using Player;
using Save;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class SceneManager : NetworkBehaviour
    {
        public static SceneManager instance { get; private set; }
        public Scene _loadedScene { get; private set; }

        private ClientNetworkTransform[] _players;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            }
        }

        private void OnClientDisconnect(ulong clientId)
        {
            NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }

        public bool sceneIsLoaded => _loadedScene.IsValid() && _loadedScene.isLoaded;

        private void ResetDungeonProgress()
        {
            if (DungeonProgressManager.instance != null)
            {
                DungeonProgressManager.instance.Reset();
            }

            if (IsHost)
            {
                ResetDungeonProgressClientRpc();
            }
        }

        [ClientRpc]
        private void ResetDungeonProgressClientRpc()
        {
            if (IsHost) return;

            if (DungeonProgressManager.instance != null)
            {
                DungeonProgressManager.instance.Reset();
            }
        }

        public void LoadSceneSingle(string sceneName)
        {
            //EnableInterpolation(false);
            if (sceneName == "Hub")
            {
                ResetDungeonProgress();
            }

            if (!IsHost) return;


            var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            CheckStatus(status);
        }

        public void ReturnToMainMenu()
        {
            if (PlayerUI.instance != null)
            {
                Destroy(PlayerUI.instance.gameObject);
            }
            if (CharacterManager.Instance != null)
            {
                Destroy(CharacterManager.Instance.gameObject);
            }
            if (InventoryManager.Instance != null)
            {
                Destroy(InventoryManager.Instance.gameObject);
            }
            if (SceneManager.instance != null)
            {
                Destroy(SceneManager.instance.gameObject);
            }
            if (OptionsManager.Instance != null)
            {
                Destroy(OptionsManager.Instance.gameObject);
            }
            if (GameHandler.instance != null)
            {
                Destroy(GameHandler.instance.gameObject);
            }
            if (MapHandler.instance != null)
            {
                Destroy(MapHandler.instance.gameObject);
            }
            if (CombatSystem.instance != null)
            {
                Destroy(CombatSystem.instance.gameObject);
            }
            if (SpawnPointerGrid.instance != null)
            {
                Destroy(SpawnPointerGrid.instance.gameObject);
            }
            if (HubProgressManager.instance != null)
            {
                Destroy(HubProgressManager.instance.gameObject);
            }
            if (ExchangeManager.Instance != null)
            {
                Destroy(ExchangeManager.Instance.gameObject);
            }
            if (DungeonProgressManager.instance != null)
            {
                Destroy(DungeonProgressManager.instance.gameObject);
            }
            if (DataManager.Instance != null)
            {
                Destroy(DataManager.Instance.gameObject);
            }
            
            
            if (IsHost)
            {
                ReturnToMainMenuClientRpc();
            }

            NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }

        [ClientRpc]
        private void ReturnToMainMenuClientRpc()
        {
            if (IsHost) return;

            ReturnToMainMenu();
        }

        private void LoadPlayers()
        {
            _players = FindObjectsOfType<ClientNetworkTransform>();
        }

        /*private void EnableInterpolation(bool b)
        {
            Debug.Log("EnableInterpolation " + b);
            if (_players == null)
                LoadPlayers();
            
            foreach (var player in _players)
            {
                player.Interpolate = b;
            }
        }*/

        /*public void EnableInterpolation()
        {
            EnableInterpolation(true);
            EnableInterpolationClientRpc();
        }*/

        public void LoadSceneAdditive(string sceneName)
        {
            var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            CheckStatus(status);
        }

        private void CheckStatus(SceneEventProgressStatus status, bool isLoading = true)
        {
            var sceneEventAction = isLoading ? "load" : "unload";
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to {sceneEventAction} with" +
                                 $" a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }

        /// <summary>
        /// Handles processing notifications when subscribed to OnSceneEvent
        /// </summary>
        private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
        {
            var clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? "server" : "client";
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.LoadComplete:
                {
                    // We want to handle this for only the server-side
                    if (sceneEvent.ClientId == NetworkManager.ServerClientId)
                    {
                        // *** IMPORTANT ***
                        // Keep track of the loaded scene, you need this to unload it
                        _loadedScene = sceneEvent.Scene;
                    }

                    Debug.Log($"Loaded the {sceneEvent.SceneName} scene on " +
                              $"{clientOrServer}-({sceneEvent.ClientId}).");

                    break;
                }
                case SceneEventType.UnloadComplete:
                {
                    Debug.Log($"Unloaded the {sceneEvent.SceneName} scene on " +
                              $"{clientOrServer}-({sceneEvent.ClientId}).");
                    break;
                }
                case SceneEventType.LoadEventCompleted:
                {
                    Debug.Log("Load event completed for the following client " +
                              $"identifiers:({sceneEvent.ClientsThatCompleted})");
                    if (sceneEvent.ClientsThatTimedOut.Count > 0)
                    {
                        Debug.LogWarning("Load event timed out for the following client " +
                                         $"identifiers:({sceneEvent.ClientsThatTimedOut})");
                    }

                    /*if (_loadedScene.name != "Grid")
                    {
                        Debug.Log("Notify interpolation active");
                        EnableInterpolationClientRpc();
                        EnableInterpolation(true);
                    }*/

                    break;
                }
                case SceneEventType.UnloadEventCompleted:
                {
                    Debug.Log("Unload event completed for the following client " +
                              $"identifiers:({sceneEvent.ClientsThatCompleted})");
                    if (sceneEvent.ClientsThatTimedOut.Count > 0)
                    {
                        Debug.LogWarning("Unload event timed out for the following client " +
                                         $"identifiers:({sceneEvent.ClientsThatTimedOut})");
                    }

                    break;
                }
            }
        }

        /*[ClientRpc]
        private void EnableInterpolationClientRpc()
        {
            Debug.Log("EnableInterpolationClientRpc");
            if(IsHost) return;
            
            EnableInterpolation(true);
        }*/

        public void UnloadScene()
        {
            // Assure only the server calls this when the NetworkObject is
            // spawned and the scene is loaded.
            if (!IsServer || !IsSpawned || !_loadedScene.IsValid() || !_loadedScene.isLoaded)
            {
                return;
            }

            var status = NetworkManager.SceneManager.UnloadScene(_loadedScene);
            CheckStatus(status, false);
        }
    }
}