using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/LegsArmor")]
    public class Legs : Armor
    {
        public Legs()
        {
            type = ItemType.Legs;
        }
        
        public override void PerformAction()
        {
            base.PerformAction();
            
        }
    }
}