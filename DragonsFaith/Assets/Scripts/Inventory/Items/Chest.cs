using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/ChestArmor")]
    public class Chest : Armor
    {
        public Chest()
        {
            type = ItemType.Chest;
        }
        
        public override void PerformAction()
        {
            base.PerformAction();
            
        }
    }
}