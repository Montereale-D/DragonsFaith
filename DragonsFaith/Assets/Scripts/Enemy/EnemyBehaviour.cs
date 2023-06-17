using System.Collections;
using System.Collections.Generic;
using Network;
using Save;
using Unity.Netcode;
using UnityEngine;

namespace Enemy
{
    public class EnemyBehaviour : NetworkBehaviour
    {
        private string _saveId;
        private bool _isMiniboss;
        private EnemySpawnPoint _spawnPoint;
        private bool _patrol = true;
        public float _waitOnPatrolPosition = 2f;
        private List<Transform> _patrolPositions;
        public FieldOfView fieldOfView;

        [SerializeField] private float speed = 4f;
        [SerializeField] private Transform characterTransform;
        [SerializeField] private Color minibossColor;

        private Vector3 _nextPosition;
        private int _positionIndex;
        private bool _keepMoving = true;

        //use this in order to avoid unwanted collision with player on scene reloading
        private bool _isReadyToFight;
        
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private static readonly int Moving = Animator.StringToHash("isMoving");
        private static readonly int X = Animator.StringToHash("x");
        private float _previousDir;

        //private readonly NetworkVariable<bool> _setUpDone = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public void SetUp(EnemySpawnPoint spawnPoint)
        {
            Debug.Log("Enemy spawned setup " + spawnPoint.saveId);

            //Instantiated as a child of a EnemySpawnPoint, get params
            _isMiniboss = spawnPoint.isMiniboss;
            _spawnPoint = spawnPoint;
            _patrol = _spawnPoint.patrol;
            _waitOnPatrolPosition = _spawnPoint.waitOnPatrolPosition;
            _patrolPositions = _spawnPoint.patrolPositions;
            _saveId = spawnPoint.saveId;

            //Init
            _positionIndex = 1;
            _nextPosition = _patrolPositions[1].position;
            characterTransform.position = _patrolPositions[0].position;
            
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer && _isMiniboss)
            {
                _spriteRenderer.color = minibossColor;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (IsHost)
            {
                GetComponent<NetworkObject>().DestroyWithScene = true;
                _animator = GetComponentInChildren<Animator>();
                //_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (IsHost && DungeonProgressManager.instance.IsEnemyDefeated(_saveId, gameObject))
            {
                Debug.Log(gameObject.name + " was already defeated");
                Destroy(gameObject);
                return;
            }

            StartCoroutine(WaitToFight());
        }

        private IEnumerator WaitToFight()
        {
            yield return new WaitForSecondsRealtime(2f);
            _isReadyToFight = true;
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
            _animator.SetBool(Moving, true);
            //_spriteRenderer.flipX = (_nextPosition - charPos).normalized == Vector3.left;
            if (_previousDir != 0) _animator.SetFloat(X, _previousDir);
            _previousDir = (_nextPosition - charPos).normalized.x;

            if (IsPositionReached())
            {
                _keepMoving = false;
                _animator.SetBool(Moving, false);
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
            //Debug.Log("Enemy OnNetworkDespawn");
            base.OnNetworkDespawn();
        }

        public void OnCombatStart()
        {
            OnCombatStart(characterTransform.position);
        }
        public void OnCombatStart(Vector3 position)
        {
            if (!_isReadyToFight)
            {
                Debug.Log("Not ready to CombatStart || already started");
                return;
            }

            _isReadyToFight = false;
            fieldOfView.Stop();
            _keepMoving = false;

            Debug.Log("OnCombatStart " + position);
            
            DungeonProgressManager.instance.EnemyDefeated(_saveId, gameObject);
            DungeonProgressManager.instance.UpdateSpawnPoint(position, GameData.PlayerType.Host);
            DungeonProgressManager.instance.UpdateSpawnPoint(position, GameData.PlayerType.Client);
            //TransitionBackground.instance.FadeOut();

            if (_isMiniboss && IsHost)
            {
                var newCounter = HubProgressManager.keyCounter + 1;
                MinibossDefeatedClientRpc(newCounter);
                MinibossDefeated(newCounter);
            }

            if (IsHost)
            {
                //OnCombatStartClientRpc(position);
                //Destroy(gameObject);

                var done = SceneManager.instance.LoadSceneSingle("Grid");
                if (done)
                {
                    OnCombatStartClientRpc(position);
                }
            }
        }

        [ClientRpc]
        private void MinibossDefeatedClientRpc(int counter)
        {
            if(IsHost) return;
            MinibossDefeated(counter);
        }

        private void MinibossDefeated(int counter)
        {
            DungeonProgressManager.instance.MinibossDefeated();
            HubProgressManager.keyCounter = counter;
        }

        [ClientRpc]
        private void OnCombatStartClientRpc(Vector3 position)
        {
            Debug.Log("OnCombatStartClientRpc " + position);
            SceneManager.instance.LoadSceneSingle("Grid");
            DungeonProgressManager.instance.EnemyDefeated(_saveId, gameObject);
            DungeonProgressManager.instance.UpdateSpawnPoint(position, GameData.PlayerType.Host);
            DungeonProgressManager.instance.UpdateSpawnPoint(position, GameData.PlayerType.Client);
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnCombatStartServerRpc(Vector3 posiiton)
        {
            if (IsHost)
            {
                OnCombatStart(posiiton);
            }
        }
    }
}