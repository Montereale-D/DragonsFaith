using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    public class HiddenArea : Openable
    {
        [SerializeField] private AnimatorNetworkController animator;

        public override bool OpenAction()
        {
            //if (!base.OpenAction()) return false;
            
            Debug.Log("HiddenArea OpenAction");
            
            //animator.ActivateAnimatorProcedureClientRpc();
            if (NetworkManager.Singleton.IsHost)
            {
                animator.ActivateAnimatorProcedureServerRpc();
            }
            
            return true;
        }

        public override bool CloseAction()
        {
            return base.CloseAction();
        }
    }
}