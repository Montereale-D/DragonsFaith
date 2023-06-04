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
    private static LTDescr delay;

    private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        //subscribe to status change event
        _isActive.OnValueChanged += RevealHiddenArea;
    }

    public void RevealHiddenArea(bool previousvalue, bool newvalue)
    {
        animator.SetTrigger(Reveal);
        wall.SetActive(false);
        delay = LeanTween.delayedCall(1f, () => { gameObject.SetActive(false); });
    }

    [ClientRpc]
    public void ActivateAnimatorProcedureClientRpc()
    {
        _isActive.Value = true;
    }
}