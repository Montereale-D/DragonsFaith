using System;
using System.Collections;
using System.Runtime.Serialization;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Grid
{
    public class CharacterGridPopUpUI : MonoBehaviour
    {
        [Header("Character Stats")]
        public GameObject ui;
        public Slider healthBar;
        public TextMeshProUGUI healthNumber;
        public TextMeshProUGUI characterName;
        private int _maxHealth;
        public Slider manaBar;
        public TextMeshProUGUI manaNumber;
        private int _maxMana;



        [Header("Damage Counter")]
        public GameObject damageCounter;
        public TextMeshProUGUI damageCounterText;
        public GameObject coveredSymbol;
        public RectTransform startPoint, endPoint;
        public float speed = 1;
        private bool _isOpening;
        private float _snapToSizeDistance = 0.25f;

        public Animator bloodAnimator;
        [OptionalField] public Animator skillAnimator;
        private static readonly int Air = Animator.StringToHash("Air");
        private static readonly int Fire = Animator.StringToHash("Fire");

        [Header("Shield")] 
        public GameObject shield;

        public void SetUI(string charName, int maxHealth, int maxMana)
        {
            ShowUI();
            _maxHealth = maxHealth;
            healthBar.maxValue = maxHealth;
            UpdateHealth(maxHealth);
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
            manaBar.value = mana;
            manaNumber.text = "Mana: " + mana + "/" + _maxMana;
        }
    
        public void ShowUI()
        {
            ui.SetActive(true);
        }
    
        public void HideUI()
        {
            ui.SetActive(false);
        }

        public void ShowDamageCounter(int value, bool heal, bool isProtected)
        {
            damageCounter.SetActive(true);
            damageCounterText.transform.position = startPoint.position;
            damageCounterText.text = value.ToString();
            coveredSymbol.SetActive(isProtected);
            damageCounterText.color = heal ? Color.green : Color.red;
            _isOpening = true;
            if (!heal) AudioManager.instance.PlayPLayerHurtSound();
            //ShowBlood();
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

        public void ShowShield()
        {
            shield.SetActive(true);
        }

        public void HideShield()
        {
            shield.SetActive(false);
        }

        public void ShowBlood()
        {
            StartCoroutine(BloodAnimation());
        }

        private IEnumerator BloodAnimation()
        {
            bloodAnimator.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            bloodAnimator.gameObject.SetActive(false);
        }

        public void ShowSkillEffect(string skill)
        {
            StartCoroutine(SkillEffectAnimation(skill));
        }
        
        private IEnumerator SkillEffectAnimation(string skill)
        {
            if(!skillAnimator) yield break;
            
            skillAnimator.gameObject.SetActive(true);
            switch (skill)
            {
                case "Air":
                    skillAnimator.SetTrigger(Air);
                    break;
                case "Fire":
                    skillAnimator.SetTrigger(Fire);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            yield return new WaitForSeconds(0.6f);
            skillAnimator.gameObject.SetActive(false);
        }
    }
}
