using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
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
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
}
