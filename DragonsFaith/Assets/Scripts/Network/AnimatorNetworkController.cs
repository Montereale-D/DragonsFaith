using System.Collections;
using System.Collections.Generic;
using Interactable;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class AnimatorNetworkController : NetworkBehaviour
{
    [SerializeField] private GameObject wall;
    [SerializeField] private Animator animator;
    private static readonly int Reveal = Animator.StringToHash("Reveal");

    private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        //subscribe to status change event
        _isActive.OnValueChanged += (_, _) =>
        {
            Debug.Log("Animator value changed");
            animator.SetTrigger(Reveal);
            wall.SetActive(false);
            LeanTween.delayedCall(1f, () => { gameObject.SetActive(false); });
        };
    }

    [ClientRpc]
    public void ActivateAnimatorProcedureClientRpc()
    {
        if (IsHost) return;
        Debug.Log("ActivateAnimatorProcedureClientRpc");
        
        animator.SetTrigger(Reveal);
        wall.SetActive(false);
        LeanTween.delayedCall(1f, () => { gameObject.SetActive(false); });
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateAnimatorProcedureServerRpc()
    {
        if (!IsHost) return;
        Debug.Log("ActivateAnimatorProcedureServerRpc");

        _isActive.Value = true;
        
        ActivateAnimatorProcedureClientRpc();
        
        animator.SetTrigger(Reveal);
        wall.SetActive(false);
        LeanTween.delayedCall(1f, () => { gameObject.SetActive(false); });
    }
}