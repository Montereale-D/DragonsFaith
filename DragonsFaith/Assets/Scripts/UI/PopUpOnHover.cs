using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// This script lets you create a UI element that pops up only when the mouse overs over the gameObject
    /// the script is in as a component. The gameObject needs to have a collider that represents the area the mouse 
    /// needs to enter and exit for the methods to be called. The UI element is the one passed as parameter.
    /// </summary>
    public class PopUpOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject gameObjectUI;
        // Start is called before the first frame update
        /*private void Start()
        {
            gameObjectUI.SetActive(false);
        }*/

        /*private void OnMouseOver()
        {
            gameObjectUI.SetActive(true);
        }

        private void OnMouseExit()
        {
            gameObjectUI.SetActive(false);
        }*/

        public void OnPointerEnter(PointerEventData eventData)
        {
            gameObjectUI.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            gameObjectUI.SetActive(false);
        }
    }
}
