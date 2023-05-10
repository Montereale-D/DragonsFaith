using System;
using System.Collections.Generic;
using Player;
using Save;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory
{
    /// <summary>
    /// Represent the inventory. It manage slot, items and network synchronization.
    /// </summary>
    public class InventoryManager : MonoBehaviour, IGameData
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
                DontDestroyOnLoad(this);
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

                if (itemInSlot == null && (newItem.type == slot.slotType || slot.slotType == ItemType.All))
                {
                    SpawnNewItem(newItem, slot, 1);
                    return true;
                }
            }
            
            foreach (var slot in equipmentSlots)
            {
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot == null && newItem.type == slot.slotType)
                {
                    SpawnNewItem(newItem, slot, 1);
                    slot.onSlotUpdate.Invoke(itemInSlot);
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
                if (slot.slotType is ItemType.Head or ItemType.Chest or ItemType.Legs or ItemType.Skills or ItemType.Weapons)
                {
                    slot.blockDrag = b;
                }
            }
        }

        public Item GetEquipmentItem(ItemType type)
        {
            if (type != ItemType.Chest && type != ItemType.Head && type != ItemType.Legs && type != ItemType.Weapons)
            {
                throw new Exception("Not equipment request");
            }
            
            foreach (var slot in equipmentSlots)
            {
                if(slot.slotType == type)
                    return slot.GetComponentInChildren<InventoryItem>().item;
            }

            return null;
        }

        public float GetEquipmentModifiers(AttributeType type)
        {
            var output = 0f;
            
            foreach (var slot in equipmentSlots)
            {
                var inventoryItem = slot.GetComponentInChildren<InventoryItem>();
                if(inventoryItem == null) continue;

                var item = inventoryItem.item;
                output += GetModifiers(item, type);
            }
            
            return output < 1 ? 1 : output;
        }

        public static float GetModifiers(Item item, AttributeType type)
        {
            return type switch
            {
                AttributeType.Strength => item.xStr,
                AttributeType.Intelligence => item.xInt,
                AttributeType.Agility => item.xAgi,
                AttributeType.Constitution => item.xConst,
                AttributeType.Dexterity => item.xDex,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
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
            foreach (var itemData in data.GetAllItemsData(GameData.GetPlayerType()))
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
            data.CleanupItemData(GameData.GetPlayerType());

            //add item to local data
            for (var i = 0; i < inventorySlots.Length; i++)
            {
                var slot = inventorySlots[i];
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot != null)
                {
                    data.AddItemData(GameData.GetPlayerType(), itemInSlot.item, GameData.InventoryType.Inventory, i, itemInSlot.count);
                }
            }
            
            for (var i = 0; i < equipmentSlots.Length; i++)
            {
                var slot = equipmentSlots[i];
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot != null)
                {
                    data.AddItemData(GameData.GetPlayerType(), itemInSlot.item, GameData.InventoryType.Equipment, i, itemInSlot.count);
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
                slot.onSlotRemoved.Invoke(inventoryItem);
            }
            
            foreach (var slot in equipmentSlots)
            {
                var inventoryItem = slot.GetComponentInChildren<InventoryItem>();
                if (inventoryItem == null) continue;
                Destroy(inventoryItem.gameObject);
                slot.onSlotRemoved.Invoke(inventoryItem);
            }
        }

        public void SetUpSlots(InventorySlot[] inventorySlots1, InventorySlot[] equipmentSlots1)
        {
            inventorySlots = inventorySlots1;
            equipmentSlots = equipmentSlots1;
        }

        [ContextMenu("Add Potion")]
        public void AddPotionContextMenu()
        {
            var potion = ItemSyllabus.Instance.SearchItem("190cd2eb-04ba-42df-af91-dbb48316af90");
            Debug.Log("AddItem request " + AddItem(potion));
        }
        
        [ContextMenu("Add Armor")]
        public void AddArmorContextMenu()
        {
            var armor = ItemSyllabus.Instance.SearchItem("5bcae7d9-cda4-4a30-a251-bdbb84552015");
            Debug.Log("AddItem request " + AddItem(armor));
        }
    }
}