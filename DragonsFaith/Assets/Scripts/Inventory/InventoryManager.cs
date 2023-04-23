using Save;
using UnityEngine;
using UnityEngine.Serialization;

namespace Inventory
{
    public class InventoryManager : MonoBehaviour, IGameData
    {
        public InventorySlot[] inventorySlots;
        [FormerlySerializedAs("itemGenerator")] public ItemSyllabus itemSyllabus;
        public GameObject itemGameObject;
        public int maxStackable = 5;

        private InventorySlot _prevSelectedSlot;

        public bool AddItem(Item newItem)
        {
            foreach (var slot in inventorySlots)
            {
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot == null)
                {
                    SpawnNewItem(newItem, slot, 1);
                    return true;
                }

                if ( /*itemInSlot != null && */itemInSlot.item == newItem && itemInSlot.item.stackable &&
                                               itemInSlot.count < maxStackable)
                {
                    itemInSlot.count++;
                    itemInSlot.UpdateCount();
                    return true;
                }
            }

            return false;
        }

        public void AddItem(string newItemIdOrName)
        {
            /*return*/
            AddItem(itemSyllabus.SearchItem(newItemIdOrName));
        }

        private void SpawnNewItem(Item item, InventorySlot slot, int quantity)
        {
            var newItemGameObject = Instantiate(itemGameObject, slot.transform);
            var inventoryItem = newItemGameObject.GetComponent<InventoryItem>();
            inventoryItem.SetItem(item, quantity);
        }

        public void OnItemUse(InventorySlot slot, InventoryItem item)
        {
            Debug.Log("Use item");
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

        public void OnSelectedChange(InventorySlot inventorySlot)
        {
            Debug.Log("Select item");
            if (_prevSelectedSlot != null)
            {
                _prevSelectedSlot.OnDeselect();
            }

            inventorySlot.OnSelect();
            _prevSelectedSlot = inventorySlot;
        }

        public void LoadData(GameData data)
        {
            CleanupInventory();
            foreach (var itemData in data.GetAllItemsData())
            {
                SpawnNewItem(itemSyllabus.SearchItem(itemData.itemId), inventorySlots[itemData.slotNumber], itemData.quantity);
            }
        }

        public void SaveData(ref GameData data)
        {
            data.CleanupItemData();
            for (var i = 0; i < inventorySlots.Length; i++)
            {
                var slot = inventorySlots[i];
                var itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot != null)
                {
                    Debug.Log("AddItemData");
                    data.AddItemData(itemInSlot.item, i, itemInSlot.count);
                }
            }
        }
        
        private void CleanupInventory()
        {
            foreach (var slot in inventorySlots)
            {
                var inventoryItem = slot.GetComponentInChildren<InventoryItem>();
                if (inventoryItem == null) continue;
                Destroy(inventoryItem.gameObject);
            }
        }
    }
}