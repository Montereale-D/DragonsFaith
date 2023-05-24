using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Save;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SpawnPointer : MonoBehaviour
{
    [SerializeField] private Transform spawnPointPlayer1;
    [SerializeField] private Transform spawnPointPlayer2;
    public UnityEvent onPlayersSpawned;

    private void Start()
    {
        foreach (var player in FindObjectsOfType<PlayerMovement>())
        {
            var position = player.IsHost ? spawnPointPlayer1.position : spawnPointPlayer2.position;
            player.ForcePosition(position);
        }

        //if (NetworkManager.Singleton.IsHost)
        {
            StartCoroutine(Notify());
        }
            
    }

    private IEnumerator Notify()
    {
        yield return new WaitForSecondsRealtime(1);
        onPlayersSpawned.Invoke();
    }

    public Vector3 GetSpawnPoint(GameData.PlayerType playerType)
    {
        return playerType == GameData.PlayerType.Host ? spawnPointPlayer1.position : spawnPointPlayer2.position;
    }
}
