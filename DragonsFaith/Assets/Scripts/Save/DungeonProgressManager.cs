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
    private Dictionary<GameData.PlayerType, Vector3> _spawnData;
    private bool _isMinibossDefeated;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        //SceneManager.activeSceneChanged += OnActiveSceneChanged;

        _chestData = new Dictionary<string, bool>();
        _buttonsData = new Dictionary<string, bool>();
        _enemyData = new Dictionary<string, bool>();
        _abilityData = new Dictionary<string, bool>();
        _spawnData = new Dictionary<GameData.PlayerType, Vector3>();
    }

    /*private void OnActiveSceneChanged(Scene current, Scene next)
    {
        if (next.name is "Grid" or "Dungeon") return;

        Debug.Log("Scene is not dungeon or grid, deleting dungeon data ...");
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        Destroy(gameObject);
    }*/

    public void Reset()
    {
        Debug.Log("Scene is not dungeon or grid, deleting dungeon data ...");
        instance = null;
        Destroy(gameObject);
    }

    public void ChestOpened(string uid, GameObject go)
    {
        CheckUid(uid, go);
        Debug.Log("ChestOpened for " + uid);
        _chestData.TryAdd(uid, true);
    }

    public bool IsChestOpened(string uid, GameObject go)
    {
        CheckUid(uid, go);
        Debug.Log("IsChestOpened for " + uid);
        return _chestData.TryGetValue(uid, out _);
    }

    public void ButtonChangeState(string uid, bool isPressed, GameObject go)
    {
        CheckUid(uid, go);
        Debug.Log("ButtonChangeState for " + uid);
        _buttonsData.TryAdd(uid, true);
    }

    public bool IsButtonPressed(string uid, GameObject go)
    {
        CheckUid(uid, go);
        Debug.Log("IsButtonPressed for " + uid);
        return _buttonsData.TryGetValue(uid, out _);
    }
    
    public void AbilityPassed(string uid, GameObject go)
    {
        //todo controllare che funzioni
        CheckUid(uid, go);
        var value = _abilityData.TryAdd(uid, true);
        //Debug.Log("AbilityPassed for " + uid + " : " + value);
        
    }

    public bool IsAbilityPassed(string uid, GameObject go)
    {
        CheckUid(uid, go);
        _abilityData.TryGetValue(uid, out bool value);
        //Debug.Log("IsAbilityPassed for " + uid + " : " + value);
        return value;
    }

    public void EnemyDefeated(string uid, GameObject go)
    {
        CheckUid(uid, go);
        //Debug.Log("EnemyDefeated for " + uid);
        _enemyData.TryAdd(uid, true);
    }

    public bool IsEnemyDefeated(string uid, GameObject go)
    {
        CheckUid(uid, go);
        //Debug.Log("IsEnemyDefeated for " + uid);
        return _enemyData.TryGetValue(uid, out _);
    }

    public void UpdateSpawnPoint(Vector3 position, GameData.PlayerType playerType)
    {
        Debug.Log("UpdateSpawnPoint " + position);
        if (!_spawnData.TryAdd(playerType, position))
        {
            _spawnData.Remove(playerType);
            _spawnData.Add(playerType, position);
        }
    }

    public Vector3? GetSpawnPoint(GameData.PlayerType playerType)
    {
        var result = _spawnData.TryGetValue(playerType, out Vector3 value);
        Debug.Log("GetSpawnPoint " + value);
        return result ? value : null;
    }

    public void MinibossDefeated()
    {
        _isMinibossDefeated = true;
    }
    
    private void CheckUid(string uid, GameObject go)
    {
        if (uid == null)
        {
            throw new Exception("Null uid of " + go.name);
        }
        if (uid == "")
        {
            throw new Exception("Empty uid of " + go.name);
        }
    }
}