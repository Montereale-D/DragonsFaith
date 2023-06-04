using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using Save;
using UnityEngine;
using UnityEngine.Events;

public class SpawnPointerGrid : MonoBehaviour
{
    public static SpawnPointerGrid instance { get; private set; }
    [SerializeField] private Vector2Int spawnPointPlayer1;
    [SerializeField] private Vector2Int spawnPointPlayer2;
    [SerializeField] private List<Vector2Int> spawnPointEnemies;
    public UnityEvent onPlayersSpawned;

    private List<PlayerMovement> _players;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }
    private void Start()
    {
        _players = FindObjectsOfType<PlayerMovement>().ToList();
        foreach (var player in _players)
        {
            var position = player.IsHost ? spawnPointPlayer1 : spawnPointPlayer2;
            player.ForcePosition(new Vector3(position.x, position.y, 0));
        }

        //if (NetworkManager.Singleton.IsHost)
        {
            StartCoroutine(Notify());
        }
            
    }

    private IEnumerator Notify()
    {
        yield return new WaitForSecondsRealtime(1);

        GameHandler.instance.Setup();
        onPlayersSpawned.Invoke();
    }

    public Vector2Int GetPlayerSpawnPoint(GameData.PlayerType playerType)
    {
        return playerType == GameData.PlayerType.Host ? spawnPointPlayer1 : spawnPointPlayer2;
    }

    public List<Vector2Int> GetEnemySpawnPoint()
    {
        return spawnPointEnemies;
    }
}