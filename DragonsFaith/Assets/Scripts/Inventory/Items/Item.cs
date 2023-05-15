using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inventory.Items
{
    public class Item : ScriptableObject
    {
        public string itemName = "item name";

        [Multiline] public string description = "item description";

        [Tooltip("Readme -> how to generate an id")]
        public string id;

        public ItemType type { get; protected set; }
        public bool consumable { get; protected set; }
        public bool stackable { get; protected set; }

        public TileBase tile;
        public Sprite image;

        public virtual void PerformAction(){}
        public override string ToString()
        {
            return itemName + " (" + id + "): [description: " + description + ", type: " + type + ", consumable: " +
                   consumable +
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
        All,
        Consumable,
        Weapon,
        Head,
        Chest,
        Legs,
        Skill,
    }
}