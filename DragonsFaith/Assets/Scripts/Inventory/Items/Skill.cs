using UnityEngine;

namespace Inventory.Items
{
    public class Skill : Item
    {
        public Skill()
        {
            type = ItemType.Skill;
            consumable = false;
            stackable = false;
        }
    }
}