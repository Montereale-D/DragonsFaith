using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inventory
{
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        public string itemName = "item name";
        public string id;
        public TileBase tile;
        public Sprite image;
        public ItemType type;
        public ActionType action;
        public bool consumable = true;
        public Vector2Int range = new Vector2Int(5, 4);
        public bool stackable = true;

        [ContextMenu("Generate guid")]
        private void GenerateGuid()
        {
            id = System.Guid.NewGuid().ToString();
        }
    }

    public enum ItemType
    {
        Items, Weapons, Armory
    }

    public enum ActionType
    {
        Skill, Melee, Ranged, Heal
    }
}