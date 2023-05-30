using System;
using Inventory.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory
{
    /// <summary>
    /// Represent a single slot in an inventory
    /// </summary>
    public class InventorySlot : MonoBehaviour, IDropHandler
    {
        public bool blockDrag;
        public Image image;
        public Color onSelectColor, onDeselectColor;

        public ItemType slotType = ItemType.All;
        /*public delegate void SlotUpdate();
        public SlotUpdate onSlotUpdate;*/

        [Serializable]
        public class SlotUpdateEvent : UnityEvent<InventoryItem>
        {
        }

        [HideInInspector] public SlotUpdateEvent onSlotUpdate;
        [HideInInspector] public SlotUpdateEvent onSlotRemoved;

        private void Awake()
        {
            OnDeselect();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (blockDrag) return;

            //if an item is dropped here and it is empty, set this slot as parent of item
            if (transform.childCount != 0) return;
            var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();

            Debug.Log("OnDrop: slot type " + slotType + " item type " + inventoryItem.item.type);
            if (slotType == ItemType.All || inventoryItem.item.type == slotType)
            {
                inventoryItem.UpdateParent(transform);
                onSlotUpdate.Invoke(inventoryItem);
            }
        }

        public void OnSelect()
        {
            if (image)
            {
                image.color = onSelectColor;
            }
        }

        public void OnDeselect()
        {
            if (image)
            {
                image.color = onDeselectColor;
            }
        }

        /// <summary>
        /// Notify this slot that the item has been clicked
        /// </summary>
        public void OnItemClick(InventoryItem inventoryItem)
        {
            InventoryManager.Instance.OnSlotSelected(this, inventoryItem);
        }

        /// <summary>
        /// Notify this slot that the item has been used
        /// </summary>
        public virtual void OnItemUse(InventoryItem inventoryItem)
        {
            Debug.Log("Use item " + inventoryItem);
            if (inventoryItem.item.consumable)
            {
                UpdateItemQuantity(inventoryItem);
            }
            else
            {
                // activate item
            }
            
            InventoryManager.Instance.OnSlotUse(this, inventoryItem);
        }

        public void OnItemSend(InventoryItem inventoryItem)
        {
            if (this is ToolbarSlot)
            {
                Debug.Log("We don't do that here");
                return;
            }

            if (slotType == ItemType.Skill)
            {
                Debug.Log("You can not send a skill");
                return;
            }
            
            Debug.Log("Passed checks in InventorySlot");
            InventoryManager.Instance.OnItemSend(this, inventoryItem);
        }
        
        public void OnItemSendResponse(InventoryItem inventoryItem)
        {
            UpdateItemQuantity(inventoryItem);
        }
        
        private void UpdateItemQuantity(InventoryItem item, int i = -1)
        {
            if(i == 0) throw new Exception("Make no sense");
            
            item.count += i;
            
            if (item.count < 0) throw new Exception("Negative quantity");
            
            if (item.count == 0)
            {
                Destroy(item.gameObject);
                onSlotRemoved.Invoke(item);
            }
            else
            {
                item.UpdateCount();
                onSlotUpdate.Invoke(item);
            }
        }

        public void OnItemDestroy(InventoryItem inventoryItem)
        {
            UpdateItemQuantity(inventoryItem);
        }

        public void OnItemDestroyAll(InventoryItem inventoryItem)
        {
            UpdateItemQuantity(inventoryItem, -inventoryItem.count);
        }
    }
}