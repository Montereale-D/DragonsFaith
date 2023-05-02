using System;
using System.Collections.Generic;
using System.Linq;
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
            Faith,
            Main,
            Options,
            Audio,
            Graphics,
            KeyBindings
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
        
        [Header("Option Screens")]
        public GameObject mainScreen;
        public GameObject optionsScreen;
        public GameObject audioScreen;
        public GameObject graphicsScreen;
        public GameObject keybindingsScreen;
        
        [Header("Graphics and Audio")]
        private Resolution[] _resolutions;
        public Dropdown resolutionDropdown;
        public Slider playerVolumeSlider;
        public Slider enemyVolumeSlider;
        public Slider backgroundVolumeSlider;

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
                    mainScreen.SetActive(true);
                    optionsScreen.SetActive(false);
                    audioScreen.SetActive(false);
                    graphicsScreen.SetActive(false);
                    keybindingsScreen.SetActive(false);
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
                case Tab.Main:
                    mainScreen.SetActive(true);
                    optionsScreen.SetActive(false);
                    break;
                case Tab.Options:
                    mainScreen.SetActive(false);
                    optionsScreen.SetActive(true);
                    audioScreen.SetActive(false);
                    graphicsScreen.SetActive(false);
                    keybindingsScreen.SetActive(false);
                    break;
                case Tab.Audio:
                    optionsScreen.SetActive(false);
                    audioScreen.SetActive(true);
                    break;
                case Tab.Graphics:
                    optionsScreen.SetActive(false);
                    graphicsScreen.SetActive(true);
                    break;
                case Tab.KeyBindings:
                    optionsScreen.SetActive(false);
                    keybindingsScreen.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
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
            
            var resolutions = Screen.resolutions.Select(resolution => new Resolution
            {
                width = resolution.width, height = resolution.height
            }).Distinct();
            _resolutions = resolutions as Resolution[] ?? resolutions.ToArray();
        
            resolutionDropdown.ClearOptions();
            var options = new List<string>();
            var currentResolutionIndex = 0;
            for (var i = 0; i < _resolutions.Length; i++)
            {
                var option = _resolutions[i].width + "x" + _resolutions[i].height;
                options.Add(option);
    
                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            
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
                else if (audioScreen.activeSelf || graphicsScreen.activeSelf ||
                         keybindingsScreen.activeSelf)
                {
                    OpenOptions();
                }
                else if (optionsScreen.activeSelf)
                {
                    OpenMain();
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
        public void OpenMain()
        {
            SetMenu(Tab.Main);
        }
        public void OpenOptions()
        {
            SetMenu(Tab.Options);
        }
        public void OpenAudio()
        {
            SetMenu(Tab.Audio);
        }
        public void OpenGraphics()
        {
            SetMenu(Tab.Graphics);
        }
        public void OpenKeyBindings()
        {
            SetMenu(Tab.KeyBindings);
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
        
        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }
        
        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
        
        public void SetResolution(int resolutionIndex)
        {
            var resolution = _resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
}
