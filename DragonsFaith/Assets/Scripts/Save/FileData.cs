using System;
using System.IO;
using UnityEngine;

namespace Save
{
    public class FileData
    {
        private readonly string _path;
        private readonly string _fileName;

        public FileData(string path, string fileName)
        {
            _path = path;
            _fileName = fileName;
        }

        public GameData Load()
        {
            var fullPath = Path.Combine(_path, _fileName);
            GameData loadData = null;
            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad;
                    using var stream = new FileStream(fullPath, FileMode.Open);
                    using var reader = new StreamReader(stream);
                    dataToLoad = reader.ReadToEnd();
                    loadData = JsonUtility.FromJson<GameData>(dataToLoad);

                }
                catch (Exception e)
                {
                    Debug.LogError("Error when trying to load from " + fullPath + "\n" + e);
                }
            }

            return loadData;
        }

        public void Save(GameData gameData)
        {
            var fullPath = Path.Combine(_path, _fileName);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException());
                var dataToJson = JsonUtility.ToJson(gameData, true);
                using var stream = new FileStream(fullPath, FileMode.Create);
                using var writer = new StreamWriter(stream);
                writer.Write(dataToJson);
            }
            catch (Exception e)
            {
                Debug.LogError("Error when trying to save in " + fullPath + "\n" + e);
            }
        }

        public void NewGame(GameData data)
        {
            Save(data);
        }
    }
}