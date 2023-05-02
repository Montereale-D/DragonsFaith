using Save;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Inventory
{
    /// <summary>
    /// Represent the inventory. It manage slot, items and network synchronization.
    /// </summary>
    public class InventoryManager : NetworkBehaviour, IGameData
    {
        [SerializeField] [Tooltip("Insert (in order) all the inventory slots, ...")]
        private InventorySlot[] inventorySlots;
        
        [SerializeField] [Tooltip("Insert (in order) all the equipment slots, ...")]
        private InventorySlot[] equipmentSlots;

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
            
            foreach (var slot in equipmentSlots)
            {
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot != null && itemInSlot.item == newItem && itemInSlot.item.stackable &&
                    itemInSlot.count < maxStackable && itemInSlot.item.type == ItemType.Items)
                {
                    itemInSlot.count++;
                    itemInSlot.UpdateCount();
                    slot.onSlotUpdate.Invoke(itemInSlot);
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
            slot.onSlotUpdate.Invoke(inventoryItem);
        }

        //tmp
        public UnityEvent<InventoryItem> onSlotUseEvent;
        /// <summary>
        /// Use this slot
        /// </summary>
        public void OnSlotUse(InventorySlot slot, InventoryItem item)
        {
            if (ItemSyllabus.Instance.SearchItem("190cd2eb-04ba-42df-af91-dbb48316af90"))
            {
                onSlotUseEvent.Invoke(item);
            }
        }

        /// <summary>
        /// This slot has been selected
        /// </summary>
        public void OnSlotSelected(InventorySlot inventorySlot, InventoryItem item)
        {
            Debug.Log("Select item " + item);
            if (_prevSelectedSlot != null)
            {
                _prevSelectedSlot.OnDeselect();
            }

            inventorySlot.OnSelect();
            _prevSelectedSlot = inventorySlot;
        }

        public void BlockEquipmentSlots(bool b)
        {
            foreach (var slot in equipmentSlots)
            {
                if (slot.slotType is ItemType.Armory or ItemType.Skills or ItemType.Weapons)
                {
                    slot.blockDrag = b;
                }
            }
        }

        [ContextMenu("Lock Equipment")]
        private void LockEquipmentFromMenu()
        {
            BlockEquipmentSlots(true);
        }
        
        [ContextMenu("Unlock Equipment")]
        private void UnlockEquipmentFromMenu()
        {
            BlockEquipmentSlots(false);
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
                SpawnNewItem(ItemSyllabus.Instance.SearchItem(itemData.itemId),
                    itemData.inventoryType == GameData.InventoryType.Inventory
                        ? inventorySlots[itemData.slotNumber]
                        : equipmentSlots[itemData.slotNumber],
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
                    data.AddItemData(GetPlayerType(), itemInSlot.item, GameData.InventoryType.Inventory, i, itemInSlot.count);
                }
            }
            
            for (var i = 0; i < equipmentSlots.Length; i++)
            {
                var slot = equipmentSlots[i];
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot != null)
                {
                    data.AddItemData(GetPlayerType(), itemInSlot.item, GameData.InventoryType.Equipment, i, itemInSlot.count);
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
            
            foreach (var slot in equipmentSlots)
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