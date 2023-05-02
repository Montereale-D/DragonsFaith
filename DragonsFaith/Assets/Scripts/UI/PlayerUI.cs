using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
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
            Faith,
            Options,
            Audio,
            Graphics,
        };

        public enum Element
        {
            Fire,
            Air,
            Earth,
            Water
        };

        [Header("Tabs")]
        public GameObject menuTab;
        public GameObject inventoryTab;
        public GameObject skillsTab;
        public GameObject faithTab;

        [Header("Pop Up")]
        [SerializeField] private Text popUpMessage;

        [Header("Faiths")]
        public Image faith;
        public Sprite fire;
        public Sprite air;
        public Sprite earth;
        public Sprite water;

        private bool _faithChoiceDone;
        public static Element chosenFaith;
        
        private RectTransform _rectTransformFaithTab;
        private static LTDescr delay;
        [SerializeField] [Tooltip("Time for the Faith tab to appear.")] 
        private float fadeInTime = 0.5f;
        [SerializeField] [Tooltip("Time for the Faith tab to dissolve.")] 
        private float fadeOutTime = 0.5f;
        
        public void ShowMessage(string message)
        {
            popUpMessage.text = message;
            PopUpMessage.Instance.GetComponent<PopUpMessage>().StartOpen();
        }
    
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
                case Tab.Faith:
                    FadeOutElement(_rectTransformFaithTab);
                    delay = LeanTween.delayedCall(fadeOutTime, () =>
                    {
                        faithTab.SetActive(false);
                    });
                    _faithChoiceDone = true;
                    break;
            }
        }
        
        private void SetFaithImage(Element element)
        {
            faith.sprite = element switch
            {
                Element.Fire => fire,
                Element.Air => air,
                Element.Earth => earth,
                Element.Water => water,
                _ => faith.sprite
            };
        }
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
            
            menuTab.SetActive(false);
            inventoryTab.SetActive(false);
            skillsTab.SetActive(false);
            faithTab.SetActive(true);
            
            _rectTransformFaithTab = faithTab.GetComponent<RectTransform>();
            
            FadeInElement(_rectTransformFaithTab);
        }

        private void Update()
        {
            if (!_faithChoiceDone) return;
            
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

            if (Input.GetKeyDown(KeyCode.T))
            {
                ShowMessage("Testing...");
            }
        }

        public void OpenMenu()
        {
            if (!_faithChoiceDone) return;
            SetMenu(Tab.Menu);
        }
        public void OpenInventory()
        {
            if (!_faithChoiceDone) return;
            SetMenu(Tab.Inventory);
        }
        public void OpenSkills()
        {
            if (!_faithChoiceDone) return;
            SetMenu(Tab.Skills);
        }

        private void CloseFaithTab()
        {
            SetMenu(Tab.Faith);
        }

        public void SetFire()
        {
            SetFaithImage(Element.Fire);
            CloseFaithTab();
            chosenFaith = Element.Fire;
        }
        
        public void SetAir()
        {
            SetFaithImage(Element.Air);
            CloseFaithTab();
            chosenFaith = Element.Air;
        }
        
        public void SetEarth()
        {
            SetFaithImage(Element.Earth);
            CloseFaithTab();
            chosenFaith = Element.Earth;
        }
        
        public void SetWater()
        {
            SetFaithImage(Element.Water);
            CloseFaithTab();
            chosenFaith = Element.Water;
        }

        private void FadeInElement(RectTransform rectTransform)
        {
            LeanTween.alpha(rectTransform, 1f, fadeInTime).setEase(LeanTweenType.linear);
        }
        
        private void FadeOutElement(RectTransform rectTransform)
        {
            LeanTween.alpha(rectTransform, 0f, fadeOutTime).setEase(LeanTweenType.linear);
        }
    }
}
