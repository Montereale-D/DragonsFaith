using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : NetworkBehaviour
{
    /// INFO: You can remove the #if UNITY_EDITOR code segment and make SceneName public,
    /// but this code assures if the scene name changes you won't have to remember to
    /// manually update it.
#if UNITY_EDITOR
    public UnityEditor.SceneAsset sceneAsset;

    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            _mSceneName = sceneAsset.name;
        }
    }
#endif

    private string _mSceneName;
    private Scene _mLoadedScene;

    public override void OnNetworkSpawn()
    {
        if (IsServer && !string.IsNullOrEmpty(_mSceneName))
        {
            NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
        }

        base.OnNetworkSpawn();
    }

    public bool SceneIsLoaded => _mLoadedScene.IsValid() && _mLoadedScene.isLoaded;

    public void LoadSceneSingle()
    {
        var status = NetworkManager.SceneManager.LoadScene(_mSceneName, LoadSceneMode.Single);
        CheckStatus(status);
    }

    public void LoadSceneAdditive()
    {
        var status = NetworkManager.SceneManager.LoadScene(_mSceneName, LoadSceneMode.Additive);
        CheckStatus(status);
    }

    private void CheckStatus(SceneEventProgressStatus status, bool isLoading = true)
    {
        var sceneEventAction = isLoading ? "load" : "unload";
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to {sceneEventAction} {_mSceneName} with" +
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
                    _mLoadedScene = sceneEvent.Scene;
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
        if (!IsServer || !IsSpawned || !_mLoadedScene.IsValid() || !_mLoadedScene.isLoaded)
        {
            return;
        }
        
        var status = NetworkManager.SceneManager.UnloadScene(_mLoadedScene);
        CheckStatus(status, false);
    }
}