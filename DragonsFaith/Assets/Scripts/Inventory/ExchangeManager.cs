using UnityEngine;
using System.Linq;
using Inventory.Items;
using Unity.Netcode;
using Random = UnityEngine.Random;

namespace Inventory
{
    /// <summary>
    /// Utility class used as a catalogue for items
    /// </summary>
    public class ExchangeManager : NetworkBehaviour
    {
        //TODO: add throwable weapons when they are implemented
        [SerializeField] private Item[] itemList;
        [SerializeField] private Item[] skillList;

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

        public Item GetRandomItem()
        {
            return itemList[Random.Range(0, itemList.Length)];
        }
        
        public Item CreateSkill(string idOrName)
        {
            var item = skillList.First(item => (item.id == idOrName) || (item.name == idOrName));
            if (item == null) Debug.LogError("Invalid name or id");
            return item;
        }

        public void GetItemFromMerchant()
        {
            
        }

        public void SendItemToFriend(string itemToSendID)
        {
            if (IsHost)
            {
                Debug.Log("Host sending item to client");
                SendItemClientRpc(itemToSendID);
            }
            else
            {
                Debug.Log("Client sending item to host");
                SendItemServerRpc(itemToSendID);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendItemServerRpc(string receivedItemID)
        {
            if(!IsHost) return;

            var isSpaceAvailable = InventoryManager.Instance.AddItem(CreateItem(receivedItemID));
            
            Debug.Log(isSpaceAvailable ? "Item accepted, replying ..." : "Item refused, replying ...");

            SendItemResponseClientRpc(isSpaceAvailable);
        }

        [ClientRpc]
        private void SendItemClientRpc(string receivedItemID)
        {
            if(IsHost) return;
            
            var isSpaceAvailable = InventoryManager.Instance.AddItem(CreateItem(receivedItemID));
            
            Debug.Log(isSpaceAvailable ? "Item accepted, replying ..." : "Item refused, replying ...");
            
            SendItemResponseServerRpc(isSpaceAvailable);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendItemResponseServerRpc(bool isSpaceAvailable)
        {
            if(!IsHost) return;

            Debug.Log("[HOST] Can remove item");
            Debug.Log(isSpaceAvailable ? "Item accepted, thanks client!" : "Item refused, Item accepted, sorry client!");
            
            InventoryManager.Instance.OnItemSendResponse(isSpaceAvailable);
        }

        [ClientRpc]
        private void SendItemResponseClientRpc(bool isSpaceAvailable)
        {
            if(IsHost) return;

            Debug.Log("[CLIENT] Can remove item");
            Debug.Log(isSpaceAvailable ? "Item accepted, thanks host!" : "Item refused, Item accepted, sorry host!");

            InventoryManager.Instance.OnItemSendResponse(isSpaceAvailable);
        }
    }
}
