using System.Collections.Generic;
using Grid;
using Inventory;
using Inventory.Items;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Unity.Netcode;

namespace UI
{
    public class CombatUI : MonoBehaviour
    {
        [Header("Actions")]
        //public RectTransform combatUI;
        public Button moveOrAttackButton;
        public Button skillButton;
        public Button blockButton;
        public Button reloadButton;
        public Button itemsButton;
        public Button skipTurnButton;
        
        [Header("Sprites and Text")]
        public Sprite attackSprite;
        public Sprite moveSprite;
        public TextMeshProUGUI weaponRange;
        public TextMeshProUGUI weaponDamage;
        public TextMeshProUGUI ammoCounter;
        
        [Header("PopUp")]
        public GameObject combatPopUp;
        public GameObject equippedItemsTab;
        
        private UnityAction _moveOrAttackAction;
        private Image _moveOrAttackImage;
        private TextMeshProUGUI _moveOrAttackText;
        private TurnUI _turnUI;

        public void SetUp(List<PlayerGridMovement> characterList)
        {
            var weapon = InventoryManager.Instance.GetWeapon();
            if (!weapon)
            {
                weapon = ScriptableObject.CreateInstance<Weapon>();
                weapon.range = 1;
                weapon.weaponType = Weapon.WeaponType.Melee;
                weapon.damage = 1;
            }

            SetWeaponRangeUI(weapon.range);
            SetWeaponDamageUI((int)weapon.damage);
            if (weapon.weaponType == Weapon.WeaponType.Melee) SetMeleeAmmoCounter();
            else SetAmmoCounter(weapon.GetAmmo());
                
            _turnUI = GetComponentInChildren<TurnUI>();
            _turnUI.SetUpList(characterList);

            _moveOrAttackText = moveOrAttackButton.GetComponentInChildren<TextMeshProUGUI>();
            var images = moveOrAttackButton.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.gameObject.GetInstanceID() != moveOrAttackButton.GetInstanceID())
                    _moveOrAttackImage = img;
            }
                
            //skillButton.onClick.AddListener(CombatSystem.instance.CheckSkillAttack);
            SkillButtonAction("Show");
            blockButton.onClick.AddListener(CombatSystem.instance.ButtonBlockAction);
            reloadButton.onClick.AddListener(CombatSystem.instance.ButtonReloadAction);
            itemsButton.onClick.AddListener(SetItemsTab);
            skipTurnButton.onClick.AddListener(CombatSystem.instance.ButtonSkipTurn);
            /*{
                var localPlayer =
                    NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerGridMovement>();
                if (CombatSystem.instance.activeUnit == localPlayer)
                {
                    CombatSystem.instance.SkipTurn();
                }
            });*/
        }

        public void Destroy()
        {
            moveOrAttackButton.onClick.RemoveAllListeners();
            skillButton.onClick.RemoveAllListeners();
            blockButton.onClick.RemoveAllListeners();
            reloadButton.onClick.RemoveAllListeners();
            itemsButton.onClick.RemoveAllListeners();
            skipTurnButton.onClick.RemoveAllListeners();
            _turnUI.DestroyList();
        }

        public void OnCombatEnd()
        {
            _turnUI.isCombatEnd = true;
        }

        private void SetItemsTab()
        {
            equippedItemsTab.SetActive(!equippedItemsTab.activeSelf);
        }
        
        public void SetWeaponRangeUI(int value)
        {
            weaponRange.text = "Weapon range: " + value;
        }
        
        public void SetWeaponDamageUI(int value)
        {
            weaponDamage.text = "Weapon damage: " + value;
        }

        public void SetAmmoCounter(int value)
        {
            ammoCounter.text = "Ammo left: " + value;
        }

        public void SetMeleeAmmoCounter()
        {
            ammoCounter.text = "No ammo needed";
        }
        
        public void SetCombatPopUp(bool state, string text = "")
        {
            if (state)
            {
                if (!combatPopUp.activeSelf) combatPopUp.SetActive(true);
                combatPopUp.GetComponentInChildren<TextMeshProUGUI>().text = text;
            }
            else combatPopUp.SetActive(false);
        }

        public TurnUI GetTurnUI()
        {
            return _turnUI;
        }

        public void SkillButtonAction(string mode)
        {
            //var aoe = new List<Tile>();
            switch (mode)
            { 
                case "Unleash":
                    skillButton.onClick.RemoveAllListeners();
                    skillButton.GetComponent<TooltipTrigger>().header = "Unleash";
                    skillButton.GetComponent<TooltipTrigger>().content = "Unleash the skill.";
                    skillButton.onClick.AddListener(CombatSystem.instance.CheckSkillAttack);
                    break;
                case "Show":
                    skillButton.onClick.RemoveAllListeners();
                    skillButton.GetComponent<TooltipTrigger>().header = "Show";
                    skillButton.GetComponent<TooltipTrigger>().content = "Show the skill's area of effect." +
                                                                         "Range is shown in the direction of the selected cell.";
                    skillButton.onClick.AddListener(CombatSystem.instance.CheckSkillRange);
                    break;
                case "Hide":
                    skillButton.onClick.RemoveAllListeners();
                    skillButton.GetComponent<TooltipTrigger>().header = "Hide";
                    skillButton.GetComponent<TooltipTrigger>().content = "Hide the skill's area of effect.";
                    skillButton.onClick.AddListener(CombatSystem.instance.HideSkillRange);
                    break;
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
                    moveOrAttackButton.GetComponent<TooltipTrigger>().header = "Attack";
                    moveOrAttackButton.GetComponent<TooltipTrigger>().content = "Attack the selected enemy.";
                    moveOrAttackButton.onClick.AddListener(CombatSystem.instance.ButtonAttackAction);
                    break;
                case "Move":
                    moveOrAttackButton.onClick.RemoveAllListeners();
                    _moveOrAttackImage.sprite = moveSprite;
                    _moveOrAttackText.text = "Move";
                    moveOrAttackButton.GetComponent<TooltipTrigger>().header = "Move";
                    moveOrAttackButton.GetComponent<TooltipTrigger>().content = "Move to the selected cell.";
                    moveOrAttackButton.onClick.AddListener(CombatSystem.instance.ButtonCheckMovement);
                    break;
                case "Destroy":
                    moveOrAttackButton.onClick.RemoveAllListeners();
                    _moveOrAttackImage.sprite = attackSprite;
                    _moveOrAttackText.text = "Destroy";
                    moveOrAttackButton.GetComponent<TooltipTrigger>().header = "Destroy";
                    moveOrAttackButton.GetComponent<TooltipTrigger>().content = "Destroy the selected obstacle";
                    moveOrAttackButton.onClick.AddListener(CombatSystem.instance.ButtonDestroyAction);
                    break;
            }
        }
    }
}
