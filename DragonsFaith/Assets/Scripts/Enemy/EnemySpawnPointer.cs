using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawnPointer : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<GameObject> spawnPoints;
    [SerializeField] private bool useAllPoints = true;
    [SerializeField] private int usePoints;

    private void Awake()
    {
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
            Instantiate(randomPrefab, spawnPoint.transform.position, Quaternion.identity, spawnPoint.transform);
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
            Instantiate(randomPrefab, randomSpawnPoint.transform.position, Quaternion.identity, randomSpawnPoint.transform);
        }
    }
}