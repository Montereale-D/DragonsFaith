using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interactable
{
    [RequireComponent(typeof(Collider2D))]
    public class KeyInteractable : NetworkBehaviour
    {
        [FormerlySerializedAs("openSprite")] [SerializeField] protected Sprite usedSprite;
        [FormerlySerializedAs("closeSprite")] [SerializeField] protected Sprite unusedSprite;
        protected SpriteRenderer _spriteRenderer;
        protected Collider2D _collider;
        protected ShowKeyOnTrigger _showKey;
    
        protected ShowKeyOnTrigger.OnKeyPressed onKeyPressedEvent;

        //synchronize state between host and client
        protected readonly NetworkVariable<bool> _isUsed = new(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
    
        protected virtual void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();

            //ensure isTrigger
            _collider.isTrigger = true;

            _showKey = GetComponentInChildren<ShowKeyOnTrigger>();
            _showKey.KeyPressed += onKeyPressedEvent;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            //subscribe to status change event
            _isUsed.OnValueChanged += SetActive;
        }

        private void SetActive(bool oldValue, bool newValue)
        {
            SetActive(newValue);
        }
        protected void SetActive(bool newValue)
        {
            _spriteRenderer.sprite = newValue ? usedSprite : unusedSprite;

            //disable collider, no more needed
            _collider.enabled = !newValue;
                
            //turn off/on key UI
            if(!newValue) _showKey.TurnOn();
            else _showKey.TurnOff();
        }
    
        //Server receive a change status request -> client use the obj
        [ServerRpc(RequireOwnership = false)]
        protected void UpdateStatusServerRpc(bool b)
        {
            if (!IsHost) return;

            if (_isUsed.Value != b)
            {
                _isUsed.Value = b;
            }
        }
    }
}
