using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerUI : MonoBehaviour
    {
        private enum Tab
        {
            Menu,
            Inventory,
            Skills,
            Options,
            Audio,
            Graphics,
        };
        
        public GameObject menuTab;
        public GameObject inventoryTab;
        public GameObject skillsTab;
        [SerializeField] private Text popUpMessage;
    
        private void SetMenu(Tab menu)
        {
            switch (menu)
            {
                case Tab.Menu:
                    menuTab.SetActive(!menuTab.activeSelf);
                    inventoryTab.SetActive(false);
                    skillsTab.SetActive(false);
                    break;
                case Tab.Inventory:
                    menuTab.SetActive(false);
                    inventoryTab.SetActive(!inventoryTab.activeSelf);
                    skillsTab.SetActive(false);
                    break;
                case Tab.Skills:
                    menuTab.SetActive(false);
                    inventoryTab.SetActive(false);
                    skillsTab.SetActive(!skillsTab.activeSelf);
                    break;
            }
        }
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
            
            menuTab.SetActive(false);
            inventoryTab.SetActive(false);
            skillsTab.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                OpenInventory();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                OpenSkills();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (inventoryTab.activeSelf)
                {
                    OpenInventory();
                }
                else if (skillsTab.activeSelf)
                {
                    OpenSkills();
                }
                else
                {
                    OpenMenu();
                }
            }
        }

        public void OpenMenu()
        {
            SetMenu(Tab.Menu);
        }
        public void OpenInventory()
        {
            SetMenu(Tab.Inventory);
        }
        public void OpenSkills()
        {
            SetMenu(Tab.Skills);
        }

        public void ShowMessage(string message)
        {
            popUpMessage.text = message;
            PopUpMessage.Instance.GetComponent<PopUpMessage>().StartOpen();
        }
    }
}
