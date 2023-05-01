using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Android;

namespace Interactable
{
    public class HiddenArea : Openable
    {
        [SerializeField] private AnimatorNetworkController animator;

        public override bool OpenAction()
        {
            if (!base.OpenAction()) return false;
            
            animator.ActivateAnimatorProcedureClientRpc();
            return true;
        }

        public override bool CloseAction()
        {
            return base.CloseAction();
        }
    }
}