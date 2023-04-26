using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory
{
    /// <summary>
    /// Represent a single slot in an inventory
    /// </summary>
    public class InventorySlot : MonoBehaviour, IDropHandler
    {
        public Image image;
        public Color onSelectColor, onDeselectColor;
        public InventoryManager inventoryManager;

        private void Awake()
        {
            OnDeselect();
        }

        public void OnDrop(PointerEventData eventData)
        {
            //if an item is dropped here and it is empty, set this slot as parent of item
            if (transform.childCount == 0)
            {
                var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
                inventoryItem.UpdateParent(transform);
            }
        }

        public void OnSelect()
        {
            image.color = onSelectColor;
        }

        public void OnDeselect()
        {
            image.color = onDeselectColor;
        }

        /// <summary>
        /// Notify this slot that the item has been clicked
        /// </summary>
        public void OnItemClick(InventoryItem inventoryItem)
        {
            inventoryManager.OnItemSelected(this, inventoryItem);
        }

        /// <summary>
        /// Notify this slot that the item has been used
        /// </summary>
        public void OnItemUse(InventoryItem inventoryItem)
        {
            inventoryManager.OnItemUse(this, inventoryItem);
        }
    }
}