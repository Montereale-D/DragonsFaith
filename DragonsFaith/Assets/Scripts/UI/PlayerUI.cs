using System;
using TMPro;
using System.Collections.Generic;
using Inventory;
using Network;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
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
        public GameObject dungeonTutorial;
        public Image portrait;
        public Sprite[] portraitSprites;
        
        [Header("CombatUI")]
        public RectTransform combatUI;
        private CombatUI _combatUI;

        [Header("Character Info")] 
        public TextMeshProUGUI nameText;
        public Slider healthSlider;
        public TextMeshProUGUI healthText;
        public Slider manaSlider;
        public TextMeshProUGUI manaText;
        public TextMeshProUGUI strengthText;
        public TextMeshProUGUI agilityText;
        public TextMeshProUGUI dexterityText;
        public TextMeshProUGUI constitutionText;
        public TextMeshProUGUI intelligenceText;

        [Header("Slots")] [Tooltip("Insert (in order) all the inventory slots, ...")]
        public InventorySlot[] inventorySlots;

        [Tooltip("Insert (in order) all the equipment slots, ...")]
        public InventorySlot[] equipmentSlots;

        public InventorySlot activeSkillSlot;
        public InventorySlot[] passiveSkillSlots;

        [Header("Pop Up")] [SerializeField] private PopUpMessage popUpMessage;

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
        private AudioManager _audioManager;

        private UnityAction _moveOrAttackAction;
        private Image _moveOrAttackImage;
        private TextMeshProUGUI _moveOrAttackText;
        [HideInInspector] public Sprite otherPlayerSprite;
        [HideInInspector] public int portraitIdx;

        public static PlayerUI instance { get; private set; }


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            Debug.Log("PlayerUI setup");
            instance = this;
            DontDestroyOnLoad(this);

            menuTab.SetActive(false);
            inventoryTab.SetActive(false);
            skillsTab.SetActive(false);
            faithTab.SetActive(true);

            _optionsManager = OptionsManager.instance;
            _audioManager = AudioManager.instance;

            _optionsManager.SetDropdown(resolutionDropdown);

            //nameText.text = _optionsManager.RetrievePlayerName();

            playerVolumeSlider.value = _audioManager.GetPlayerVolumeSound();
            enemyVolumeSlider.value = _audioManager.GetEnemyVolumeSound();
            backgroundVolumeSlider.value = _audioManager.GetBackgroundVolumeSound();

            //GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<CharacterManager>().SetPlayerName();

            //_rectTransformTurnUI = turnUI.GetComponent<RectTransform>();

            _rectTransformFaithTab = faithTab.GetComponent<RectTransform>();
            FadeInElement(_rectTransformFaithTab, faithTabFadeInTime);
        }

        private void Start()
        {
            InventoryManager.Instance.SetUpSlots(inventorySlots, equipmentSlots, passiveSkillSlots);
            CharacterManager.Instance.LockPlayerMovement();
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
                    LeanTween.alpha(_rectTransformFaithTab, 0f, faithTabFadeOutTime).setEase(LeanTweenType.linear)
                        .setOnComplete(FaithTabAnimComplete);
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
            CharacterManager.Instance.UnlockPlayerMovement();
            InventoryManager.Instance.AddHealthKitContextMenu();
            InventoryManager.Instance.AddWeaponContextMenu();
        }

        public void SetFire()
        {
            SetFaithSprite(Element.Fire);
            var skill = ExchangeManager.Instance.CreateSkill("Fire Breath");
            var passive1 = ExchangeManager.Instance.CreateSkill("Strength increase");
            var passive2 = ExchangeManager.Instance.CreateSkill("Agility increase");
            InventoryManager.Instance.SpawnNewItem(skill, activeSkillSlot, 1);
            InventoryManager.Instance.SpawnNewItem(passive1, passiveSkillSlots[0], 1);
            InventoryManager.Instance.SpawnNewItem(passive2, passiveSkillSlots[1], 1);
            /*passiveSkillSlots[0].GetCurrentItem().image.color = Color.gray;
            passiveSkillSlots[1].GetCurrentItem().image.color = Color.gray;*/
            SetAllAttributeValues();
            CloseFaithTab();
            chosenFaith = Element.Fire;
        }

        public void SetAir()
        {
            SetFaithSprite(Element.Air);
            var skill = ExchangeManager.Instance.CreateSkill("Thunder Strike");
            var passive1 = ExchangeManager.Instance.CreateSkill("Dexterity increase");
            var passive2 = ExchangeManager.Instance.CreateSkill("Agility increase");
            InventoryManager.Instance.SpawnNewItem(skill, activeSkillSlot, 1);
            InventoryManager.Instance.SpawnNewItem(passive1, passiveSkillSlots[0], 1);
            InventoryManager.Instance.SpawnNewItem(passive2, passiveSkillSlots[1], 1);
            /*passiveSkillSlots[0].GetCurrentItem().image.color = Color.gray;
            passiveSkillSlots[1].GetCurrentItem().image.color = Color.gray;*/
            SetAllAttributeValues();
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

        /*private void FadeOutElement(RectTransform rectTransform, float fadeOutDuration)
        {
            LeanTween.alpha(rectTransform, 0f, fadeOutDuration).setEase(LeanTweenType.linear);
        }*/

        private void FaithTabAnimComplete()
        {
            faithTab.SetActive(false);
        }

        private void CombatUIAnimComplete()
        {
            combatUI.gameObject.SetActive(false);
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

        public void SetStrengthText(int value)
        {
            strengthText.text = "Strength: " + value;
        }

        public void SetAgilityText(int value)
        {
            agilityText.text = "Agility: " + value;
        }

        public void SetDexterityText(int value)
        {
            dexterityText.text = "Dexterity: " + value;
        }

        public void SetConstitutionText(int value)
        {
            constitutionText.text = "Constitution: " + value;
        }

        public void SetIntelligenceText(int value)
        {
            intelligenceText.text = "Intelligence: " + value;
        }

        public void SetAllAttributeValues()
        {
            SetStrengthText((int)CharacterManager.Instance.GetTotalStr());
            SetAgilityText((int)CharacterManager.Instance.GetTotalAgi());
            SetDexterityText((int)CharacterManager.Instance.GetTotalDex());
            SetConstitutionText((int)CharacterManager.Instance.GetTotalConst());
            SetIntelligenceText((int)CharacterManager.Instance.GetTotalInt());
        }

        public void SetBackgroundVolume(float value)
        {
            _audioManager.SetBackgroundVolume(value);
        }

        public void SetPlayerVolume(float value)
        {
            _audioManager.SetPlayerVolume(value);
        }

        public void SetEnemyVolume(float value)
        {
            _audioManager.SetEnemyVolume(value);
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

        public void SetWeaponRangeUI(int value)
        {
            //weaponRange.text = "Weapon range: " + value;
            _combatUI.SetWeaponRangeUI(value);
        }

        public void SetWeaponDamageUI(int value)
        {
            //weaponDamage.text = "Weapon damage: " + value;
            _combatUI.SetWeaponDamageUI(value);
        }

        public void SetAmmoCounter(int value)
        {
            //ammoCounter.text = "Ammo left: " + value;
            _combatUI.SetAmmoCounter(value);
        }

        public void SetCombatPopUp(bool state, string text = "")
        {
            _combatUI.SetCombatPopUp(state, text);
        }

        public CombatUI GetCombatUI()
        {
            return _combatUI;
        }

        public void ShowCombatUI()
        {
            combatUI.gameObject.SetActive(true);
        }

        public void HideCombatUI()
        {
            _combatUI.OnCombatEnd();
            combatUI.gameObject.SetActive(false);
        }

        #endregion

        public void ToggleCombatUI(List<PlayerGridMovement> characterList)
        {
            if (!combatUI.gameObject.activeSelf)
            {
                if (dungeonTutorial.activeSelf) dungeonTutorial.SetActive(false);
                combatUI.gameObject.SetActive(true);
                FadeInElement(combatUI, turnUIFadeInTime);
                _combatUI = combatUI.GetComponent<CombatUI>();
                _combatUI.SetUp(characterList);
            }
            else
            {
                //FadeOutElement(combatUI, turnUIFadeOutTime);
                LeanTween.alpha(combatUI, 0f, turnUIFadeOutTime).setEase(LeanTweenType.linear)
                    .setOnComplete(CombatUIAnimComplete);
                _combatUI.Destroy();
                /*delay = LeanTween.delayedCall(turnUIFadeOutTime, () =>
                {
                    combatUI.gameObject.SetActive(false);
                    //turnUI.DestroyList();
                });*/
            }
        }

        public void ToggleMoveAttackButton(string mode)
        {
            _combatUI.ToggleMoveAttackButton(mode);
        }

        public void SkillButtonAction(string mode)
        {
            _combatUI.SkillButtonAction(mode);
        }

        public void StartDungeonTutorial()
        {
            dungeonTutorial.SetActive(true);
        }

        public void ReturnToMainMenu()
        {
            SceneManager.instance.ReturnToMainMenu(NetworkManager.Singleton.IsHost);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}