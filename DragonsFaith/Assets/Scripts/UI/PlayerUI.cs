using System;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

        [Serializable]
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
        public Dropdown resolutionDropdown;
        public Slider playerVolumeSlider;
        public Slider enemyVolumeSlider;
        public Slider backgroundVolumeSlider;

        [Header("Player UI")] 
        public GameObject playerUI;
        public GameObject settingsButton;
        public Image portrait;
        public Sprite[] portraitSprites;
        
        [Header("CombatUI")]
        public RectTransform combatUI;
        public Button moveOrAttackButton;
        public Button skipTurnButton;
        public Button reloadButton;
        public Button blockButton;
        public Sprite attackSprite;
        public Sprite moveSprite;
        public TextMeshProUGUI ammoCounter;
        public TextMeshProUGUI movementCounter;
        public TextMeshProUGUI rangeCounter;

        [Header("Character Info")] 
        public TextMeshProUGUI nameText;
        public Slider healthSlider;
        public TextMeshProUGUI healthText;
        public Slider manaSlider;
        public TextMeshProUGUI manaText;
        
        [Header("Slots")]
        [Tooltip("Insert (in order) all the inventory slots, ...")]
        public InventorySlot[] inventorySlots;
        [Tooltip("Insert (in order) all the equipment slots, ...")]
        public InventorySlot[] equipmentSlots;

        [Header("Pop Up")] [SerializeField] 
        private PopUpMessage popUpMessage;

        [Header("Faiths")] public Image faith;
        public Sprite fire;
        public Sprite air;
        public Sprite earth;
        public Sprite water;

        private bool _faithChoiceDone;
        public Element chosenFaith;

        private RectTransform _rectTransformFaithTab;
        private RectTransform _rectTransformTurnUI;
        private static LTDescr delay;
        [SerializeField] [Tooltip("Faith tab fade in duration.")] 
        private float faithTabFadeInTime = 0.5f;
        [SerializeField] [Tooltip("Faith tab fade out duration.")] 
        private float faithTabFadeOutTime = 0.5f;
        [SerializeField] [Tooltip("Faith tab fade in duration.")] 
        private float turnUIFadeInTime = 0.5f;
        [SerializeField] [Tooltip("Faith tab fade out duration.")] 
        private float turnUIFadeOutTime = 0.5f;
        
        private OptionsManager _optionsManager;

        private UnityAction _moveOrAttackAction;
        private Image _moveOrAttackImage;
        private TextMeshProUGUI _moveOrAttackText;
        public TurnUI turnUI;
        public int portraitIdx;

        public static PlayerUI Instance { get; private set; }

        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }

            
            
            menuTab.SetActive(false);
            inventoryTab.SetActive(false);
            skillsTab.SetActive(false);
            faithTab.SetActive(true);
            
            _optionsManager = OptionsManager.Instance;
            
            _optionsManager.SetDropdown(resolutionDropdown);

            nameText.text = _optionsManager.RetrievePlayerName();
        
            playerVolumeSlider.value = _optionsManager.GetPlayerVolumeSound();
            enemyVolumeSlider.value = _optionsManager.GetEnemyVolumeSound();
            backgroundVolumeSlider.value = _optionsManager.GetBackgroundVolumeSound();
            
            //GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<CharacterManager>().SetPlayerName();
            
            //_rectTransformTurnUI = turnUI.GetComponent<RectTransform>();
            _rectTransformFaithTab = faithTab.GetComponent<RectTransform>();
            FadeInElement(_rectTransformFaithTab, faithTabFadeInTime);
        }

        private void Start()
        {
            InventoryManager.Instance.SetUpSlots(inventorySlots, equipmentSlots);
        }

        private void Update()
        {
            if (!_faithChoiceDone) return;
            //if (_faithChoiceDone && faithTab.activeSelf) faithTab.SetActive(false);
            
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

            /*if (Input.GetKeyDown(KeyCode.T))
            {
                /*ShowMessage("Testing...");#1#
                ToggleCombatUI();
            }*/
        }
        
        public void ShowMessage(string message)
        {
            popUpMessage.uiSettings.text.text = message;
            popUpMessage.StartOpen();
        }
        
        public void ShowUI(bool b)
        {
            GetComponent<Canvas>().enabled = b;
        }
        
        #region OpenTabFunctions

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
                    FadeOutElement(_rectTransformFaithTab, faithTabFadeOutTime);
                    /*delay = LeanTween.delayedCall(faithTabFadeOutTime, () =>
                    {
                        faithTab.SetActive(false);
                    });*/
                    faithTab.SetActive(false);
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
        
        private void SetFaithSprite(Element element)
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

        private void SetPortraitSprite()
        {
            var spriteIdx = Random.Range(0, portraitSprites.Length);
            portraitIdx = spriteIdx;
            portrait.sprite = portraitSprites[spriteIdx];
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
            SetPortraitSprite();
            playerUI.SetActive(true);
            settingsButton.SetActive(true);
        }

        public void SetFire()
        {
            SetFaithSprite(Element.Fire);
            CloseFaithTab();
            chosenFaith = Element.Fire;
        }
        
        public void SetAir()
        {
            SetFaithSprite(Element.Air);
            CloseFaithTab();
            chosenFaith = Element.Air;
        }
        
        public void SetEarth()
        {
            SetFaithSprite(Element.Earth);
            CloseFaithTab();
            chosenFaith = Element.Earth;
        }
        
        public void SetWater()
        {
            SetFaithSprite(Element.Water);
            CloseFaithTab();
            chosenFaith = Element.Water;
        }
        
        #endregion

        private void FadeInElement(RectTransform rectTransform, float fadeInDuration)
        {
            LeanTween.alpha(rectTransform, 1f, fadeInDuration).setEase(LeanTweenType.linear);
        }
        
        private void FadeOutElement(RectTransform rectTransform, float fadeOutDuration)
        {
            LeanTween.alpha(rectTransform, 0f, fadeOutDuration).setEase(LeanTweenType.linear);
        }
        
        #region UpdateValues
        
        public void UpdateMaxHealth(int value)
        {
            healthSlider.maxValue = value;
            UpdateHealthBar((int)healthSlider.value, value);
        }

        public void UpdateMaxMana(int value)
        {
            manaSlider.maxValue = value;
            UpdateManaBar((int)manaSlider.value, value);
        }

        public void UpdateHealthBar(int value, int maxValue)
        {
            healthSlider.value = value;
            healthText.text = "Life: " + value + "/" + maxValue;
        }
        public void UpdateManaBar(int value, int maxValue)
        {
            manaSlider.value = value;
            manaText.text = "Mana: " + value + "/" + maxValue;
        }
        public void SetBackgroundVolume(float value)
        {
            _optionsManager.SetBackgroundVolume(value);
        }
        public void SetPlayerVolume(float value)
        {
            _optionsManager.SetPlayerVolume(value);
        }
        public void SetEnemyVolume(float value)
        {
            _optionsManager.SetEnemyVolume(value);
        }
        
        public void SetQuality(int qualityIndex)
        {
            OptionsManager.SetQuality(qualityIndex);
        }
        
        public void SetFullscreen(bool isFullscreen)
        {
            OptionsManager.SetFullscreen(isFullscreen);
        }
        
        public void SetResolution(int resolutionIndex)
        {
            _optionsManager.SetResolution(resolutionIndex);
        }

        public void SetAmmoCounter(int value)
        {
            ammoCounter.text = "Ammo: " + value.ToString();
        }
        
        public void SetMovementCounter(int value)
        {
            movementCounter.text = "Move range: " + value.ToString();
        }
        
        public void SetRangeCounter(int value)
        {
            rangeCounter.text = value.ToString();
        }
        
        #endregion
        
        public void ToggleCombatUI(List<PlayerGridMovement> characterList)
        {
            if (!combatUI.gameObject.activeSelf)
            {
                combatUI.gameObject.SetActive(true);
                FadeInElement(combatUI, turnUIFadeInTime);
                //SetAmmoCounter(InventoryManager.Instance.GetWeapon().GetAmmo());
                //SetRangeCounter((int)InventoryManager.Instance.GetWeapon().GetRange());   //uncomment when adding range counter
                SetMovementCounter((int)CharacterManager.Instance.GetTotalAgi());
                
                turnUI = combatUI.GetComponentInChildren<TurnUI>();
                /*Debug.Log("spriteIdx=" + spriteIdx);
                for (int i = 0; i < portraitSprites.Length; i++)
                {
                    Debug.Log(portraitSprites[i].name);
                }
                var otherPlayerSprite = portraitSprites[spriteIdx];*/
                
                turnUI.SetUpList(characterList);

                _moveOrAttackText = moveOrAttackButton.GetComponentInChildren<TextMeshProUGUI>();
                var imgs = moveOrAttackButton.GetComponentsInChildren<Image>();
                foreach (var img in imgs)
                {
                    if (img.gameObject.GetInstanceID() != moveOrAttackButton.GetInstanceID())
                        _moveOrAttackImage = img;
                }
                
                skipTurnButton.onClick.AddListener(CombatSystem.instance.SkipTurn);
                reloadButton.onClick.AddListener(CombatSystem.instance.ReloadAction);
                blockButton.onClick.AddListener(CombatSystem.instance.BlockAction);
            }
            else
            {
                FadeOutElement(combatUI, turnUIFadeOutTime);
                moveOrAttackButton.onClick.RemoveAllListeners();
                skipTurnButton.onClick.RemoveAllListeners();
                reloadButton.onClick.RemoveAllListeners();
                blockButton.onClick.RemoveAllListeners();
                delay = LeanTween.delayedCall(turnUIFadeOutTime, () =>
                {
                    combatUI.gameObject.SetActive(false);
                    turnUI.DestroyList();
                });
            }
        }

        public void ToggleMoveAttackButton(string mode)
        {
            switch (mode)
            {
                case "Attack":
                    moveOrAttackButton.onClick.RemoveAllListeners();
                    _moveOrAttackImage.sprite = attackSprite;
                    _moveOrAttackText.text = "Attack";
                    moveOrAttackButton.onClick.AddListener(CombatSystem.instance.ButtonCheckAction);
                    break;
                case "Move":
                    moveOrAttackButton.onClick.RemoveAllListeners();
                    _moveOrAttackImage.sprite = moveSprite;
                    _moveOrAttackText.text = "Move";
                    moveOrAttackButton.onClick.AddListener(CombatSystem.instance.ButtonCheckMovement);
                    break;
            }
        }
    }
}