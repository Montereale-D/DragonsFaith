using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    public class ButtonSystem : NetworkBehaviour
    {
        [Tooltip("Reference to the openable object")] [SerializeField]
        private Openable openable;

        //counter of buttons active
        private int _buttonPressedCounter;

        //synchronize state between host and client
        private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            //subscribe to status change event
            _isActive.OnValueChanged += (_, newValue) =>
            {
                if (newValue)
                    openable.OpenAction();
                else
                    openable.CloseAction();
            };
        }

        public void OnButtonPressed()
        {
            _buttonPressedCounter++;

            //if both the buttons are active
            if (_buttonPressedCounter == 2)
            {
                ChangeStatusProcedure(true);
            }
        }

        public void OnButtonRelease()
        {
            _buttonPressedCounter--;
        }

        //Procedure to follow when the status change
        private void ChangeStatusProcedure(bool isActive)
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