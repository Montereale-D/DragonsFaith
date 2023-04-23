using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory
{
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
            if (transform.childCount == 0)
            {
                var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
                inventoryItem.parentAfterDrag = transform;
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

        public void OnItemClick(InventoryItem inventoryItem)
        {
            Debug.Log("Click on " + inventoryItem.item.itemName);
            //tshow info
            inventoryManager.OnSelectedChange(this);
        }

        public void OnItemUse(InventoryItem inventoryItem)
        {
            Debug.Log("Double click on " + inventoryItem.item.itemName);
            inventoryManager.OnItemUse(this, inventoryItem);
        }
    }
}