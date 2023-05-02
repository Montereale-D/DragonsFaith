using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static LTDescr delay;
        public string header;
        
        [Multiline]
        public string content;

        private bool _completed;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            delay = LeanTween.delayedCall(0.5f, () =>
            {
                TooltipSystem.Show(content, header);
                _completed = true;
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (delay == null) return;
            if (_completed)
            {
                TooltipSystem.Hide();
                _completed = false;
            }
            else LeanTween.cancel(delay.uniqueId);
        }

        private void OnDestroy()
        {
            if (delay == null) return;
            if (_completed)
            {
                TooltipSystem.Hide();
                _completed = false;
            }
            else LeanTween.cancel(delay.uniqueId);
        }
        
        private void OnDisable()
        {
            if (delay == null) return;
            if (_completed)
            {
                TooltipSystem.Hide();
                _completed = false;
            }
            else LeanTween.cancel(delay.uniqueId);
        }
    }
}
