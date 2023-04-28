using UnityEngine;
using UnityEngine.Android;

namespace Interactable
{
    public class Door : Openable
    {
        //TODO: add animator and animation when opening door
        
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override bool OpenAction()
        {
            if (!base.OpenAction()) return false;
            
            _spriteRenderer.color = Color.green;
            return true;

        }

        public override bool CloseAction()
        {
            if (!base.CloseAction()) return false;
            
            _spriteRenderer.color = Color.red;
            return true;
        }
    }
}