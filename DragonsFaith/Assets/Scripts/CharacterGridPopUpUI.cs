using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterGridPopUpUI : MonoBehaviour
{
    public GameObject ui;
    public Slider healthBar;
    public TextMeshProUGUI healthNumber;
    public TextMeshProUGUI characterName;
    private int _maxHealth;

    public void SetUI(string charName, int maxHealth)
    {
        HideUI();
        _maxHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        this.characterName.text = charName;
    }

    public void UpdateHealth(int health)
    {
        healthBar.value = health;
        healthNumber.text = "Life: " + health + "/" + _maxHealth;
    }
    
    public void UpdateMana(int mana)
    {
        //nothing
    }
    
    public void ShowUI()
    {
        ui.SetActive(true);
    }
    
    public void HideUI()
    {
        ui.SetActive(false);
    }
}
