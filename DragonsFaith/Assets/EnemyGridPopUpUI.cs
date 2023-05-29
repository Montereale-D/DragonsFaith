using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyGridPopUpUI : MonoBehaviour
{
    public GameObject ui;
    public Slider healthBar;
    public TextMeshProUGUI healthNumber;
    private int maxHealth;

    public void SetUI(int maxHealth)
    {
        HideUI();
        this.maxHealth = maxHealth;
        healthBar.maxValue = maxHealth;
    }

    public void UpdateUI(int health)
    {
        healthBar.value = health;
        healthNumber.text = "Life: " + health + "/" + maxHealth;
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
