using Grid;
using UI;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    private PlayerUI _playerUI;
    private CharacterGridPopUpUI _characterUI;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxMana = 100;

    [HideInInspector] public bool isLocalPlayer;
    public int health;

    public int mana { get; private set; }
    [HideInInspector] public string characterName;
    public PlayerUI.Element faith { get; private set; }

    public bool isBlocking;

    private PlayerGridMovement _gridMovement;

    private void Awake()
    {
        health = maxHealth;
        mana = maxMana;
        //characterName = PlayerPrefs.GetString("playerName");
        _characterUI = GetComponent<CharacterGridPopUpUI>();
        //_characterUI.SetUI(characterName, maxHealth);
        _gridMovement = GetComponent<PlayerGridMovement>();
    }

    public void SetUp()
    {
        if (isLocalPlayer)
        {
            _playerUI = PlayerUI.instance;
            _playerUI.healthSlider.maxValue = maxHealth;
            _playerUI.manaSlider.maxValue = maxMana;
            _playerUI.UpdateHealthBar(maxHealth, maxHealth);
            _playerUI.UpdateManaBar(maxMana, maxMana);
            characterName = _playerUI.nameText.text = PlayerPrefs.GetString("playerName");
            _characterUI.SetUI(characterName, maxHealth);
        }
        else
        {
            _characterUI = GetComponent<CharacterGridPopUpUI>();
            _characterUI.SetUI(characterName, maxHealth);
        }
    }

    public void UpdateMaxHealth(int value)
    {
        maxHealth += value;

        if (isLocalPlayer)
        {
            _playerUI.UpdateMaxHealth(value);
        }
    }

    public void UpdateMaxMana(int value)
    {
        if (isLocalPlayer)
        {
            maxMana = value;
            _playerUI.UpdateMaxMana(value);
        }
    }

    public void Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            health = 0;
        }

        if (isLocalPlayer)
        {
            _playerUI.UpdateHealthBar(health, maxHealth);
        }
        else
        {
            _characterUI.UpdateHealth(health);
        }
        
        if (!IsAlive())
        {
            Die();
        }
    }
    
    private void Die()
    {
        CombatSystem.instance.CharacterDied(_gridMovement);
        _gridMovement.OnDeath();
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public void Heal(int heal)
    {
        health += heal;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        if (isLocalPlayer)
        {
            _playerUI.UpdateHealthBar(health, maxHealth);
        }
        else
        {
            _characterUI.UpdateHealth(health);
            GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(heal, true);
        }
    }

    public void Revive()
    {
        Heal(maxHealth/2);
        _gridMovement.OnRevive();
    }

    public bool UseMana(int value)
    {
        if (mana - value < 0) return false;

        mana -= value;

        if (isLocalPlayer)
        {
            _playerUI.UpdateManaBar(mana, maxMana);
        }


        return true;
    }

    public void RestoreMana(int value)
    {
        mana += value;

        if (mana > maxMana)
        {
            mana = maxMana;
        }

        if (isLocalPlayer)
        {
            _playerUI.UpdateManaBar(mana, maxMana);
        }
    }

    public void LoadLocalPlayer(int health, int maxHealth, int mana, int maxMana, string characterName,
        PlayerUI.Element faith)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.mana = mana;
        this.maxMana = maxMana;
        this.characterName = characterName;
        this.faith = faith;

        _playerUI.UpdateHealthBar(this.health, this.maxHealth);
        _playerUI.UpdateManaBar(this.mana, this.maxMana);

        _playerUI.nameText.text = this.characterName;
        _playerUI.chosenFaith = this.faith;
    }

    public void OverrideSettings(int health, int maxHealth, int mana, int maxMana, string characterName)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.mana = mana;
        this.maxMana = maxMana;
        this.characterName = characterName;
        this.faith = faith;

        if (_characterUI)
        {
            _characterUI.SetUI(characterName, maxHealth);
        }
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetMaxMana()
    {
        return maxMana;
    }
}