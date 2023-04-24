using UnityEngine;

namespace Interactable
{
    public abstract class Openable : MonoBehaviour
    {
        public abstract bool OpenAction();
        public abstract bool CloseAction();
    }
}