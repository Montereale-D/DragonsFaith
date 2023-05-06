using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ItemContextMenuSystem : MonoBehaviour, IPointerClickHandler
    {
        private static ItemContextMenuSystem current;
        public ItemContextMenu contextMenu;
        private static Image background;

        private void Awake()
        {
            current = this;
            background = GetComponent<Image>();
            background.enabled = false;
        }
        
        public static void Show()
        {
            background.enabled = true;
            current.contextMenu.Open();
        }

        public static void Hide()
        {
            background.enabled = false;
            current.contextMenu.Close();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Hide();
            }
        }
    }
}
