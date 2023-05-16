using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/ConsumableWeapon")]
    public class ConsumableWeapon : Weapon
    {
        public ConsumableWeapon()
        {
            type = ItemType.Weapon;
            consumable = true;
            stackable = true;
        }

        public override void PerformAction()
        {
            base.PerformAction();
            
        }
    }
}