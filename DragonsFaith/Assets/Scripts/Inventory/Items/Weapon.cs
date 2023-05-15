using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/Weapon")]
    public class Weapon : Item
    {
        public WeaponType weaponType;
        public float range = 1f;
        
        public Weapon()
        {
            type = ItemType.Weapon;
            consumable = false;
            stackable = false;
        }
        
        public enum WeaponType
        {
            Melee,
            Range
        }

        public override void PerformAction()
        {
            base.PerformAction();
            
        }
    }
}