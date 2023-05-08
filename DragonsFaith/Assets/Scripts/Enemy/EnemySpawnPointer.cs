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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsHost) return;

        if (useAllPoints || usePoints == spawnPoints.Count)
            SpawnAllPoints();
        else
            SpawnSomePoints();
    }

    private void SpawnAllPoints()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            var spawnPoint = spawnPoints[i];
            var randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            var go = Instantiate(randomPrefab, Vector3.zero, Quaternion.identity, spawnPoint.transform);
            go.GetComponent<EnemyBehaviour>().SetUp(spawnPoint);
            var networkObject = go.GetComponent<NetworkObject>();
            networkObject.Spawn();
            SetUpEnemyClientRpc(networkObject.NetworkObjectId, i);
        }
    }

    [ClientRpc]
    public void SetUpEnemyClientRpc(ulong objectIdToSet, int spawnPointIndex)
    {
        if(IsHost) return;
        
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(objectIdToSet, out var objToSet);
        if (objToSet == null)
        {
            Debug.LogError("Network object not found");
            return;
        }
        
        objToSet.GetComponent<EnemyBehaviour>().SetUp(spawnPoints[spawnPointIndex]);
        
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
            go.GetComponent<NetworkObject>().Spawn();
        }
    }
}