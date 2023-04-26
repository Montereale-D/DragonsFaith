using UnityEngine;
using UnityEngine.Android;

namespace Interactable
{
    public class HiddenArea : Openable
    {
        //TODO: add animator and animation when opening door
        
        //private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator animator;
        private static readonly int Reveal = Animator.StringToHash("Reveal");

        /*private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }*/

        public override bool OpenAction()
        {
            animator.SetTrigger(Reveal);
            gameObject.SetActive(false);
            return true;
        }

        public override bool CloseAction()
        {
            return true;
        }
    }
}