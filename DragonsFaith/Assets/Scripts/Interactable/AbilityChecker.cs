using Player;
using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    public class AbilityChecker : KeyInteractable
    {
        [SerializeField] private Attribute abilityToCheck;
        public UnityEvent onSuccess;
        protected override void Awake()
        {
            onKeyPressedEvent = CheckAbility;
            base.Awake();
        }

        private void CheckAbility(Collider2D col)
        {
            if (!_isUsed.Value)
            {
                //try to add item to the inventory
                if (!CharacterManager.Instance.AbilityCheck(abilityToCheck))
                {
                    ShowNotAble();
                    return;
                }

                if (IsHost)
                {
                    //direct change
                    _isUsed.Value = true;
                }
                else
                {
                    //ask host to change
                    UpdateStatusServerRpc(true);
                }
                
                onSuccess?.Invoke();
            }
        }

        private void ShowNotAble()
        {
            Debug.Log("Not enough ability!");
        }
    }
}
