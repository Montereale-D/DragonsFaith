using System;
using Grid;
using Inventory;
using Save;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class CharacterManager : MonoBehaviour, IGameData
    {
        [SerializeField] private CharacterSO characterSo;
        
        private CharacterInfo _characterInfo;

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
            _characterInfo = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CharacterInfo>();
            _characterInfo.isLocalPlayer = true;
        }

        private void Start()
        {
            _characterInfo.SetUp();
        }

        public void Heal(int value)
        {
            Debug.Log("Heal");
            _characterInfo.Heal(value);
        }
        public void RestoreMana(int value)
        {
            _characterInfo.RestoreMana(value);
        }

        public void GiveRevive()
        {
            CombatSystem.instance.PlayerReviveAction();
            CombatSystem.instance.NotifyRevive();
        }

        public void ReceiveRevive()
        {
            _characterInfo.Revive();
        }
        
        public int GetMaxHealth()
        {
            return _characterInfo.GetMaxHealth();
        }

        public int GetMaxMana()
        {
            return _characterInfo.GetMaxMana();
        }

        public void Damage(int value)
        {
            _characterInfo.Damage(value);
        }

        public bool IsAlive()
        {
            return _characterInfo.IsAlive();
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

            data.GetBarsData(playerType, out int health, out int maxHealth, out int mana, out int maxMana);
            _characterInfo.LoadLocalPlayer(health, maxHealth, mana, maxMana, data.GetPlayerName(playerType), data.GetFaith(playerType));
        }

        public void SaveData(ref GameData data)
        {
            var playerType = GameData.GetPlayerType();

            data.UpdateBarsData(playerType, _characterInfo.health, _characterInfo.GetMaxHealth(), _characterInfo.mana, _characterInfo.GetMaxMana());

            data.SetPlayerName(playerType, _characterInfo.characterName);
            data.SetFaith(playerType, _characterInfo.faith);
        }

        public void SetPlayerGridMode()
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            localPlayer.GetComponent<PlayerMovement>().InterruptAnimations();
            localPlayer.GetComponent<PlayerMovement>().enabled = false;
            localPlayer.GetComponent<CameraFindPlayer>().enabled = false;
            localPlayer.GetComponent<BoxCollider2D>().enabled = false;
            localPlayer.GetComponent<PlayerGridMovement>().enabled = true;
            /*//TODO: the weapon is spawning only for host and can't be seen
            localPlayer.GetComponentInChildren<WeaponHolder>().SetUpWeapon();*/
        }

        public void SetPlayerFreeMode()
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            localPlayer.GetComponent<PlayerMovement>().enabled = true;
            localPlayer.GetComponent<CameraFindPlayer>().enabled = true;
            localPlayer.GetComponent<BoxCollider2D>().enabled = true;
            localPlayer.GetComponent<PlayerGridMovement>().enabled = false;
            /*//TODO: test
            localPlayer.GetComponentInChildren<WeaponHolder>().DestroyWeapon();*/
            
        }
        
        [ContextMenu("Increase Max Health")]
        public void IncreaseMaxHealth()
        {
            _characterInfo.UpdateMaxHealth(20);
        }
        

        [ContextMenu("Take Damage")]
        public void DamageFromContext()
        {
            _characterInfo.Damage(20);
        }

        public string GetCharName()
        {
            return _characterInfo.characterName;
        }
    }
}