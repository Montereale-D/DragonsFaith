﻿using System.Collections;
using System.Collections.Generic;
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
            CharacterManager.Instance.Heal(CharacterManager.Instance.GetMaxHealth());
            CharacterManager.Instance.RestoreMana(CharacterManager.Instance.GetMaxMana());
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