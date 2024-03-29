using System.Collections;
using Inventory.Items;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory
{
    /// <summary>
    /// Represent an item (icon) inside a slot. Contains a reference to the item and the quantity (in that slot).
    /// </summary>
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler

    {
        public Image image;
        public TMP_Text countText;
        public Image frame;
        public TooltipTrigger tooltip;

        [HideInInspector] public Item item;
        [HideInInspector] public int count = 1;
        private Transform _parentAfterDrag;

        private IEnumerator _coroutineOnClick;

        public void SetItem(Item newItem, int quantity)
        {
            item = newItem;
            count = quantity;
            image.sprite = newItem.image;
            tooltip.header = newItem.itemName;
            tooltip.content = newItem.description;
            UpdateCount();
        }

        //Update the text containing the number of this item in that slot
        public void UpdateCount()
        {
            countText.text = count.ToString();

            //show number only if there is more than one item in that slot
            frame.gameObject.SetActive(count > 1);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //save reference to parent and set parent to the topmost transform
            image.raycastTarget = false;
            _parentAfterDrag = transform.parent;
            // transform.SetParent(transform.root);
            transform.SetParent(transform.root.GetComponentInChildren<PlayerUI>().transform);
        }

        public void OnDrag(PointerEventData eventData)
        {
            //update item image position according to mouse position
            if (_parentAfterDrag.GetComponent<InventorySlot>().blockDrag) return;
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //set the new parent according to the new parent
            image.raycastTarget = true;
            transform.SetParent(_parentAfterDrag);
            transform.position = _parentAfterDrag.position;
        }

        public void UpdateParent(Transform parentTransform)
        {
            var slot = _parentAfterDrag.GetComponent<InventorySlot>();
            if (slot.blockDrag) return;

            if (_parentAfterDrag != null && _parentAfterDrag != parentTransform)
            {
                slot.onSlotRemoved.Invoke(this);
            }
            
            _parentAfterDrag = parentTransform;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                //if one click action is waiting, stop it
                if (_coroutineOnClick != null) StopCoroutine(_coroutineOnClick);
                DoubleClickAction();
            }
            else if (eventData.clickCount == 1)
            {
                //wait, maybe is a double click
                _coroutineOnClick = OneClickAction();
                StartCoroutine(_coroutineOnClick);
            }
        }

        private IEnumerator OneClickAction()
        {
            yield return new WaitForSeconds(0.35f);
            GetComponentInParent<InventorySlot>().OnItemClick(this);
            _coroutineOnClick = null;
        }

        private void DoubleClickAction()
        {
            UseItemAction();
            //GetComponentInParent<InventorySlot>().OnItemSend(this);
        }

        public override string ToString()
        {
            return count + " X " + item;
        }
        
        public void SendItemAction()
        {
            GetComponentInParent<InventorySlot>().OnItemSend(this);
        }

        public void UseItemAction()
        {
            GetComponentInParent<InventorySlot>().OnItemUse(this);
        }

        public void DestroyItemAction()
        {
            GetComponentInParent<InventorySlot>().OnItemDestroy(this);
        }

        public void DestroyAllItemAction()
        {
            GetComponentInParent<InventorySlot>().OnItemDestroyAll(this);
        }
    }
}