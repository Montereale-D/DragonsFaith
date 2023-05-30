using System;
using System.Linq;
using Inventory.Items;
using Player;
using Save;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Represent the inventory. It manage slot, items and network synchronization.
    /// </summary>
    public class InventoryManager : MonoBehaviour, IGameData
    {
        [SerializeField] [Tooltip("Insert (in order) all the inventory slots, ...")] [HideInInspector]
        private InventorySlot[] inventorySlots;

        [SerializeField] [Tooltip("Insert (in order) all the equipment slots, ...")] [HideInInspector]
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
                    itemInSlot.count < maxStackable && itemInSlot.item.type == ItemType.Consumable)
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

        /*/// <summary>
        /// Inventory request to add an item using a name or ID
        /// </summary>
        public void AddItem(string newItemIdOrName)
        {
            //return commented in order to test it with a button
            AddItem(ExchangeManager.Instance.CreateItem(newItemIdOrName));
        }*/

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

        /// <summary>
        /// Use this slot
        /// </summary>
        public void OnSlotUse(InventorySlot slot, InventoryItem item)
        {
            switch (item.item.type)
            {
                case ItemType.Consumable:
                    OnConsumableUse(item);
                    break;
                case ItemType.Weapon:
                    //TODO add weapon function
                    break;
                case ItemType.Head:
                    break;
                case ItemType.Legs:
                    break;
                case ItemType.Skill:
                    break;
            }
        }

        private void OnConsumableUse(InventoryItem item)
        {
            var consumable = item.item as Consumable;
            if (consumable == null) throw new Exception("Not valid casting");
            switch (consumable.consumableType)
            {
                case Consumable.ConsumableType.PotionHealing:
                    CharacterManager.Instance.Heal(20);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private InventorySlot _sendSlot;
        private InventoryItem _sendItem;

        public void OnItemSend(InventorySlot slot, InventoryItem item)
        {
            if (item == null) return;
            
            Debug.Log("Passed checks in InventoryManager");
            _sendSlot = slot;
            _sendItem = item;
            Debug.Log("(" + slot + ", " + item + ")");
            ExchangeManager.Instance.SendItemToFriend(item.item.id);
        }

        public void OnItemSendResponse(bool isSuccess)
        {
            if (isSuccess)
            {
                _sendSlot.OnItemSendResponse(_sendItem);
                _sendSlot = null;
                _sendItem = null;
            }
            else
            {
                //do nothing
                _sendSlot = null;
                _sendItem = null;
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
                if (slot.slotType is ItemType.Consumable) continue;
                
                slot.blockDrag = b;
            }
        }

        public Item GetEquipmentItem(ItemType type)
        {
            if (type is ItemType.Consumable or ItemType.Skill)
            {
                throw new Exception("Not equipment request");
            }

            foreach (var slot in equipmentSlots)
            {
                if (slot.slotType == type)
                    return slot.GetComponentInChildren<InventoryItem>().item;
            }

            return null;
        }

        public Weapon GetWeapon()
        {
            return (from slot in equipmentSlots select slot.GetComponentInChildren<InventoryItem>() into inventoryItem 
                where inventoryItem select inventoryItem.item as Weapon).FirstOrDefault(weapon => weapon);
        }

        public float GetEquipmentModifiers(AttributeType type)
        {
            var output = 1f;

            foreach (var slot in equipmentSlots)
            {
                var inventoryItem = slot.GetComponentInChildren<InventoryItem>();
                if (!inventoryItem) continue;
                
                var armor = inventoryItem.item as Armor;
                if (armor)
                {
                    output += GetModifiers(armor, type);
                }
            }
            
            return output < 1 ? 1 : output;
        }

        public static float GetModifiers(Armor item, AttributeType type)
        {
            return type switch
            {
                AttributeType.Strength => item.Str,
                AttributeType.Intelligence => item.Int,
                AttributeType.Agility => item.Agi,
                AttributeType.Constitution => item.Const,
                AttributeType.Dexterity => item.Dex,
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
                SpawnNewItem(ExchangeManager.Instance.CreateItem(itemData.itemId),
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
                    data.AddItemData(GameData.GetPlayerType(), itemInSlot.item, GameData.InventoryType.Inventory, i,
                        itemInSlot.count);
                }
            }

            for (var i = 0; i < equipmentSlots.Length; i++)
            {
                var slot = equipmentSlots[i];
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot != null)
                {
                    data.AddItemData(GameData.GetPlayerType(), itemInSlot.item, GameData.InventoryType.Equipment, i,
                        itemInSlot.count);
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

        [ContextMenu("Add Health Kit")]
        public void AddHealthKitContextMenu()
        {
            var potion = ExchangeManager.Instance.CreateItem("190cd2eb-04ba-42df-af91-dbb48316af90");
            Debug.Log("AddItem request " + AddItem(potion));
        }

        [ContextMenu("Add Health Kit Full Inventory")]
        public void AddHealthKitFullInventoryContextMenu()
        {
            var potion = ExchangeManager.Instance.CreateItem("190cd2eb-04ba-42df-af91-dbb48316af90");
            while (AddItem(potion))
            {
            }
        }

        [ContextMenu("Add Armor")]
        public void AddArmorContextMenu()
        {
            var armor = ExchangeManager.Instance.CreateItem("777225e7-ea15-4ea2-bb10-40a5d2dbac4a");
            Debug.Log("AddItem request " + AddItem(armor));
        }
        
        [ContextMenu("Add Weapon")]
        public void AddWeaponContextMenu()
        {
            var armor = ExchangeManager.Instance.CreateItem("3785c742-f8ff-4016-a374-81d62dc75746");
            Debug.Log("AddItem request " + AddItem(armor));
        }
        
        [ContextMenu("Add Sniper")]
        public void AddSniperContextMenu()
        {
            var armor = ExchangeManager.Instance.CreateItem("73cc3261-00ae-4a96-bc79-fe4f9d1ff07c");
            Debug.Log("AddItem request " + AddItem(armor));
        }
    }
}