using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/HeadArmor")]
    public class Head : Armor
    {
        public Head()
        {
            type = ItemType.Head;
        }
        public override void PerformAction()
        {
            base.PerformAction();
            
        }
    }
}