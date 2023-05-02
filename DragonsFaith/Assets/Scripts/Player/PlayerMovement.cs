using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float speed = 3f;
        [SerializeField] private float fastSpeed = 6f;

        private float _speed;

        private void Awake()
        {
            _speed = speed;
        }

        public void ForcePosition(Vector3 position)
        {
            if (!IsOwner) return;

            transform.position = position;
        }

        /*private void Start()
        {
            var spawnPointer = FindObjectOfType<SpawnPointer>();
            if (!spawnPointer) return;
            
            var playerType = IsHost ? GameData.PlayerType.Host : GameData.PlayerType.Client;
            var position = spawnPointer.GetSpawnPoint(playerType);
            transform.position = position;
        }*/

        private void Update()
        {
            if (!IsLocalPlayer) return;
        
            var moveDir = new Vector3(0, 0, 0);

            if (Input.GetKey(KeyCode.W)) moveDir.y = +1f;
            if (Input.GetKey(KeyCode.S)) moveDir.y = -1f;
            if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
            if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
            
            _speed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : speed;

            transform.position += moveDir * (_speed * Time.deltaTime);
        }
    }
}