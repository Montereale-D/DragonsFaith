namespace Save
{
    public interface IGameData
    {
        void LoadData(GameData data);
        void SaveData(ref GameData data);
    }
}