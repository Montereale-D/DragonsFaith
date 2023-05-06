using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NetworkPopUpOnHover : NetworkBehaviour
    {
        public GameObject gameObjectUI;
        public TextMeshProUGUI nameText;
        public Slider healthSlider;
        public TextMeshProUGUI healthText;
        
        // Start is called before the first frame update
        private void Start()
        {
            gameObjectUI.SetActive(false);
        }

        private void OnMouseOver()
        {
            if (!IsOwner)
            {
                gameObjectUI.SetActive(true);
            }
        }

        private void OnMouseExit()
        {
            if (!IsOwner)
            {
                gameObjectUI.SetActive(false);
            }
        }
    }
}
