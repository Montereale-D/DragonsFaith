using UnityEngine;
using System.Linq;
using Inventory.Items;
using Unity.Netcode;

namespace Inventory
{
    /// <summary>
    /// Utility class used as a catalogue for items
    /// </summary>
    public class ExchangeManager : NetworkBehaviour
    {
        
         [SerializeField] private Item[] itemList;

        public static ExchangeManager Instance { get; private set; }
        private void Awake() 
        { 
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
                DontDestroyOnLoad(this);
            } 
        }
        
        /// <summary>
        /// Get an item
        /// </summary>
        public Item CreateItem(string idOrName)
        {
            var item = itemList.First(item => (item.id == idOrName) || (item.name == idOrName));
            if (item == null) Debug.LogError("Invalid name or id");
            return item;
        }

        public void GetItemFromMerchant()
        {
            
        }

        public void SendItemToFriend(string itemToSendID)
        {
            Debug.Log("SendItemToFriend");
            if (IsHost)
            {
                Debug.Log("Host");
                SendItemClientRpc(itemToSendID);
            } else
            {
                Debug.Log("Client");
                SendItemServerRpc(itemToSendID);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendItemServerRpc(string receivedItemID)
        {
            if(!IsHost) return;

            Debug.Log("SendItemServerRpc");
            var isSpaceAvailable = InventoryManager.Instance.AddItem(CreateItem(receivedItemID));
            SendItemResponseClientRpc(isSpaceAvailable);
        }

        [ClientRpc]
        private void SendItemClientRpc(string receivedItemID)
        {
            if(IsHost) return;
            
            Debug.Log("SendItemClientRpc");
            var isSpaceAvailable = InventoryManager.Instance.AddItem(CreateItem(receivedItemID));
            SendItemResponseServerRpc(isSpaceAvailable);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendItemResponseServerRpc(bool isSpaceAvailable)
        {
            if(!IsHost) return;

            Debug.Log("[HOST] Can remove item");
            InventoryManager.Instance.OnItemSendResponse(isSpaceAvailable);
        }

        [ClientRpc]
        private void SendItemResponseClientRpc(bool isSpaceAvailable)
        {
            if(IsHost) return;

            Debug.Log("[CLIENT] Can remove item");
            InventoryManager.Instance.OnItemSendResponse(isSpaceAvailable);
        }
    }
}
