using System;
using System.Collections;
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
            }
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
            if(IsHost) return;
            
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

            StartCoroutine(StartTransition(sceneName));
            /*var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            CheckStatus(status);*/
        }
        
        private IEnumerator StartTransition(string sceneName)
        {
            TransitionBackground.instance.FadeOut();
            if (IsHost)
            {
                StartTransitionClientRpc();
            }
            else
            {
                StartTransitionServerRpc();
            }

            yield return new WaitForSeconds(1f);
            var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            CheckStatus(status);
            
            TransitionBackground.instance.FadeIn();
            if (IsHost)
            {
                EndTransitionClientRpc();
            }
            else
            {
                EndTransitionServerRpc();
            }
        }

        [ClientRpc]
        private void StartTransitionClientRpc()
        {
            TransitionBackground.instance.FadeOut();
        }
        
        [ServerRpc(RequireOwnership = false)]
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