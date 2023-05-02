using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private EnemySpawnPoint _spawnPoint;
    private bool _patrol = true;
    private float _waitOnPatrolPosition = 2f;
    private List<Transform> _patrolPositions;

    [Header("Detect")] [SerializeField] private float reactionTime = 2f;
    [SerializeField] private float detectRange = 5f;

    [Header("Other")] [SerializeField] private float speed = 4f;
    [SerializeField] private Transform characterTransform;

    private Vector3 _nextPosition;
    private int _positionIndex;
    private bool _keepMoving = true;

    private void Awake()
    {
        _spawnPoint = GetComponentInParent<EnemySpawnPoint>();
        _patrol = _spawnPoint.patrol;
        _waitOnPatrolPosition = _spawnPoint.waitOnPatrolPosition;
        _patrolPositions = _spawnPoint.patrolPositions;
        
        if (_patrolPositions.Count > 1)
        {
            _positionIndex = 0;
            _nextPosition = _patrolPositions[0].localPosition;
        }

        if (_patrol)
        {
            StartCoroutine(Patrol());
        }

        StartCoroutine(Detect());
    }

    private void Update()
    {
        if (_keepMoving)
        {
            characterTransform.localPosition = Vector3.MoveTowards(characterTransform.localPosition, _nextPosition,
                speed * Time.deltaTime);
        }
    }

    private IEnumerator Detect()
    {
        while (true)
        {
            if (EnemiesAround())
            {
                //do stuff
            }

            yield return new WaitForSeconds(reactionTime);
        }
    }

    private IEnumerator Patrol()
    {
        while (true)
        {
            if (IsPositionReached())
            {
                _keepMoving = false;
                LoadNextPosition();
            }

            yield return new WaitForSeconds(_waitOnPatrolPosition);
            _keepMoving = true;
        }
    }

    public bool EnemiesAround()
    {
        foreach (var go in GameObject.FindGameObjectsWithTag("Player"))
        {
            if ((go.transform.position - characterTransform.position).magnitude <= detectRange) return true;
        }

        return false;
    }

    private void LoadNextPosition()
    {
        _positionIndex++;
        if (_positionIndex >= _patrolPositions.Count)
        {
            _positionIndex = 0;
        }

        _nextPosition = _patrolPositions[_positionIndex].localPosition;
    }

    private bool IsPositionReached()
    {
        return (_nextPosition - characterTransform.localPosition).magnitude < 0.1;
    }
}