using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Save
{
    public class DataManager : MonoBehaviour
    {
        private GameData _gameData;
        private List<IGameData> _dataObjects;
        private const string FileName = "DragonsFaithData.json";
        private FileData _fileData;

        public static DataManager Instance { get; private set; }

        private void Awake()
        {
            if(Instance != null)
                Debug.LogError("More than one DataManager in the scene");

            Instance = this;
        }

        private void Start()
        {
            _dataObjects = FindDataObjects();
            _fileData = new FileData(Application.persistentDataPath, FileName);
        }

        private List<IGameData> FindDataObjects()
        {
            IEnumerable<IGameData> dataGameObjects = FindObjectsOfType<MonoBehaviour>().OfType<IGameData>();

            return new List<IGameData>(dataGameObjects);
        }

        public void NewGame()
        {
            _gameData = new GameData();
        }
        
        public void SaveGame()
        {
            if(_gameData == null)
                NewGame();
            
            foreach (var dataObject in _dataObjects)
            {
                dataObject.SaveData(ref _gameData);
            }
            
            Debug.Log("Print data before save: ");
            foreach (var item in _gameData.GetAllItemsData().ToArray())
            {
                Debug.Log(item);
            }
            
            _fileData.Save(_gameData);
        }
        public void LoadGame()
        {
            _gameData = _fileData.Load();
            
            if (_gameData == null)
            {
                Debug.Log("No data found. Start new game");
                NewGame();
            }

            foreach (var dataObject in _dataObjects)
            {
                dataObject.LoadData(_gameData);
            }
        }
    }
}
