using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : NetworkBehaviour
{
    private Scene _loadedScene;

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(this);
        if (IsServer /*&& !string.IsNullOrEmpty(_mSceneName)*/)
        {
            NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
        }

        base.OnNetworkSpawn();
    }

    public bool SceneIsLoaded => _loadedScene.IsValid() && _loadedScene.isLoaded;

    public void LoadSceneSingle(string sceneName)
    {
        if (!IsHost) return;

        var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        CheckStatus(status);
    }

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
            case SceneEventType.UnloadEventCompleted:
            {
                var loadUnload = sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted ? "Load" : "Unload";
                Debug.Log($"{loadUnload} event completed for the following client " +
                          $"identifiers:({sceneEvent.ClientsThatCompleted})");
                if (sceneEvent.ClientsThatTimedOut.Count > 0)
                {
                    Debug.LogWarning($"{loadUnload} event timed out for the following client " +
                                     $"identifiers:({sceneEvent.ClientsThatTimedOut})");
                }

                break;
            }
        }
    }

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

    [ContextMenu("Change to grid scene")]
    public void LoadGrid()
    {
        LoadSceneSingle("Grid");
    }
}