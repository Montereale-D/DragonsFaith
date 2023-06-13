using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionBackground : MonoBehaviour
{
    public static TransitionBackground instance { get; private set; }
    private Animator _animator;
    private static readonly int Out = Animator.StringToHash("fadeOut");
    private static readonly int In = Animator.StringToHash("fadeIn");

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        _animator = GetComponent<Animator>();
    }

    public void FadeOut()
    {
        _animator.SetTrigger(Out);
    }

    public void FadeIn()
    {
        _animator.SetTrigger(In);
    }
}
