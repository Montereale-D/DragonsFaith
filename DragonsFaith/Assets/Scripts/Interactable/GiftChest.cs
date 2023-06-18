using Inventory;
using Inventory.Items;
using UI;
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

            if (DungeonProgressManager.instance.IsChestOpened(saveId, gameObject))
            {
                Debug.Log(gameObject.name + " was already activated");
                /*if (IsHost)
                {
                    _isUsed.Value = true;
                }*/

                SetActive(true);
            }
            else
            {
                Debug.Log(gameObject.name + " OnNetworkSpawn");
            }
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
            AudioManager.instance.PlayOpenChestSound();
            DungeonProgressManager.instance.ChestOpened(saveId, gameObject);
            Notify();

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

        private void Notify()
        {
            if (IsHost)
            {
                ChestOpenedClientRpc();
            }
            else
            {
                ChestOpenedServerRpc();
            }
        }

        [ClientRpc]
        private void ChestOpenedClientRpc()
        {
            if(IsHost) return;
            
            AudioManager.instance.PlayOpenChestSound();
            DungeonProgressManager.instance.ChestOpened(saveId, gameObject);
        }
        
        [ServerRpc (RequireOwnership = false)]
        private void ChestOpenedServerRpc()
        {
            if(!IsHost) return;
            
            AudioManager.instance.PlayOpenChestSound();
            DungeonProgressManager.instance.ChestOpened(saveId, gameObject);
        }

        public override void OnDestroy()
        {
            Debug.Log("GiftChest onDestroy " + name);
            base.OnDestroy();
        }

        [ContextMenu("Generate guid")]
        private void GenerateGuid()
        {
            saveId = System.Guid.NewGuid().ToString();
        }
    }
}