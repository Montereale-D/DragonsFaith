using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inventory
{
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        public string itemName = "item name";

        [Tooltip("Readme -> how to generate an id")]
        public string id;

        public ItemType type;
        public ActionType action;
        public bool consumable = true;
        public bool stackable = true;

        public TileBase tile;
        public Sprite image;

        public override string ToString()
        {
            return itemName + " (" + id + "): [type: " + type + ", action: " + action + ", consumable: " + consumable +
                   ", stackable: " + stackable + "]";
        }

        [ContextMenu("Generate guid")]
        private void GenerateGuid()
        {
            id = System.Guid.NewGuid().ToString();
        }
    }

    public enum ItemType
    {
        Items,
        Weapons,
        Armory
    }

    public enum ActionType
    {
        Skill,
        Melee,
        Ranged,
        Heal
    }
}