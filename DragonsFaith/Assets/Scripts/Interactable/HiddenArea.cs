using Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Serialization;

namespace Interactable
{
    public class HiddenArea : Openable
    {
        //[SerializeField] private AnimatorNetworkController networkAnimator;
        [SerializeField] private GameObject wall;
        [SerializeField] private Animator animator;
        private static readonly int Reveal = Animator.StringToHash("Reveal");
        
        public override bool OpenAction()
        {
            if (!base.OpenAction()) return false;
            
            //animator.ActivateAnimator();
            
            Debug.Log("animator activating");
            animator.SetTrigger(Reveal);
            wall.SetActive(false);
            
            
            return true;
        }

        public override bool CloseAction()
        {
            return base.CloseAction();
        }
        
        
    }
}