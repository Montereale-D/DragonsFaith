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
    /*[SerializeField] private bool isBossRoom;*/
    
    private List<Vector2Int> spawnPointEnemies;
    private List<Vector2Int> spawnPointObstacles;
    private List<PlayerMovement> _players;
    
    public GameObject[] enemyPrefabs;
    public GameObject[] obstaclePrefabs;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        
        spawnPointEnemies = new List<Vector2Int>();
        spawnPointObstacles = new List<Vector2Int>();
    }

    public override void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
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
        
        PopulateGrid();
        
        OnReadyClientRpc();
        GameHandler.instance.Setup();
    }

    [ClientRpc]
    private void OnReadyClientRpc()
    {
        if(IsHost) return;
        
        GameHandler.instance.Setup();
    }

    private void PopulateGrid()
    {
        var enemyCount = Random.Range(1, 4);
        var obstaclesCount = Random.Range(4, 7);

        var topLeft = new Vector2Int(-halfWidthMap, halfHeightMap);
        var topMiddle = new Vector2Int(0, halfHeightMap);
        var bottomRight = new Vector2Int(halfWidthMap, -halfHeightMap);
        /*if (!isBossRoom) */
        SpawnEnemy(topMiddle, bottomRight, enemyCount);
        SpawnObstacle(topLeft, bottomRight, obstaclesCount);
    }

    private void SpawnObstacle(Vector2Int topLeft, Vector2Int bottomRight, int obstaclesCount)
    {
        spawnPointObstacles = GenerateSpawnPoints(obstaclesCount, topLeft, bottomRight);
        
        for (var i = 0; i < obstaclesCount; i++)
        {
            var spawnPoint = spawnPointObstacles[i];
            var position = new Vector3(spawnPoint.x, spawnPoint.y, -1);
            var randomPrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            var go = Instantiate(randomPrefab, position, Quaternion.identity);
            var networkObject = go.GetComponent<NetworkObject>();
            
            networkObject.Spawn(true);
            SetUpObstacleClientRpc(networkObject.NetworkObjectId, position);
        }
    }

    private void SpawnEnemy(Vector2Int topLeft, Vector2Int bottomRight, int enemyCount)
    {
        spawnPointEnemies = GenerateSpawnPoints(enemyCount, topLeft, bottomRight);
        
        for (var i = 0; i < enemyCount; i++)
        {
            var spawnPoint = spawnPointEnemies[i];
            var position = new Vector3(spawnPoint.x, spawnPoint.y, 0);
            var randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var go = Instantiate(randomPrefab, position, Quaternion.identity);
            go.GetComponent<PlayerGridMovement>().enemyLocalOrder = i;
            var networkObject = go.GetComponent<NetworkObject>();
            
            networkObject.Spawn(true);
            SetUpEnemyClientRpc(networkObject.NetworkObjectId, position, i);
        }
    }
    
    [ClientRpc]
    private void SetUpEnemyClientRpc(ulong objectIdToSet, Vector3 position, int enemyLocalOrder)
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
        objToSet.GetComponent<PlayerGridMovement>().enemyLocalOrder = enemyLocalOrder;
    }
    
    [ClientRpc]
    private void SetUpObstacleClientRpc(ulong objectIdToSet, Vector3 position)
    {
        if(IsHost) return;
        
        spawnPointObstacles.Add(new Vector2Int((int)position.x, (int)position.y));
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(objectIdToSet, out var objToSet);
        if (objToSet == null)
        {
            Debug.LogWarning("Network object not found, if the enemy was just defeated is ok");
            return;
        }

        objToSet.transform.position = position;
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