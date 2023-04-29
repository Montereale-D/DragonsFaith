using Inventory;
using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    [RequireComponent(typeof(Collider2D))]
    public class GiftChest : NetworkBehaviour
    {
        [SerializeField] [Tooltip("Item inside the chest")]
        private Item item;

        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closeSprite;
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        private ShowKeyOnTrigger _showKey;

        //synchronize state between host and client
        private readonly NetworkVariable<bool> _isOpen = new(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();

            //ensure isTrigger
            _collider.isTrigger = true;

            _showKey = GetComponentInChildren<ShowKeyOnTrigger>();
            _showKey.KeyPressed += TryAddItem;
        }

        public override void OnNetworkSpawn()
        {
            //subscribe to status change event
            _isOpen.OnValueChanged += (_, newValue) =>
            {
                _spriteRenderer.sprite = newValue ? openSprite : closeSprite;

                //disable collider, no more needed
                _collider.enabled = !newValue;
                
                if(!newValue) _showKey.TurnOn();
                else _showKey.TurnOff();
            };
        }

        public void TryAddItem(Collider2D col)
        {
            //check if the player is mine
            //if (!col.gameObject.GetComponent<NetworkObject>().IsOwner) return;

            if (!_isOpen.Value)
            {
                //try to add item to the inventory
                if (!InventoryManager.Instance.AddItem(item))
                {
                    Debug.Log("No space in the inventory!");
                    return;
                }

                Debug.Log("Get item");

                if (IsHost)
                {
                    //direct change
                    _isOpen.Value = true;
                }
                else
                {
                    //ask host to change
                    UpdateChestStatusServerRpc(true);
                }
            }
        }

        //Server receive a change status request -> client get the item
        [ServerRpc(RequireOwnership = false)]
        private void UpdateChestStatusServerRpc(bool b)
        {
            if (!IsHost) return;

            if (_isOpen.Value != b)
            {
                _isOpen.Value = b;
            }
        }
    }
}