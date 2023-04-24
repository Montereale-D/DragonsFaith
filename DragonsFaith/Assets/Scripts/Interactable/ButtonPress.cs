using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    [RequireComponent(typeof(Collider2D))]
    public class ButtonPress : NetworkBehaviour
    {
        [Header("Mode")]
        [Tooltip("If true -> will be use Stand Alone section otherwise System section")]
        [SerializeField] private bool standAloneMode;
        [Header("Stand alone section")]
        [SerializeField] private Openable openable;
        [Header("System section")]
        public ButtonSystem buttonSystem;
        
        private int _triggerCount;

        private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
        
        public override void OnNetworkSpawn()
        {
            _isActive.OnValueChanged += (_, newValue) =>
            {
                ChangeSprite(newValue);
                if (newValue)
                    openable.OpenAction();
                else
                    openable.CloseAction();
                //_collider.enabled = !newValue;
            };
        }

        private void ChangeSprite(bool newValue)
        {
            GetComponent<SpriteRenderer>().color = newValue ? Color.green : Color.red;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            _triggerCount++;
            if (_triggerCount == 1)
            {
                if(standAloneMode)
                    ChangeStatusProcedure(true);
                else
                {
                    buttonSystem.OnButtonPressed();
                    GetComponent<SpriteRenderer>().color = Color.green;
                }
                    
            }
        }
        private void OnTriggerExit2D(Collider2D col)
        {
            _triggerCount--;
            if (_triggerCount == 0)
            {
                if(standAloneMode)
                    ChangeStatusProcedure(false);
                else
                {
                    buttonSystem.OnButtonRelease();
                    GetComponent<SpriteRenderer>().color = Color.red;
                }
            }
        }

        private void ChangeStatusProcedure(bool isActive)
        {
            if (standAloneMode)
            {
                if (IsHost)
                {
                    _isActive.Value = isActive;
                }
                else
                {
                    ChangeStatusProcedureServerRpc(isActive);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeStatusProcedureServerRpc(bool isActive)
        {
            if (!IsHost) return;
            _isActive.Value = isActive;
        }
    }
}
