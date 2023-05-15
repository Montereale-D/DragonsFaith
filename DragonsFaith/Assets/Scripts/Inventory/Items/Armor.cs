using UnityEngine;

namespace Inventory.Items
{
    public class Armor : Item
    {
        [Header("Equipment section")]
        public float Str;
        public float Dex;
        public float Int;
        public float Const;
        public float Agi;

        public Armor()
        {
            consumable = false;
            stackable = false;
        }
    }
}