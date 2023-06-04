using System;
using System.Collections.Generic;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class DungeonProgressManager : MonoBehaviour
{
    public static DungeonProgressManager instance { get; private set; }
    private Dictionary<string, bool> _chestData;
    private Dictionary<string, bool> _buttonsData;
    private Dictionary<string, bool> _enemyData;
    private Dictionary<string, bool> _abilityData;
    private Vector3 _hostPosition;
    private Vector3 _clientPosition;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        _chestData = new Dictionary<string, bool>();
        _buttonsData = new Dictionary<string, bool>();
        _enemyData = new Dictionary<string, bool>();
        _abilityData = new Dictionary<string, bool>();
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        Debug.Log("Dungeon progress setup");
    }

    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        if (next.name is "Grid" or "Dungeon") return;

        Debug.Log("Scene is not dungeon or grid, deleting dungeon data ...");
        Destroy(gameObject);
    }

    public void ResetData()
    {
        //useful?
    }

    public void ChestOpened(string uid)
    {
        CheckUid(uid);
        Debug.Log("ChestOpened for " + uid);
        _chestData.TryAdd(uid, true);
    }

    public bool IsChestOpened(string uid)
    {
        CheckUid(uid);
        Debug.Log("IsChestOpened for " + uid);
        return _chestData.TryGetValue(uid, out _);
    }

    public void ButtonChangeState(string uid, bool isPressed)
    {
        CheckUid(uid);
        Debug.Log("ButtonChangeState for " + uid);
        _buttonsData.TryAdd(uid, true);
    }

    public bool IsButtonPressed(string uid)
    {
        CheckUid(uid);
        Debug.Log("IsButtonPressed for " + uid);
        return _buttonsData.TryGetValue(uid, out _);
    }
    
    public void AbilityPassed(string uid)
    {
        CheckUid(uid);
        Debug.Log("AbilityPassed for " + uid);
        _enemyData.TryAdd(uid, true);
    }

    public bool IsAbilityPassed(string uid)
    {
        CheckUid(uid);
        Debug.Log("IsAbilityPassed for " + uid);
        return _enemyData.TryGetValue(uid, out _);
    }

    public void EnemyDefeated(string uid)
    {
        CheckUid(uid);
        Debug.Log("EnemyDefeated for " + uid);
        _enemyData.TryAdd(uid, true);
    }

    public bool IsEnemyDefeated(string uid)
    {
        CheckUid(uid);
        Debug.Log("IsEnemyDefeated for " + uid);
        return _enemyData.TryGetValue(uid, out _);
    }

    public void UpdateSpawnPoint(Vector3 position, GameData.PlayerType playerType)
    {
        if (playerType == GameData.PlayerType.Host)
        {
            _hostPosition = position;
        }
        else
        {
            _clientPosition = position;
        }
    }

    public Vector3 GetSpawnPoint(GameData.PlayerType playerType)
    {
        return playerType == GameData.PlayerType.Host ? _hostPosition : _clientPosition;
    }
    
    private void CheckUid(string uid)
    {
        if (string.IsNullOrEmpty(uid))
        {
            throw new Exception("Not valid uid");
        }
    }
}