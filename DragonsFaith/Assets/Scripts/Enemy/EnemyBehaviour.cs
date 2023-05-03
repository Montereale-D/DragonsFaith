using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyBehaviour : MonoBehaviour
    {
        private EnemySpawnPoint _spawnPoint;
        private bool _patrol = true;
        private float _waitOnPatrolPosition = 2f;
        private List<Transform> _patrolPositions;
        public FieldOfView fieldOfView;

        [SerializeField] private float speed = 4f;
        [SerializeField] private Transform characterTransform;

        private Vector3 _nextPosition;
        private int _positionIndex;
        private bool _keepMoving = true;

        private void Awake()
        {
            //Instantiated as a child of a EnemySpawnPoint, get params
            _spawnPoint = GetComponentInParent<EnemySpawnPoint>();
            _patrol = _spawnPoint.patrol;
            _waitOnPatrolPosition = _spawnPoint.waitOnPatrolPosition;
            _patrolPositions = _spawnPoint.patrolPositions;

            //Init
            _positionIndex = 0;
            _nextPosition = _patrolPositions[0].position;
        }

        private void Update()
        {
            var charPos = characterTransform.position;

            //Need to be updated for the correct mesh rendering
            fieldOfView.SetOrigin(charPos);
            fieldOfView.SetAimDirection((_nextPosition - charPos).normalized);
            fieldOfView.transform.position = transform.localPosition; //offset for the correct mesh positioning

            if (!_patrol) return;

            if (!_keepMoving) return;

            characterTransform.position = Vector3.MoveTowards(charPos, _nextPosition, speed * Time.deltaTime);

            if (IsPositionReached())
            {
                _keepMoving = false;
                StartCoroutine(WaitOnPosition());
            }
        }

        private IEnumerator WaitOnPosition()
        {
            yield return new WaitForSeconds(_waitOnPatrolPosition);
            LoadNextPosition();
            _keepMoving = true;
        }

        private void LoadNextPosition()
        {
            _positionIndex++;
            if (_positionIndex >= _patrolPositions.Count)
            {
                _positionIndex = 0;
            }

            _nextPosition = _patrolPositions[_positionIndex].position;
        }

        private bool IsPositionReached()
        {
            return (_nextPosition - characterTransform.position).magnitude < 0.1;
        }
    }
}