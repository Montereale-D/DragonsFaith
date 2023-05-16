using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    [RequireComponent(typeof(Collider2D))]
    public class ButtonPress : NetworkBehaviour
    {
        [Header("Mode")] [Tooltip("If true -> use Stand Alone section otherwise System section")] [SerializeField]
        private bool standAloneMode;

        [Header("Stand alone section")] [Tooltip("Reference to the openable object")] [SerializeField]
        private Openable openable;

        [Header("System section")] [Tooltip("Reference to button system")] [SerializeField]
        private ButtonSystem buttonSystem;

        //counter of collision at the moment
        private int _triggerCount;

        //synchronize state between host and client
        private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private void Awake()
        {
            //ensure isTrigger
            GetComponent<Collider2D>().isTrigger = true;
        }

        public override void OnNetworkSpawn()
        {
            //subscribe to status change event
            _isActive.OnValueChanged += (_, newValue) =>
            {
                ChangeSprite(newValue);
                if (newValue)
                    openable.OpenAction();
                else
                    openable.CloseAction();
            };
            
            GetComponent<NetworkObject>().DestroyWithScene = true;
        }

        private void ChangeSprite(bool newValue)
        {
            GetComponent<SpriteRenderer>().color = newValue ? Color.green : Color.red;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            _triggerCount++;

            //if first trigger collision
            if (_triggerCount == 1)
            {
                if (standAloneMode)
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

            //if it was the last trigger collision
            if (_triggerCount == 0)
            {
                if (standAloneMode)
                    ChangeStatusProcedure(false);
                else
                {
                    buttonSystem.OnButtonRelease();
                    GetComponent<SpriteRenderer>().color = Color.red;
                }
            }
        }

        //Procedure to follow when standAloneMode is active
        private void ChangeStatusProcedure(bool isActive)
        {
            if (standAloneMode)
            {
                if (IsHost)
                {
                    //direct change
                    _isActive.Value = isActive;
                }
                else
                {
                    //ask host to change
                    ChangeStatusProcedureServerRpc(isActive);
                }
            }
        }

        //Server receive a change status request from client
        [ServerRpc(RequireOwnership = false)]
        private void ChangeStatusProcedureServerRpc(bool isActive)
        {
            if (!IsHost) return;

            if (_isActive.Value != isActive)
            {
                _isActive.Value = isActive;
            }
        }
    }
}