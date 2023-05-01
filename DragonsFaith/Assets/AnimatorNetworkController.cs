using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class AnimatorNetworkController : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private static readonly int Reveal = Animator.StringToHash("Reveal");

    [SerializeField] private NetworkAnimator networkAnimator;
    private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    
    public override void OnNetworkSpawn()
    {
        //subscribe to status change event
        _isActive.OnValueChanged += (_, _) =>
        {
            animator.SetTrigger(Reveal);
            networkAnimator.SetTrigger(Reveal);
        };
    }
    
    [ClientRpc]
    public void ActivateAnimatorProcedureClientRpc()
    {
        _isActive.Value = true;
    }
}
