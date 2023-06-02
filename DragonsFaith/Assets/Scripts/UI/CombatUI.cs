using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Unity.Netcode;
using UnityEngine.Serialization;

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
            //TODO: uncomment when player weapons
            /*SetWeaponRangeUI((int)InventoryManager.Instance.GetWeapon().GetRange());
            SetWeaponDamageUI((int)InventoryManager.Instance.GetWeapon().GetDamage());
            SetAmmoCounter(InventoryManager.Instance.GetWeapon().GetAmmo());*/
                
            _turnUI = GetComponentInChildren<TurnUI>();
            /*Debug.Log("spriteIdx=" + spriteIdx);
            for (int i = 0; i < portraitSprites.Length; i++)
            {
                Debug.Log(portraitSprites[i].name);
            }
            var otherPlayerSprite = portraitSprites[spriteIdx];*/
                
            _turnUI.SetUpList(characterList);

            _moveOrAttackText = moveOrAttackButton.GetComponentInChildren<TextMeshProUGUI>();
            var imgs = moveOrAttackButton.GetComponentsInChildren<Image>();
            foreach (var img in imgs)
            {
                if (img.gameObject.GetInstanceID() != moveOrAttackButton.GetInstanceID())
                    _moveOrAttackImage = img;
            }
                
            //skillButton.onClick.AddListener(add use skill function);
            blockButton.onClick.AddListener(CombatSystem.instance.BlockAction);
            reloadButton.onClick.AddListener(CombatSystem.instance.ReloadAction);
            itemsButton.onClick.AddListener(SetItemsTab);
            skipTurnButton.onClick.AddListener(() =>
            {
                var localPlayer =
                    NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerGridMovement>();
                if (CombatSystem.instance._activeUnit == localPlayer)
                {
                    CombatSystem.instance.SkipTurn();
                }
            });
        }

        public void Destroy()
        {
            moveOrAttackButton.onClick.RemoveAllListeners();
            //skillButton.onClick.RemoveAllListeners();
            blockButton.onClick.RemoveAllListeners();
            reloadButton.onClick.RemoveAllListeners();
            itemsButton.onClick.RemoveAllListeners();
            skipTurnButton.onClick.RemoveAllListeners();
            _turnUI.DestroyList();
        }

        public void SetItemsTab()
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
                    moveOrAttackButton.onClick.AddListener(CombatSystem.instance.ButtonCheckAction);
                    break;
                case "Move":
                    moveOrAttackButton.onClick.RemoveAllListeners();
                    _moveOrAttackImage.sprite = moveSprite;
                    _moveOrAttackText.text = "Move";
                    moveOrAttackButton.GetComponent<TooltipTrigger>().header = "Move";
                    moveOrAttackButton.GetComponent<TooltipTrigger>().content = "Move to the selected cell.";
                    moveOrAttackButton.onClick.AddListener(CombatSystem.instance.ButtonCheckMovement);
                    break;
            }
        }
    }
}
