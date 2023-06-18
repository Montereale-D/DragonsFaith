using System.Collections;
using System.Collections.Generic;
using Inventory;
using Network;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class HubProgressManager : MonoBehaviour
{
    public static HubProgressManager instance { get; private set; }
    public static int keyCounter;

    public Sprite obtainedSprite;
    public Sprite missingSprite;
    
    public static bool firstTime = true;
    [SerializeField] private List<GameObject> notificationObjects;

    public UnityEvent onAllKeysCollect;

    private void Awake()
    {
        ResetNotification();

        if (!SceneManager.instance.isFirstEntering)
        {
            var maxHealth = CharacterManager.Instance.GetMaxHealth();
            CharacterManager.Instance.Heal(maxHealth);
            ExchangeManager.Instance.NotifyHealToAnother(maxHealth);

            var maxMana = CharacterManager.Instance.GetMaxMana();
            CharacterManager.Instance.RestoreMana(maxMana);
        }

        SceneManager.instance.isFirstEntering = false;

        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void ResetNotification()
    {
        for (var i = 0; i < keyCounter; i++)
        {
            //notificationObjects[i].GetComponent<SpriteRenderer>().color = Color.green;
            notificationObjects[i].GetComponent<SpriteRenderer>().sprite = obtainedSprite;
        }

        if (keyCounter == notificationObjects.Count)
        {
            StartCoroutine(WaitAndNotify());
        }
    }

    private IEnumerator WaitAndNotify()
    {
        yield return new WaitForSecondsRealtime(2f);
        onAllKeysCollect.Invoke();
    }

    [ContextMenu("AddKey")]
    public void AddKeyDebug()
    {
        keyCounter++;

        foreach (var notificationObject in notificationObjects)
        {
            //notificationObject.GetComponent<SpriteRenderer>().color = Color.red;
            notificationObject.GetComponent<SpriteRenderer>().sprite = missingSprite;
        }
        
        ResetNotification();
    }
}