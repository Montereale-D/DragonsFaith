using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// This script lets you create a draggable UI element.
    /// </summary>
    public class DraggableWindow : MonoBehaviour, IDragHandler
    {
        public Canvas canvas;

        private RectTransform _rectTransform;

        // Start is called before the first frame update
        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
}
