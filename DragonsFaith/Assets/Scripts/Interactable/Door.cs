using UnityEngine;

namespace Interactable
{
    public class Door : Openable
    {
        private SpriteRenderer _spriteRenderer;
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override bool OpenAction()
        {
            _spriteRenderer.color = Color.green;
            return true;
        }

        public override bool CloseAction()
        {
            _spriteRenderer.color = Color.red;
            return true;
        }
    }
}
