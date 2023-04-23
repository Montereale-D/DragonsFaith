using System;
using System.Collections.Generic;
using Inventory;
using UnityEngine;

namespace Save
{
    [Serializable]
    public class GameData
    {
        [Serializable]
        public struct ItemData
        {
            public string itemId;
            public int slotNumber;
            public int quantity;
            
            public ItemData(string itemId, int slotNumber, int quantity)
            {
                this.itemId = itemId;
                this.slotNumber = slotNumber;
                this.quantity = quantity;
            }

            public override string ToString()
            {
                return "Item: " + itemId + " x" + quantity;
            }
        }
        
        [SerializeField] private List<ItemData> itemDataList;

        public GameData()
        {
            itemDataList = new List<ItemData>();
        }

        public void AddItemData(Item item, int slotNumber, int quantity)
        {
            itemDataList.Add(new ItemData(item.id, slotNumber, quantity));
        }

        public void CleanupItemData()
        {
            itemDataList = new List<ItemData>();
        }

        public List<ItemData> GetAllItemsData()
        {
            return itemDataList;
        }
    }
}