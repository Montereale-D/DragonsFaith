using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Network
{
    public class AnimatorNetworkController : NetworkBehaviour
    {
        [SerializeField] private GameObject wall;
        //[SerializeField] private Animator animator;
        [SerializeField] private NetworkAnimator networkAnimator;
        private static readonly int Reveal = Animator.StringToHash("Reveal");
        //private static LTDescr delay;
    
        /*private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);*/
    
        /*public override void OnNetworkSpawn()
        {
            //subscribe to status change event
            _isActive.OnValueChanged += (_, _) =>
            {
                Debug.Log("value changed");
                animator.SetTrigger(Reveal);
                wall.SetActive(false);
            };
        }*/
    
        public void ActivateAnimator()
        {
            Debug.Log("host animator activating");
            //networkAnimator.SetTrigger(Reveal);
            networkAnimator.Animator.SetTrigger(Reveal);
            wall.SetActive(false);
            ActivateAnimatorClientRpc();
        }
        
        [ClientRpc]
        public void ActivateAnimatorClientRpc()
        {
            if (!IsHost) return;
            Debug.Log("client animator activating");
            networkAnimator.SetTrigger(Reveal);
            wall.SetActive(false);
        }
    }
}
