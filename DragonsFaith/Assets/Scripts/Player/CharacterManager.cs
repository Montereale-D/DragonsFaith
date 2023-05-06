using Inventory;
using Save;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class CharacterManager : NetworkBehaviour, IGameData
    {
        [SerializeField] private CharacterSO characterSo;
        [SerializeField] private GameObject playerGameObject;

        private InventoryManager _inventoryManager;
        private PlayerUI _playerUI;

        private int _maxHealth = 100;
        private int _health = 100;
        private int _maxMana = 100;
        private int _mana = 100;


        private void Awake()
        {
            _inventoryManager = GetComponentInChildren<InventoryManager>();
            //_inventoryManager = InventoryManager.Instance;
            _inventoryManager.onSlotUseEvent.AddListener(Heal);
            //TODO: CharacterManager is awoken before playerUI, so no inventoryManager is available at this stage

            _playerUI = PlayerUI.Instance;

            characterSo.Reset();

            _health = _maxHealth;
            _mana = _maxMana;

            _playerUI.healthSlider.maxValue = _maxHealth;
            _playerUI.manaSlider.maxValue = _maxMana;
            _playerUI.UpdateHealthBar(_maxHealth, _maxHealth);
            _playerUI.UpdateManaBar(_maxMana, _maxMana);
        }

        [ContextMenu("Increase Max Health")]
        public void IncreaseMaxHealth()
        {
            UpdateMaxHealth(_maxHealth + 20);
        }

        public void UpdateMaxHealth(int value)
        {
            _maxHealth = value;
            _playerUI.UpdateMaxHealth(value);
        }

        public void UpdateMaxMana(int value)
        {
            _maxMana = value;
            _playerUI.UpdateMaxMana(value);
        }

        [ContextMenu("Take Damage")]
        public void Damage()
        {
            TakeDamage(20);
        }

        public void TakeDamage(int damage)
        {
            _health -= damage;

            if (_health <= 0)
            {
                _health = 0;

                //do stuff
            }

            _playerUI.UpdateHealthBar(_health, _maxHealth);
        }

        public void Heal(int heal)
        {
            _health += heal;

            if (_health > _maxHealth)
            {
                _health = _maxHealth;
            }

            _playerUI.UpdateHealthBar(_health, _maxHealth);
        }

        public void Heal(InventoryItem item)
        {
            Heal(10);
        }

        public bool UseMana(int value)
        {
            if (_mana - value < 0) return false;

            _mana -= value;
            _playerUI.UpdateManaBar(_mana, _maxMana);
            return true;
        }

        public void RestoreMana(int value)
        {
            _mana += value;

            if (_mana > _maxMana)
            {
                _mana = _maxMana;
            }

            _playerUI.UpdateManaBar(_mana, _maxMana);
        }

        public bool AbilityCheck(Attribute abilityAttribute)
        {
            var playerScore = characterSo.GetAttributeScore(abilityAttribute.attribute);

            Debug.Log("AbilityCheck " + abilityAttribute.attribute + " : ME = " + playerScore + " REQUIRED = " +
                      abilityAttribute.score);

            return playerScore > abilityAttribute.score;
        }


        public void LoadData(GameData data)
        {
            data.GetBarsData(GameData.GetPlayerType(IsHost), out _health, out _maxHealth, out _mana, out _maxMana);
            _playerUI.UpdateHealthBar(_health, _maxHealth);
            _playerUI.UpdateManaBar(_mana, _maxMana);
        }

        public void SaveData(ref GameData data)
        {
            data.UpdateBarsData(GameData.GetPlayerType(IsHost), _health, _maxHealth, _mana, _maxMana);
        }
    }
}