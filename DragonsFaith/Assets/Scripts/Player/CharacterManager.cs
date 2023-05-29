using System;
using Inventory;
using Save;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class CharacterManager : MonoBehaviour, IGameData
    {
        [SerializeField] private CharacterSO characterSo;

        private PlayerUI _playerUI;

        private int _maxHealth = 100;
        private int _health = 100;
        private int _maxMana = 100;
        private int _mana = 100;

        public static CharacterManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);

            characterSo.Reset();

            _health = _maxHealth;
            _mana = _maxMana;
        }


        private void Start()
        {
            _playerUI = PlayerUI.Instance;
            _playerUI.healthSlider.maxValue = _maxHealth;
            _playerUI.manaSlider.maxValue = _maxMana;
            _playerUI.UpdateHealthBar(_maxHealth, _maxHealth);
            _playerUI.UpdateManaBar(_maxMana, _maxMana);
            _playerUI.nameText.text = PlayerPrefs.GetString("playerName");
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
        public void DamageFromContext()
        {
            Damage(20);
        }

        public void Damage(int damage)
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
            var playerScore = abilityAttribute.attribute switch
            {
                AttributeType.Strength => GetTotalStr(),
                AttributeType.Intelligence => GetTotalInt(),
                AttributeType.Agility => GetTotalAgi(),
                AttributeType.Constitution => GetTotalConst(),
                AttributeType.Dexterity => GetTotalDex(),
                _ => throw new ArgumentOutOfRangeException()
            };

            return playerScore > (int)abilityAttribute.score;
        }

        public float GetTotalStr()
        {
            var score = (int)characterSo.GetAttributeScore(AttributeType.Strength);
            var modifiers = InventoryManager.Instance.GetEquipmentModifiers(AttributeType.Strength);
            Debug.Log("Strength " + score + " x " + modifiers);
            return score * modifiers;
        }

        public float GetTotalDex()
        {
            var score = (int)characterSo.GetAttributeScore(AttributeType.Dexterity);
            var modifiers = InventoryManager.Instance.GetEquipmentModifiers(AttributeType.Dexterity);
            Debug.Log("Dexterity " + score + " x " + modifiers);
            return score * modifiers;
        }

        public float GetTotalInt()
        {
            var score = (int)characterSo.GetAttributeScore(AttributeType.Intelligence);
            var modifiers = InventoryManager.Instance.GetEquipmentModifiers(AttributeType.Intelligence);
            Debug.Log("Intelligence " + score + " x " + modifiers);
            return score * modifiers;
        }

        public float GetTotalConst()
        {
            var score = (int)characterSo.GetAttributeScore(AttributeType.Constitution);
            var modifiers = InventoryManager.Instance.GetEquipmentModifiers(AttributeType.Constitution);
            Debug.Log("Constitution " + score + " x " + modifiers);
            return score * modifiers;
        }

        public float GetTotalAgi()
        {
            var score = (int)characterSo.GetAttributeScore(AttributeType.Agility);
            var modifiers = InventoryManager.Instance.GetEquipmentModifiers(AttributeType.Agility);
            Debug.Log("Agility " + score + " x " + modifiers);
            return score * modifiers;
        }


        public void LoadData(GameData data)
        {
            var playerType = GameData.GetPlayerType();

            Debug.Log("CharacterManager load data " + playerType);

            data.GetBarsData(playerType, out _health, out _maxHealth, out _mana, out _maxMana);
            _playerUI.UpdateHealthBar(_health, _maxHealth);
            _playerUI.UpdateManaBar(_mana, _maxMana);

            _playerUI.nameText.text = data.GetPlayerName(playerType);
            _playerUI.chosenFaith = data.GetFaith(playerType);
        }

        public void SaveData(ref GameData data)
        {
            var playerType = GameData.GetPlayerType();

            data.UpdateBarsData(playerType, _health, _maxHealth, _mana, _maxMana);

            data.SetPlayerName(playerType, _playerUI.nameText.text);
            data.SetFaith(playerType, _playerUI.chosenFaith);
        }

        public void SetPlayerGridMode()
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            localPlayer.GetComponent<PlayerMovement>().enabled = false;
            localPlayer.GetComponent<CameraFindPlayer>().enabled = false;
            localPlayer.GetComponent<BoxCollider2D>().enabled = false;
            localPlayer.GetComponent<PlayerGridMovement>().enabled = true;
        }

        public void SetPlayerFreeMode()
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            localPlayer.GetComponent<PlayerMovement>().enabled = true;
            localPlayer.GetComponent<CameraFindPlayer>().enabled = true;
            localPlayer.GetComponent<BoxCollider2D>().enabled = true;
            localPlayer.GetComponent<PlayerGridMovement>().enabled = false;
        }
    }
}