using UnityEngine;

namespace Interactable
{
    /// <summary>
    /// Use this abstract class as a start point for an openable object (or for an objects that can be activated).
    /// </summary>
    public abstract class Openable : MonoBehaviour
    {
        public abstract bool OpenAction();
        public abstract bool CloseAction();
    }
}