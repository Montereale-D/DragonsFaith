using System;
using Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ItemContextMenuTrigger : MonoBehaviour, IPointerClickHandler
    {
        private InventoryItem _item;

        private void Awake()
        {
            _item = GetComponentInParent<InventoryItem>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ItemContextMenuSystem.Show(_item);
            }
        }
    }
}
