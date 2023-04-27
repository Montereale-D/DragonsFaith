using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
        public InventoryManager inventoryManager;

        public ItemType slotType = ItemType.Items;
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
            inventoryManager.OnSlotSelected(this, inventoryItem);
        }

        /// <summary>
        /// Notify this slot that the item has been used
        /// </summary>
        public virtual void OnItemUse(InventoryItem item)
        {
            Debug.Log("Use item " + item);
            if (item.item.consumable)
            {
                item.count--;
                if (item.count <= 0)
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
            else
            {
                //active item
            }
            
            inventoryManager.OnSlotUse(this, item);
        }
    }
}