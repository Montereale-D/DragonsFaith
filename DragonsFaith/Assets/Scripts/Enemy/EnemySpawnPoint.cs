using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public bool isMiniboss;
    public bool patrol = true;
    public float waitOnPatrolPosition = 2f;
    public List<Transform> patrolPositions;
    public string saveId;
    public override string ToString()
    {
        return "Patrol: " + patrol + " WaitTime " + waitOnPatrolPosition + " Points Number " + patrolPositions.Count;
    }

    private void Awake()
    {
        Debug.Log("EnemySpawn id " + saveId);
    }


    [ContextMenu("Generate guid")]
    private void GenerateGuid()
    {
        saveId = System.Guid.NewGuid().ToString();
    }
}
