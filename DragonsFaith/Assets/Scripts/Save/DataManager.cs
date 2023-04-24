using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

namespace Save
{
    public class DataManager : NetworkBehaviour
    {
        private GameData _gameData;
        private List<IGameData> _dataObjects;
        private const string FileName = "DragonsFaithData.json";
        private FileData _fileData;

        public static DataManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
                Debug.LogError("More than one DataManager in the scene");

            Instance = this;
        }

        private void Start()
        {
            _dataObjects = FindDataObjects();
            //if (IsHost) 
            _fileData = new FileData(Application.persistentDataPath, FileName);
        }

        private List<IGameData> FindDataObjects()
        {
            IEnumerable<IGameData> dataGameObjects = FindObjectsOfType<MonoBehaviour>().OfType<IGameData>();

            return new List<IGameData>(dataGameObjects);
        }

        public void NewGameRequest()
        {
            if (!IsHost) return;
            Debug.Log("New Game request");
            NewGame();
        }

        public void SaveGameRequest()
        {
            if (!IsHost) return;
            Debug.Log("Save Game request");
            SaveGame();
        }

        public void LoadGameRequest()
        {
            if (!IsHost) return;
            Debug.Log("Load Game request");
            LoadGame();
        }

        private void NewGame()
        {
            _gameData = new GameData();

            if (IsHost)
                _fileData.NewGame(_gameData);
        }

        private void SaveGame()
        {
            if (_gameData == null)
                _gameData = new GameData();

            foreach (var dataObject in _dataObjects)
            {
                dataObject.SaveData(ref _gameData);
            }

            if (IsHost)
            {
                _fileData.Save(_gameData);
                SaveDataClientRpc();
            }
            else
                SavePlayerDataServerRpc(_gameData);
        }

        private void LoadGame()
        {
            if (!IsHost) return;

            _gameData = _fileData.Load();

            if (_gameData == null)
            {
                Debug.Log("No data found. Start new game");
                NewGame();
            }

            LoadDataClientRpc(_gameData);
            
            foreach (var dataObject in _dataObjects)
            {
                dataObject.LoadData(_gameData);
            }
        }

        [ClientRpc]
        private void SaveDataClientRpc()
        {
            if (!IsHost)
            {
                Debug.Log("[CLIENT RPC] Save data request from server");
                SaveGame();
            }
        }
        
        [ClientRpc]
        private void LoadDataClientRpc(GameData gameData)
        {
            if (!IsHost)
            {
                Debug.Log("[CLIENT RPC] Load data request from server");
                _gameData = gameData;
                
                foreach (var dataObject in _dataObjects)
                {
                    dataObject.LoadData(_gameData);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SavePlayerDataServerRpc(GameData clientData)
        {
            if (!IsHost) return;
            
            Debug.Log("[SERVER RPC] Save data request from client: \n" + clientData.ClientData);
            
            _gameData.UpdateInventoryData(GameData.PlayerType.Client,
                clientData.GetAllItemsData(GameData.PlayerType.Client));
            _fileData.Save(_gameData);
        }
    }
}