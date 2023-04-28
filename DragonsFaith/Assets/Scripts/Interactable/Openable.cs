using UnityEngine;
using UnityEngine.Serialization;

namespace Interactable
{
    /// <summary>
    /// Use this abstract class as a start point for an openable object (or for an objects that can be activated).
    /// </summary>
    public abstract class Openable : MonoBehaviour
    {
        public bool ignoreCloseAction;
        public bool isOpened;

        /// <summary>
        /// Return true if CLOSE -> OPEN
        /// </summary>
        public virtual bool OpenAction()
        {
            if (isOpened) return false;

            isOpened = true;
            return true;
        }

        /// <summary>
        /// Return true if OPEN -> CLOSE
        /// </summary>
        public virtual bool CloseAction()
        {
            if (ignoreCloseAction)
            {
                return false;
            }

            if (!isOpened) return false;

            isOpened = false;
            return true;
        }
    }
}