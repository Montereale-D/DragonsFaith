using System;
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
            
            try
            {
                if (current.tooltip != null)
                {
                    current.tooltip.SetText(content, header);
                    current.tooltip.gameObject.SetActive(true);
                    current.tooltip.FadeStart();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Hide()
        {
            if (!current.tooltip.enabled || current.tooltip == null) return;
            current.tooltip.FadeFinished();
            LeanTween.delayedCall(0.1f, () => { current.tooltip.gameObject.SetActive(false); });
        }
    }
}