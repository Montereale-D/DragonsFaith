using Inventory;
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
    }
}