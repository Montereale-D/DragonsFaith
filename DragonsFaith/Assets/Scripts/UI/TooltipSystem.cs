using UnityEngine;

namespace UI
{
    public class TooltipSystem : MonoBehaviour
    {
        private static TooltipSystem current;
        public Tooltip tooltip;

        private void Awake()
        {
            current = this;
        }

        public static void Show(string content, string header = "")
        {
            current.tooltip.SetText(content, header);
            current.tooltip.gameObject.SetActive(true);
            current.tooltip.FadeStart();
        }

        public static void Hide()
        {
            current.tooltip.FadeFinished();
            LeanTween.delayedCall(0.1f, () =>
            {
                current.tooltip.gameObject.SetActive(false);
            });
        }    
    }
}
