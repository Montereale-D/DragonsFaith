using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public bool patrol = true;
    public float waitOnPatrolPosition = 2f;
    public List<Transform> patrolPositions;
    public override string ToString()
    {
        return "Patrol: " + patrol + " WaitTime " + waitOnPatrolPosition + " Points Number " + patrolPositions.Count;
    }
}
