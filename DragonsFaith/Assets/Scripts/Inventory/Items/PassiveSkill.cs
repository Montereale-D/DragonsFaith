using UnityEngine;

namespace Inventory.Items
{
    [CreateAssetMenu(menuName = "ItemsType/PassiveSkill")]
    public class PassiveSkill : Skill
    {
        public float Str;
        public float Dex;
        public float Int;
        public float Const;
        public float Agi;
        public override void PerformAction()
        {
            base.PerformAction();
            
        }
    }
}