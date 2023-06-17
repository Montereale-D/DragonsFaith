using System;
using Grid;
using Inventory;
using Player;
using Save;
using UI;
using System.Collections;
using System.Collections.Generic;
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
        private bool _isLoading;
        private bool isFirstDungeon = true;

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

        public override void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
                //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            }

            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnect;
        }

        private void OnSceneLoaded(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
        {
            if(!_isLoading) return;
            
            Debug.Log("On scene loaded");
            TransitionBackground.instance.FadeIn();
            _isLoading = false;
        }

        private void OnPlayerDisconnect(ulong clientId)
        {
            ReturnToMainMenu(false);
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

        public bool LoadSceneSingle(string sceneName)
        {
            if(_isLoading) return false;

            _isLoading = true;
            
            //EnableInterpolation(false);
            if (sceneName == "Hub")
            {
                ResetDungeonProgress();
            }

            //if (!IsHost) return;

            StartCoroutine(StartTransition(sceneName));
            
            //var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            //CheckStatus(status); 
            return true;
        }
        
        private IEnumerator StartTransition(string sceneName)
        {
            TransitionBackground.instance.FadeOut();
            
            /*if (IsHost)
            {
                StartTransitionClientRpc();
            }*/
            /*else
            {
                StartTransitionServerRpc();
            }*/

            yield return new WaitForSeconds(2f);

            if (!IsHost) yield break;
            
            var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            CheckStatus(status);

            //yield return new WaitForSecondsRealtime(1.5f);
            //TransitionBackground.instance.FadeIn();
            
            /*if (IsHost)
            {
                EndTransitionClientRpc();
            }
            else
            {
                EndTransitionServerRpc();
            }*/
        }

        [ClientRpc]
        private void StartTransitionClientRpc()
        {
            TransitionBackground.instance.FadeOut();
        }
        
        /*[ServerRpc(RequireOwnership = false)]
        private void StartTransitionServerRpc()
        {
            TransitionBackground.instance.FadeOut();
        }
        
        [ClientRpc]
        private void EndTransitionClientRpc()
        {
            TransitionBackground.instance.FadeIn();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void EndTransitionServerRpc()
        {
            TransitionBackground.instance.FadeIn();
        }*/

        public void ReturnToMainMenu(bool notifyClient)
        {
            Debug.Log("ReturnToMainMenu");
            _isLoading = true;
            CleanDontDestroy();

            if (IsHost && notifyClient)
            {
                ReturnToMainMenuClientRpc();
            }

            isFirstDungeon = true;
            NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }

        private static void CleanDontDestroy()
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

            if (SpawnPointerGridBoss.instance != null)
            {
                Destroy(SpawnPointerGridBoss.instance.gameObject);
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
        }

        [ClientRpc]
        private void ReturnToMainMenuClientRpc()
        {
            if (IsHost) return;

            ReturnToMainMenu(false);
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

        private string dungeonSceneName;
        public void LoadSceneSingleDungeon(string sceneName)
        {
            dungeonSceneName = sceneName;
            LoadSceneSingle(sceneName);
            if (!isFirstDungeon) return;
            PlayerUI.instance.StartDungeonTutorial();
            isFirstDungeon = false;
        }

        public void ReloadSceneSingleDungeon()
        {
            Debug.Log("ReloadSceneSingleDungeon");
            LoadSceneSingle(dungeonSceneName);
        }
    }
}