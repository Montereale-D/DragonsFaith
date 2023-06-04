using System.Collections;
using Network;
using Player;
using Save;
using UnityEngine;
using UnityEngine.Events;

public class SpawnPointer : MonoBehaviour
{
    [SerializeField] private Transform spawnPointPlayer1;
    [SerializeField] private Transform spawnPointPlayer2;
    public UnityEvent onPlayersSpawned;

    private Vector3 _hostPos;
    private Vector3 _clientPos;
    private void Awake()
    {
        
    }

    private void Start()
    {
        Vector3? hostPos = null;
        Vector3? clientPos = null;

        if (DungeonProgressManager.instance != null)
        {
            hostPos = DungeonProgressManager.instance.GetSpawnPoint(GameData.PlayerType.Host);
            clientPos = DungeonProgressManager.instance.GetSpawnPoint(GameData.PlayerType.Client);
        }
        

        if (hostPos != null && clientPos != null)
        {
            _hostPos = (Vector3)hostPos;
            _clientPos = (Vector3)clientPos;
        }
        else
        {
            _hostPos = spawnPointPlayer1.position;
            _clientPos = spawnPointPlayer2.position;
        }
        
        foreach (var player in FindObjectsOfType<PlayerMovement>())
        {
            var position = player.IsHost ? _hostPos : _clientPos;
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
        return playerType == GameData.PlayerType.Host ? _hostPos : _clientPos;
    }
}
