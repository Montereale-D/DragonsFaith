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
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            delay = LeanTween.delayedCall(0.5f, () =>
            {
                TooltipSystem.Show(content, header);
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            LeanTween.cancel(delay.uniqueId);
            TooltipSystem.Hide();
        }

        private void OnDestroy()
        {
            LeanTween.cancel(delay.uniqueId);
            TooltipSystem.Hide();
        }
        
        private void OnDisable()
        {
            LeanTween.cancel(delay.uniqueId);
            TooltipSystem.Hide();
        }
    }
}
