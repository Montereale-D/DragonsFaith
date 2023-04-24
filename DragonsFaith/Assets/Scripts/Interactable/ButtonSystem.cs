using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    public class ButtonSystem : NetworkBehaviour
    {
        [SerializeField] private Openable openable;
        private int _buttonPressed;

        private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            _isActive.OnValueChanged += (_, newValue) =>
            {
                if (newValue)
                    openable.OpenAction();
                else
                    openable.CloseAction();
                //_collider.enabled = !newValue;
            };
        }

        public void OnButtonPressed()
        {
            _buttonPressed++;
            if (_buttonPressed == 2)
            {
                ChangeStatusProcedure(true);
            }
        }

        public void OnButtonRelease()
        {
            _buttonPressed--;
        }

        private void ChangeStatusProcedure(bool isActive)
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

        [ServerRpc(RequireOwnership = false)]
        private void ChangeStatusProcedureServerRpc(bool isActive)
        {
            if (!IsHost) return;
            _isActive.Value = isActive;
        }
    }
}