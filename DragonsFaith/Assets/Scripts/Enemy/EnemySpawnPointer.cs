using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawnPointer : NetworkBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<GameObject> spawnPoints;
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
        foreach (var spawnPoint in spawnPoints)
        {
            var randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            var go = Instantiate(randomPrefab, spawnPoint.transform.position, Quaternion.identity, spawnPoint.transform);
            go.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void SpawnSomePoints()
    {
        var spawnPointsCopy = new List<GameObject>(spawnPoints);

        for (var i = 0; i < usePoints; i++)
        {
            var randomIndex = Random.Range(0, spawnPointsCopy.Count);
            var randomSpawnPoint = spawnPointsCopy[randomIndex];
            spawnPointsCopy.RemoveAt(randomIndex);

            var randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            var go = Instantiate(randomPrefab, randomSpawnPoint.transform.position, Quaternion.identity,
                randomSpawnPoint.transform);
            go.GetComponent<NetworkObject>().Spawn();
        }
    }
}