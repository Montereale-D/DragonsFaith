using UnityEngine;

namespace UI
{
    public class PopUpOnHover : MonoBehaviour
    {
        public GameObject enemyUI;
        // Start is called before the first frame update
        private void Start()
        {
            enemyUI.SetActive(false);
        }

        private void OnMouseOver()
        {
            enemyUI.SetActive(true);
        }

        private void OnMouseExit()
        {
            enemyUI.SetActive(false);
        }
    }
}
