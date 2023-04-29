using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Android;

namespace Interactable
{
    public class HiddenArea : Openable
    {
        //private SpriteRenderer _spriteRenderer;
        [SerializeField] private AnimatorNetworkController animator;
        //[SerializeField] private Animator animator;
        //private static readonly int Reveal = Animator.StringToHash("Reveal");

        /*private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }*/

        public override bool OpenAction()
        {
            if (!base.OpenAction()) return false;
            
            animator.ActivateAnimatorProcedureClientRpc();
            //animator.SetTrigger(Reveal);
            gameObject.SetActive(false);
            return true;
        }

        public override bool CloseAction()
        {
            if (!base.CloseAction()) return false;
            return true;
        }
    }
}