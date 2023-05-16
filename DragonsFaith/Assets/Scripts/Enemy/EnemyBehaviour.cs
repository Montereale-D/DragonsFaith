using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Enemy
{
    public class EnemyBehaviour : NetworkBehaviour
    {
        private EnemySpawnPoint _spawnPoint;
        private bool _patrol = true;
        public float _waitOnPatrolPosition = 2f;
        private List<Transform> _patrolPositions;
        public FieldOfView fieldOfView;

        [SerializeField] private float speed = 4f;
        [SerializeField] private Transform characterTransform;

        private Vector3 _nextPosition;
        private int _positionIndex;
        private bool _keepMoving = true;
        
        //private readonly NetworkVariable<bool> _setUpDone = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public void SetUp(EnemySpawnPoint spawnPoint)
        {
            Debug.Log("Enemy spawned setup");

            //Instantiated as a child of a EnemySpawnPoint, get params
            _spawnPoint = spawnPoint;
            _patrol = _spawnPoint.patrol;
            _waitOnPatrolPosition = _spawnPoint.waitOnPatrolPosition;
            _patrolPositions = _spawnPoint.patrolPositions;

            //Init
            _positionIndex = 1;
            _nextPosition = _patrolPositions[1].position;
            characterTransform.position = _patrolPositions[0].position;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            GetComponent<NetworkObject>().DestroyWithScene = true;
        }

        private void Update()
        {
            var charPos = characterTransform.position;

            //Need to be updated for the correct mesh rendering
            fieldOfView.SetOrigin(charPos);
            fieldOfView.SetAimDirection((_nextPosition - charPos).normalized);

            if (!_patrol) return;

            if (!_keepMoving) return;

            if (!IsHost) return;
            
            characterTransform.position = Vector3.MoveTowards(charPos, _nextPosition,
                speed * 0.003f /*Time.deltaTime*/ );

            if (IsPositionReached())
            {
                _keepMoving = false;
                StartCoroutine(WaitOnPosition());
            }
        }

        private void PrintDebug(string s)
        {
            if (IsHost)
            {
                Debug.Log("[HOST] " + s);
            }
            else
            {
                DebugServerRpc(s);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void DebugServerRpc(string s)
        {
            Debug.Log("[CLIENT] " + s);
        }

        private IEnumerator WaitOnPosition()
        {
            //LoadNextPositionClientRpc(LoadNextPosition());
            yield return new WaitForSeconds(_waitOnPatrolPosition);
            LoadNextPositionClientRpc(LoadNextPosition());
            _keepMoving = true;
        }

        [ClientRpc]
        private void LoadNextPositionClientRpc(Vector3 position)
        {
            if (!IsHost)
            {
                _nextPosition = position;
            }
        }

        private Vector3 LoadNextPosition()
        {
            _positionIndex++;
            if (_positionIndex >= _patrolPositions.Count)
            {
                _positionIndex = 0;
            }

            _nextPosition = _patrolPositions[_positionIndex].position;
            return _nextPosition;
        }

        private bool IsPositionReached()
        {
            return (_nextPosition - characterTransform.position).magnitude < 0.1;
        }

        public override void OnDestroy()
        {
            Debug.Log("Enemy OnDestroy");
            base.OnDestroy();
            
        }

        public override void OnNetworkDespawn()
        {
            Debug.Log("Enemy OnNetworkDespawn");
            base.OnNetworkDespawn();
            
        }
    }
}