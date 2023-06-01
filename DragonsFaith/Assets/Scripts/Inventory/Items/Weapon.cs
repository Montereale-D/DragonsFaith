using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/Weapon")]
    public class Weapon : Item
    {
        public WeaponType weaponType;
        public float range = 1f;
        public float damage = 1;
        public int capacity = 1;
        private int _ammo;
        
        public Weapon()
        {
            type = ItemType.Weapon;
            consumable = false;
            stackable = false;
            _ammo = capacity;
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

        public void UseAmmo()
        {
            _ammo--;
        }

        public void Reload()
        {
            _ammo = capacity;
        }

        public bool CanFire()
        {
            if (weaponType == WeaponType.Melee) return true;

            return _ammo > 0;
        }

        public bool IsFullyLoaded()
        {
            return _ammo == capacity;
        }

        public int GetAmmo()
        {
            return _ammo;
        }
        
        public float GetRange()
        {
            return range;
        }
    }
}