using Save;
using Unity.Netcode;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Represent the inventory. It manage slot, items and network synchronization.
    /// </summary>
    public class InventoryManager : NetworkBehaviour, IGameData
    {
        [SerializeField] [Tooltip("Insert (in order) all the slots, toolbar, inventory, ...")]
        private InventorySlot[] inventorySlots;

        [SerializeField] [Tooltip("Insert reference to the inventory item prefab")]
        private GameObject inventoryItemPrefab;

        [SerializeField] [Tooltip("Max number of item in a stack")]
        private int maxStackable = 5;

        private InventorySlot _prevSelectedSlot;

        public static InventoryManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        /// <summary>
        /// Inventory request to add an item
        /// </summary>
        public bool AddItem(Item newItem)
        {
            //if there is a not empty stack, add this item in the stack
            foreach (var slot in inventorySlots)
            {
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot != null && itemInSlot.item == newItem && itemInSlot.item.stackable &&
                    itemInSlot.count < maxStackable)
                {
                    itemInSlot.count++;
                    itemInSlot.UpdateCount();
                    return true;
                }
            }

            //if there is an empty slot, add this item in the slot
            foreach (var slot in inventorySlots)
            {
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot == null)
                {
                    SpawnNewItem(newItem, slot, 1);
                    return true;
                }
            }

            //no slot available
            return false;
        }

        /// <summary>
        /// Inventory request to add an item using a name or ID
        /// </summary>
        public void AddItem(string newItemIdOrName)
        {
            //return commented in order to test it with a button
            AddItem(ItemSyllabus.Instance.SearchItem(newItemIdOrName));
        }

        /// <summary>
        /// Instantiate an item and add it to the slot
        /// </summary>
        private void SpawnNewItem(Item item, InventorySlot slot, int quantity)
        {
            var newItemGameObject = Instantiate(inventoryItemPrefab, slot.transform);
            var inventoryItem = newItemGameObject.GetComponent<InventoryItem>();
            inventoryItem.SetItem(item, quantity);
        }

        /// <summary>
        /// Use this item
        /// </summary>
        public void OnItemUse(InventorySlot slot, InventoryItem item)
        {
            Debug.Log("Use item " + item);
            if (item.item.consumable)
            {
                item.count--;
                if (item.count <= 0) Destroy(item.gameObject);
                else item.UpdateCount();
            }
            else
            {
                //active item
            }
        }

        /// <summary>
        /// This item has been selected
        /// </summary>
        public void OnItemSelected(InventorySlot inventorySlot, InventoryItem item)
        {
            Debug.Log("Select item " + item);
            if (_prevSelectedSlot != null)
            {
                _prevSelectedSlot.OnDeselect();
            }

            inventorySlot.OnSelect();
            _prevSelectedSlot = inventorySlot;
        }

        /// <summary>
        /// Load inventory item from local files
        /// </summary>
        public void LoadData(GameData data)
        {
            // clean current inventory
            CleanupInventory();

            //get item from local data
            foreach (var itemData in data.GetAllItemsData(GetPlayerType()))
            {
                SpawnNewItem(ItemSyllabus.Instance.SearchItem(itemData.itemId), inventorySlots[itemData.slotNumber],
                    itemData.quantity);
            }
        }

        /// <summary>
        /// Save inventory item in the local files
        /// </summary>
        public void SaveData(ref GameData data)
        {
            //clean the previous local data
            data.CleanupItemData(GetPlayerType());

            //add item to local data
            for (var i = 0; i < inventorySlots.Length; i++)
            {
                var slot = inventorySlots[i];
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot != null)
                {
                    //Debug.Log("AddItemData");
                    data.AddItemData(GetPlayerType(), itemInSlot.item, i, itemInSlot.count);
                }
            }
        }

        /// <summary>
        /// Destroy items game objects
        /// </summary>
        private void CleanupInventory()
        {
            foreach (var slot in inventorySlots)
            {
                var inventoryItem = slot.GetComponentInChildren<InventoryItem>();
                if (inventoryItem == null) continue;
                Destroy(inventoryItem.gameObject);
            }
        }

        private GameData.PlayerType GetPlayerType()
        {
            return IsHost ? GameData.PlayerType.Host : GameData.PlayerType.Client;
        }
    }
}