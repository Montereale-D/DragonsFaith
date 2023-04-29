using UnityEngine;

namespace Interactable
{
    public class ShowKeyOnTrigger : MonoBehaviour
    {
        public delegate void OnKeyPressed(Collider2D collider2D);
        public event OnKeyPressed KeyPressed;
        [SerializeField] private KeyCode key;
        
        [SerializeField] private bool turnOffOnKeyPress = true;

        private GameObject _keyImage;
        private bool _isVisible;
        private int _triggerCount;
        private Collider2D _collider;

        private void Awake()
        {
            _keyImage = transform.GetChild(0).gameObject;
            _keyImage.SetActive(false);
        }

        private void Update()
        {
            if (!_isVisible) return;
            if (!Input.GetKeyDown(key)) return;

            if (turnOffOnKeyPress)
            {
                TurnOff();
            }

            KeyPressed?.Invoke(_collider);
        }

        public void TurnOff()
        {
            gameObject.SetActive(false);
        }

        public void TurnOn()
        {
            gameObject.SetActive(true);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            _triggerCount++;

            if (_isVisible) return;
            if (_triggerCount != 1) return;

            _isVisible = true;
            _keyImage.SetActive(true);
            _collider = col;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _triggerCount--;

            if (!_isVisible) return;
            if (_triggerCount != 0) return;

            _isVisible = false;
            _keyImage.SetActive(false);
            _collider = null;
        }
    }
}