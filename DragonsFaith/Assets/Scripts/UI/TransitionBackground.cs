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
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        _animator = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void FadeOut()
    {
        _animator.SetTrigger(Out);
    }

    public void FadeIn()
    {
        _animator.SetTrigger(In);
    }

    public bool IsFadedOut()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName("fadeout");
    }
}
