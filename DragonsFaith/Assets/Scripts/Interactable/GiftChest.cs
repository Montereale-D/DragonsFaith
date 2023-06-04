using Inventory;
using Inventory.Items;
using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    public class GiftChest : KeyInteractable
    {
        [SerializeField] private string saveId;
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
            
            if (DungeonProgressManager.instance.IsChestOpened(saveId))
            {
                Debug.Log(gameObject.name + " was already activated");
                _isUsed.Value = true;
            }
            else
            {
                //Debug.Log(gameObject.name + " OnNetworkSpawn");
            }
        }

        public void TryAddItem(Collider2D col)
        {
            if (_isUsed.Value) return;
            
            //try to add item to the inventory
            if (!InventoryManager.Instance.AddItem(item))
            {
                Debug.Log("No space in the inventory!");
                return;
            }

            Debug.Log("Item  picked up");
            DungeonProgressManager.instance.ChestOpened(saveId);

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
            //Debug.Log("GiftChest onDestroy " + name);
            base.OnDestroy();
            
        }
        
        [ContextMenu("Generate guid")]
        private void GenerateGuid()
        {
            saveId = System.Guid.NewGuid().ToString();
        }
    }
}