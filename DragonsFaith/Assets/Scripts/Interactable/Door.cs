using UnityEngine;
using UnityEngine.Android;

namespace Interactable
{
    public class Door : Openable
    {
        public Sprite closedDoor;
        public Sprite openDoor;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _boxCollider.enabled = true;
        }

        public override bool OpenAction()
        {
            if (!base.OpenAction()) return false;
            
            //_spriteRenderer.color = Color.green;
            _spriteRenderer.sprite = openDoor;
            _boxCollider.enabled = false;
            return true;

        }

        public override bool CloseAction()
        {
            if (!base.CloseAction()) return false;
            
            //_spriteRenderer.color = Color.red;
            _spriteRenderer.sprite = closedDoor;
            _boxCollider.enabled = true;
            return true;
        }
    }
}