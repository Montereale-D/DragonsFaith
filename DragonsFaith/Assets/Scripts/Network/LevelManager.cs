using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Network
{
    public class LevelManager : NetworkBehaviour
    {
        public static LevelManager instance;
        private List<GameObject> _roomPool;
        public int mapIdx;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            
            _roomPool = new List<GameObject>();
            _roomPool.Clear();
            _roomPool.AddRange(Resources.LoadAll<GameObject>("Dungeons/"));
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (!IsHost) return;
            
            mapIdx = Random.Range(0, _roomPool.Count);
            Instantiate(_roomPool[mapIdx]);
            
            InstantiateMapClientRpc(mapIdx);
        }
    
        [ClientRpc]
        private void InstantiateMapClientRpc(int idx)
        {
            if (IsHost) return;
            
            Instantiate(_roomPool[idx]);
        }
    }
}
