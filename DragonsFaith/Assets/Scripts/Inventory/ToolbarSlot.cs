using UnityEngine;

namespace Inventory
{
    public class ToolbarSlot : InventorySlot
    {
        [SerializeField] [Tooltip("Slot to copy")]
        private InventorySlot inventorySlot;

        private InventoryItem _itemCopy;

        private void Start()
        {
            blockDrag = true;
            inventorySlot.onSlotUpdate.AddListener(OnSlotUpdate);
            inventorySlot.onSlotRemoved.AddListener(OnSlotRemove);
            _itemCopy = GetComponentInChildren<InventoryItem>();

            if (_itemCopy)
            {
                _itemCopy.gameObject.SetActive(false);
            }
        }

        private void OnSlotUpdate(InventoryItem realItem)
        {
            Debug.Log("Slot update");

            if (realItem == null)
            {
                Debug.Log("Slot null");
                return;
            }

            _itemCopy.gameObject.SetActive(true);
            CopyItem(realItem);
        }

        private void OnSlotRemove(InventoryItem realItem)
        {
            Debug.Log("Slot Remove");

            if (realItem == null)
            {
                Debug.Log("Slot null");
                return;
            }

            CleanItem();
            _itemCopy.gameObject.SetActive(false);
        }

        public override void OnItemUse(InventoryItem inventoryItem)
        {
            var originalItem = inventorySlot.GetComponentInChildren<InventoryItem>();
            if (originalItem)
            {
                inventorySlot.OnItemUse(originalItem);
            }
            else
            {
                Debug.LogWarning("Original item not found");
            }
        }

        private void CopyItem(InventoryItem realItem)
        {
            _itemCopy.item = realItem.item;
            _itemCopy.count = realItem.count;
            _itemCopy.tooltip.header = realItem.item.itemName;
            _itemCopy.tooltip.content = realItem.item.description;

            var tmpColor = _itemCopy.image.color;
            tmpColor.a = 1;
            _itemCopy.image.color = tmpColor;

            _itemCopy.image.sprite = realItem.image.sprite;
            _itemCopy.countText.text = realItem.countText.text;
            _itemCopy.frame.gameObject.SetActive(_itemCopy.count > 1);
        }

        private void CleanItem()
        {
            _itemCopy.item = null;
            _itemCopy.count = 0;
            _itemCopy.tooltip.header = "";
            _itemCopy.tooltip.content = "";

            var tmpColor = _itemCopy.image.color;
            tmpColor.a = 0;
            _itemCopy.image.color = tmpColor;

            _itemCopy.image.sprite = null;
            _itemCopy.countText.text = "";
        }
    }
}