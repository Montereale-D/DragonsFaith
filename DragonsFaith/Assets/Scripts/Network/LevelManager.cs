using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Network
{
    public class LevelManager : NetworkBehaviour
    {
        public static LevelManager instance;
        private List<GameObject> _roomPool;
        [HideInInspector] public int mapIdx;
        private Transform[] _spawnPoints;
        private GameObject[] _hiddenAreas;

        private List<NetworkObject> _networkObjects  = new List<NetworkObject>();

        public override void OnNetworkSpawn()
        {
            /*if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;*/
            base.OnNetworkSpawn();
            _roomPool = new List<GameObject>();
            _roomPool.Clear();
            _roomPool.AddRange(Resources.LoadAll<GameObject>("Dungeons/"));
            
            if (!IsHost) return;
            SetUp();
            
            GetComponent<NetworkObject>().DestroyWithScene = true;
        }

        // Start is called before the first frame update
        private void SetUp()
        {
            mapIdx = Random.Range(0, _roomPool.Count);
            var map = Instantiate(_roomPool[mapIdx]);
            
            map.GetComponent<NetworkObject>().Spawn();
            //InstantiateMapClientRpc(mapIdx);

            var objs = map.GetComponentsInChildren<NetworkObject>();

            foreach (var obj in objs)
            {
                var children = obj.GetComponentsInChildren<NetworkObject>();
                //var position = obj.transform.position;
                if (children.Length > 0)
                {
                    foreach (var child in children)
                    { 
                        if (!child.IsSpawned) child.Spawn();
                    }
                }
                if (!obj.IsSpawned) obj.Spawn();
                //obj.transform.position = position;
                
                GetComponent<EnemySpawnPointer>().SetUp(map.GetComponent<DungeonController>().enemySpawnPoints);
            }

            /*_spawnPoints = map.GetComponent<DungeonController>().spawnPoints;
            _hiddenAreas = map.GetComponent<DungeonController>().hiddenAreas;

            for (var i = 0; i < _spawnPoints.Length; i++)
            {
                var hiddenArea = Instantiate(_hiddenAreas[i]); 
                //hiddenArea.transform.parent = map.transform;
                hiddenArea.transform.position = _spawnPoints[i].position;
                _networkObjects.Add(hiddenArea.GetComponent<NetworkObject>());
            }
            
            foreach (var netObj in _networkObjects)
            {
                netObj.GetComponent<NetworkObject>().Spawn();
            }*/
        }
    
        /*[ClientRpc]
        private void InstantiateMapClientRpc(int idx)
        {
            if (IsHost) return;
            
            Instantiate(_roomPool[idx]);
        }*/
        
        public override void OnDestroy()
        {
            Debug.Log("LevelManager OnDestroy");
            base.OnDestroy();
        }

        public override void OnNetworkDespawn()
        {
            Debug.Log("LevelManager OnNetworkDespawn");
            base.OnNetworkDespawn();
        }
    }
}
