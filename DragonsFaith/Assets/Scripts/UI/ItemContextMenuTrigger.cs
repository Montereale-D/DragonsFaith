using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ItemContextMenuTrigger : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ItemContextMenuSystem.Show();
            }
        }
    }
}
