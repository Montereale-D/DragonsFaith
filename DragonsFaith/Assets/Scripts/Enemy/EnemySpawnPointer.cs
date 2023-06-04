using System.Collections.Generic;
using Enemy;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawnPointer : NetworkBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<EnemySpawnPoint> spawnPoints;
    [SerializeField] private bool useAllPoints = true;
    [SerializeField] private int usePoints;
    
    private List<NetworkObject> networkObjects = new List<NetworkObject>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsHost) return;

        if (useAllPoints || usePoints == spawnPoints.Count)
            SpawnAllPoints();
        else
            SpawnSomePoints();

        GetComponent<NetworkObject>().DestroyWithScene = true;
    }

    private void SpawnAllPoints()
    {
        
        for (var i = 0; i < spawnPoints.Count; i++)
        {
            var spawnPoint = spawnPoints[i];
            var randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            var go = Instantiate(randomPrefab, Vector3.zero, Quaternion.identity, spawnPoint.transform);
            go.GetComponent<EnemyBehaviour>().SetUp(spawnPoint);
            var networkObject = go.GetComponent<NetworkObject>();
            networkObjects.Add(networkObject);
        }

        for (var i = 0; i < networkObjects.Count; i++)
        {
            networkObjects[i].Spawn();
            SetUpEnemyClientRpc(networkObjects[i].NetworkObjectId, i);
        }
    }

    private void SpawnSomePoints()
    {
        var spawnPointsCopy = new List<EnemySpawnPoint>(spawnPoints);

        for (var i = 0; i < usePoints; i++)
        {
            var randomIndex = Random.Range(0, spawnPointsCopy.Count);
            var randomSpawnPoint = spawnPointsCopy[randomIndex];
            spawnPointsCopy.RemoveAt(randomIndex);

            var randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            var go = Instantiate(randomPrefab, randomSpawnPoint.transform.position, Quaternion.identity,
                randomSpawnPoint.transform);
            go.GetComponent<EnemyBehaviour>().SetUp(randomSpawnPoint);
            var networkObject = go.GetComponent<NetworkObject>();
            networkObjects.Add(networkObject);
        }

        for (var i = 0; i < networkObjects.Count; i++)
        {
            networkObjects[i].Spawn();
            SetUpEnemyClientRpc(networkObjects[i].NetworkObjectId, i);
        }
    }
    
    [ClientRpc]
    public void SetUpEnemyClientRpc(ulong objectIdToSet, int spawnPointIndex)
    {
        if(IsHost) return;
        
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(objectIdToSet, out var objToSet);
        if (objToSet == null)
        {
            Debug.LogWarning("Network object not found, if the enemy was just defeated is ok");
            return;
        }
        
        objToSet.GetComponent<EnemyBehaviour>().SetUp(spawnPoints[spawnPointIndex]);
        
    }
    
    public override void OnDestroy()
    {
        //Debug.Log("SpawnPointer OnDestroy");
        base.OnDestroy();
            
    }

    public override void OnNetworkDespawn()
    {
        //Debug.Log("SpawnPointer OnNetworkDespawn");
        base.OnNetworkDespawn();
            
    }
}