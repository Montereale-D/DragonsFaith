using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterGridPopUpUI : MonoBehaviour
{
    [Header("Character Stats")]
    public GameObject ui;
    public Slider healthBar;
    public TextMeshProUGUI healthNumber;
    public TextMeshProUGUI characterName;
    private int _maxHealth;

    [Header("Damage Counter")]
    public GameObject damageCounter;
    public TextMeshProUGUI damageCounterText;
    public RectTransform startPoint, endPoint;
    public float speed = 1;
    private bool _isOpening;
    private float _snapToSizeDistance = 0.25f;

    public void SetUI(string charName, int maxHealth)
    {
        _maxHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        characterName.text = charName;
        HideUI();
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

    public void ShowDamageCounter(int value)
    {
        damageCounter.SetActive(true);
        damageCounterText.transform.position = startPoint.position;
        damageCounterText.text = value.ToString();
        _isOpening = true;
    }

    private void Update()
    {
        if (!_isOpening) return;
        //damageCounterText.transform.Translate(Vector3.up * Time.deltaTime * speed);
        damageCounterText.rectTransform.position = Vector3.MoveTowards(damageCounterText.rectTransform.position, 
            endPoint.position,Time.deltaTime*speed);
        if (!(Mathf.Abs(damageCounterText.transform.position.y - endPoint.position.y) < _snapToSizeDistance)) return;
        damageCounterText.transform.position = endPoint.position;
        _isOpening = false;
        damageCounter.SetActive(false);
    }
}
