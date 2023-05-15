using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/ActiveSkill")]
    public class ActiveSkill : Skill
    {
        public Weapon.WeaponType weaponType;
        public float range = 1f;

        public override void PerformAction()
        {
            base.PerformAction();
            
        }
    }
}