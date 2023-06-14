﻿using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class HubProgressManager : MonoBehaviour
{
    public static HubProgressManager instance { get; private set; }
    public static int keyCounter;
    public static bool firstTime = true;
    [SerializeField] private List<GameObject> notificationObjects;

    public UnityEvent onAllKeysCollect;

    private void Awake()
    {
        ResetNotification();

        if (!firstTime)
        {
            CharacterManager.Instance.Heal(CharacterManager.Instance.GetMaxHealth());
            CharacterManager.Instance.RestoreMana(CharacterManager.Instance.GetMaxMana());
        }

        firstTime = false;

        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void ResetNotification()
    {
        for (int i = 0; i < keyCounter; i++)
        {
            notificationObjects[i].GetComponent<SpriteRenderer>().color = Color.green;
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
            notificationObject.GetComponent<SpriteRenderer>().color = Color.red;
        }
        
        ResetNotification();
    }
}