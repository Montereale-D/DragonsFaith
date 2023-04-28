using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [ExecuteInEditMode]
    public class Tooltip : MonoBehaviour
    {
        public TextMeshProUGUI headerField;
        public TextMeshProUGUI contentField;
        public LayoutElement layoutElement;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        
        public void SetText(string content, string header = "")
        {
            if (string.IsNullOrEmpty(header))
            {
                headerField.gameObject.SetActive(false);
            }
            else
            {
                headerField.gameObject.SetActive(true);
                headerField.text = header;
            }

            contentField.text = content;
            
            layoutElement.enabled = Math.Max(headerField.preferredWidth, contentField.preferredWidth) >= layoutElement.preferredWidth;
        }
        
        private void Update()
        {
            if (Application.isEditor)
            {
                layoutElement.enabled = Math.Max(headerField.preferredWidth, contentField.preferredWidth) >= layoutElement.preferredWidth;
            }

            //TODO: change for the new input system
            Vector2 position = Input.mousePosition;

            var pivotX = position.x / Screen.width;
            var pivotY = position.y / Screen.width;
            
            var finalPivotX = 0f;
            var finalPivotY = 0f;
            if (pivotX < 0.5) //If mouse on left of screen move tooltip to right of cursor and vice vera
            {
                finalPivotX = -0.1f;
            }
            else
            {
                finalPivotX = 1.01f;
            }
            if (pivotY < 0.5) //If mouse on lower half of screen move tooltip above cursor and vice versa
            {
                finalPivotY = 0;
            }
            else
            {
                finalPivotY = 1;
            }
            _rectTransform.pivot = new Vector2(finalPivotX, finalPivotY);
            transform.position = position;
        }

        public void FadeStart()
        {
            LeanTween.alpha(_rectTransform, 1f, 0.2f).setEase(LeanTweenType.linear);
        }
        public void FadeFinished()
        {
            LeanTween.alpha(_rectTransform, 0f, 0.1f).setEase(LeanTweenType.linear);
        }
    }
}
