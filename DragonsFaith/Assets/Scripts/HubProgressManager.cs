using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class HubProgressManager : NetworkBehaviour
{
    public static HubProgressManager instance { get; private set; }
    public static int keyCounter;

    public Sprite obtainedSprite;
    public Sprite missingSprite;
    
    [SerializeField] private List<GameObject> notificationObjects;

    public UnityEvent onAllKeysCollect;

    private void Awake()
    {
        ResetNotification();
        
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        //DontDestroyOnLoad(this);
    }

    private void ResetNotification()
    {
        for (var i = 0; i < keyCounter; i++)
        {
            //notificationObjects[i].GetComponent<SpriteRenderer>().color = Color.green;
            notificationObjects[i].GetComponent<SpriteRenderer>().sprite = obtainedSprite;
        }
        
        //TODO: uncomment to sync sprites for both host and client
        //SyncSprites(keyCounter);

        if (keyCounter == notificationObjects.Count)
        {
            StartCoroutine(WaitAndNotify());
        }
    }

    private void SyncSprites(int keyCounter)
    {
        if (IsHost)
            SyncSpritesClientRpc(keyCounter);
        else
            SyncSpritesServerRpc(keyCounter);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncSpritesServerRpc(int keyCounter)
    {
        for (var i = 0; i < keyCounter; i++)
        {
            notificationObjects[i].GetComponent<SpriteRenderer>().sprite = obtainedSprite;
        }
        
        if (keyCounter == notificationObjects.Count)
        {
            StartCoroutine(WaitAndNotify());
        }
    }

    [ClientRpc]
    private void SyncSpritesClientRpc(int keyCounter)
    {
        for (var i = 0; i < keyCounter; i++)
        {
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