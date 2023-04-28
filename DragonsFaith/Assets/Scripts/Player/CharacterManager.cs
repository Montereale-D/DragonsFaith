using Inventory;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class CharacterManager : NetworkBehaviour
    {
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private CharacterSO characterSo;
        [SerializeField] private GameObject playerPrefab;
        private GameObject _player;

        public override void OnNetworkSpawn()
        {
            if (IsHost)
            {
                SpawnPlayer(NetworkManager.Singleton.LocalClientId, true);
            }
            else
            {
                SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPlayerServerRpc(ulong clientId)
        {
            if (!IsHost) return;

            SpawnPlayer(clientId, false);
        }

        private void SpawnPlayer(ulong localId, bool isHost)
        {
            //todo assegnare gameobject host + client
            var newPlayer = Instantiate(playerPrefab);
            var netObj = newPlayer.GetComponent<NetworkObject>();
            
            newPlayer.SetActive(true);
            netObj.SpawnAsPlayerObject(localId, true);
        }
    }
}