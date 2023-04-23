using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Inventory
{
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler

    {
        [HideInInspector] public Item item;
        [HideInInspector] public int count = 1;
        public Image image;
        public Text countText;
        [HideInInspector] public Transform parentAfterDrag;

        public void SetItem(Item newItem, int quantity)
        {
            item = newItem;
            count = quantity;
            image.sprite = newItem.image;
            UpdateCount();
        }

        public void UpdateCount()
        {
            countText.text = count.ToString();
            countText.gameObject.SetActive(count > 1);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            image.raycastTarget = false;
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            image.raycastTarget = true;
            transform.SetParent(parentAfterDrag);
        }

        private IEnumerator _coroutine;

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                if (_coroutine != null) StopCoroutine(_coroutine);
                DoubleClickAction();
            }
            else if (eventData.clickCount == 1)
            {
                _coroutine = OneClickAction();
                StartCoroutine(_coroutine);
            }
        }

        private IEnumerator OneClickAction()
        {
            yield return new WaitForSeconds(0.35f);
            GetComponentInParent<InventorySlot>().OnItemClick(this);
            _coroutine = null;
        }

        private void DoubleClickAction()
        {
            GetComponentInParent<InventorySlot>().OnItemUse(this);
        }
    }
}