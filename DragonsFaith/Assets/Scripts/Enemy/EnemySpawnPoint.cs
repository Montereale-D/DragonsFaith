using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public bool patrol = true;
    public float waitOnPatrolPosition = 2f;
    public List<Transform> patrolPositions;
}
