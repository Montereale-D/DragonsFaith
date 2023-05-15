using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/Consumable")]
    public class Consumable : Item
    {
        public ConsumableType consumableType;

        public Consumable()
        {
            type = ItemType.Consumable;
            consumable = true;
            stackable = true;
        }
        public enum ConsumableType
        {
            PotionHealing,
            PotionMana
        }

        public override void PerformAction()
        {
            base.PerformAction();
        }
    }
}