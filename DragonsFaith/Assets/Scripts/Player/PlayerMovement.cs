using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float speed = 3f;
        [SerializeField] private float fastSpeed = 6f;
        [SerializeField] private Animator animator;

        private float _speed;
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int X = Animator.StringToHash("x");
        private float _previousDir;

        private void Awake()
        {
            _speed = speed;
            //_animator = GetComponent<Animator>();
        }

        public void ForcePosition(Vector3 position)
        {
            if (!IsOwner) return;

            transform.position = position;
        }

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
            //TODO: interrupt animation when entering combat
            if (moveDir != Vector3.zero)
            {
                animator.SetBool(IsRunning, Math.Abs(_speed - fastSpeed) < 0.1f);
                animator.SetBool(IsMoving, true);
            }
            else
            {
                animator.SetBool(IsMoving, false);
                animator.SetBool(IsRunning, false);
            }
            if (_previousDir != 0) animator.SetFloat(X, _previousDir);
            _previousDir = moveDir.x;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsLocalPlayer)
            {
                gameObject.name = IsHost ? "Player_Host" : "Player_Client";
            }
            else
            {
                gameObject.name = IsHost ? "Player_Client" : "Player_Host" ;
            }
        }

        public void InterruptAnimations()
        {
            animator.SetBool(IsMoving, false);
            animator.SetBool(IsRunning, false);
        }
    }
}