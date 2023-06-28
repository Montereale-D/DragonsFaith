namespace Save
{
    /// <summary>
    /// Must use this interface if you want to save state in the file data
    /// </summary>
    public interface IGameData
    {
        void LoadData(GameData data);
        void SaveData(ref GameData data);
    }
}