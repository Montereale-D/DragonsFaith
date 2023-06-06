using Inventory;
using Inventory.Items;
using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    public class GiftChest : KeyInteractable
    {
        [SerializeField] [Tooltip("Item inside the chest")]
        private Item item;

        protected override void Awake()
        {
            onKeyPressedEvent = TryAddItem;
            base.Awake();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            GetComponent<NetworkObject>().DestroyWithScene = true;
            
        }

        public void TryAddItem(Collider2D col)
        {
            if (_isUsed.Value) return;

            var loot = ExchangeManager.Instance.GetRandomItem();
            
            //try to add item to the inventory
            if (!InventoryManager.Instance.AddItem(loot))
            {
                Debug.Log("No space in the inventory!");
                return;
            }

            Debug.Log("Item  picked up");

            if (IsHost)
            {
                //direct change
                _isUsed.Value = true;
            }
            else
            {
                //ask host to change
                UpdateStatusServerRpc(true);
            }
        }

        public override void OnDestroy()
        {
            Debug.Log("GiftChest onDestroy " + name);
            base.OnDestroy();
            
        }
    }
}