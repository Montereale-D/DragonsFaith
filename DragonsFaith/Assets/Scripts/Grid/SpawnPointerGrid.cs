using System.Collections.Generic;
using System.Linq;
using Player;
using Save;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnPointerGrid : NetworkBehaviour
{
    public static SpawnPointerGrid instance { get; private set; }
    
    [SerializeField] private int halfHeightMap = 3;
    [SerializeField] private int halfWidthMap = 7;
    [SerializeField] private Vector2Int spawnPointPlayer1;
    [SerializeField] private Vector2Int spawnPointPlayer2;
    
    private List<Vector2Int> spawnPointEnemies;
    private List<Vector2Int> spawnPointObstacles;
    private List<PlayerMovement> _players;
    
    public GameObject characterMelee;
    public GameObject characterRanged;
    public GameObject destroyableObject;
    public GameObject undestroyableObject;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
       
        //GenerateEnemies();
        spawnPointEnemies = new List<Vector2Int>();
        spawnPointObstacles = new List<Vector2Int>();
    }
    

    public override void OnNetworkSpawn()
    {
        _players = FindObjectsOfType<PlayerMovement>().ToList();
        foreach (var player in _players)
        {
            var position = player.IsHost ? spawnPointPlayer1 : spawnPointPlayer2;
            player.ForcePosition(new Vector3(position.x, position.y, 0));
        }
        
        GetComponent<NetworkObject>().DestroyWithScene = true;
        
        if (!IsHost) return;
        Debug.Log("SpawnPointerGrid OnNetworkSpawn");
        
        GenerateEnemies();
        
        OnReadyClientRpc();
        GameHandler.instance.Setup();
    }

    [ClientRpc]
    private void OnReadyClientRpc()
    {
        if(IsHost) return;
        
        GameHandler.instance.Setup();
    }

    private void GenerateEnemies()
    {
        var enemyCount = Random.Range(1, 4);
        //DestroyRandomFromList(enemyGameObjects, 4 - enemyCount);
        
        //var obstaclesCount = Random.Range(4, 7);
        //DestroyRandomFromList(obstacleGameObjects, 7 - obstaclesCount);

        var topLeft = new Vector2Int(-halfWidthMap, halfHeightMap);
        var topMiddle = new Vector2Int(0, halfHeightMap);
        var bottomRight = new Vector2Int(halfWidthMap, -halfHeightMap);
        
        spawnPointEnemies = GenerateSpawnPoints(enemyCount, topMiddle, bottomRight);
        //spawnPointObstacles = GenerateSpawnPoints(obstaclesCount, topLeft, bottomRight);

        for (var i = 0; i < enemyCount; i++)
        {
            var spawnPoint = spawnPointEnemies[i];
            var position = new Vector3(spawnPoint.x, spawnPoint.y, 0);
            //var randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            var go = Instantiate(characterMelee, position, Quaternion.identity);
            //go.GetComponent<EnemyBehaviour>().SetUp(spawnPoint);
            var networkObject = go.GetComponent<NetworkObject>();
            
            networkObject.Spawn();
            SetUpEnemyClientRpc(networkObject.NetworkObjectId, position);
        }
    }
    
    [ClientRpc]
    public void SetUpEnemyClientRpc(ulong objectIdToSet, Vector3 position)
    {
        if(IsHost) return;
        
        spawnPointEnemies.Add(new Vector2Int((int)position.x, (int)position.y));
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(objectIdToSet, out var objToSet);
        if (objToSet == null)
        {
            Debug.LogWarning("Network object not found, if the enemy was just defeated is ok");
            return;
        }

        objToSet.transform.position = position;
    }

    private void DestroyRandomFromList(List<GameObject> gameObjects, int destroyCount)
    {
        for (int i = 0; i < destroyCount; i++)
        {
            var randomIndex = Random.Range(0, gameObjects.Count - 1);
            var go = gameObjects[randomIndex];
            gameObjects.RemoveAt(randomIndex);
            DestroyImmediate(go);
        }
    }

    private List<Vector2Int> GenerateSpawnPoints(int n, Vector2Int topLeft, Vector2Int bottomRight)
    {
        var tmpList = new List<Vector2Int>();
        var maxX = bottomRight.x;
        var minX = topLeft.x;
        var maxY = topLeft.y;
        var minY = bottomRight.y;

        var i = 0;
        while (i < n)
        {
            var x = Random.Range(minX, maxX + 1);
            var y = Random.Range(minY, maxY + 1);
            var point = new Vector2Int(x, y);
            
            if (IsAvailable(point))
            {
                tmpList.Add(point);
                i++;
            }
        }

        return tmpList;
    }

    private bool IsAvailable(Vector2Int position)
    {
        if (position == spawnPointPlayer1 || position == spawnPointPlayer2)
        {
            return false;
        }

        if (spawnPointEnemies != null)
        {
            if (spawnPointEnemies.Contains(position))
            {
                return false;
            }
        }

        if (spawnPointObstacles != null)
        {
            if (spawnPointObstacles.Contains(position))
            {
                return false;
            }
        }

        return true;
    }

    public Vector2Int GetPlayerSpawnPoint(GameData.PlayerType playerType)
    {
        return playerType == GameData.PlayerType.Host ? spawnPointPlayer1 : spawnPointPlayer2;
    }

    public List<Vector2Int> GetEnemySpawnPoint()
    {
        return spawnPointEnemies;
    }

    public List<Vector2Int> GetObstaclesSpawnPoint()
    {
        return spawnPointObstacles;
    }
}