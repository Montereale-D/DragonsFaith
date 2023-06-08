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

        private NetworkObject _netObj;

        [SerializeField] private string saveId;

        public override void OnNetworkSpawn()
        {
            //subscribe to status change event
            _isActive.OnValueChanged += OnStateChange;

            _netObj = GetComponent<NetworkObject>();
            _netObj.DestroyWithScene = true;
            
            if(!IsHost) return;
            
            if (DungeonProgressManager.instance.IsButtonPressed(saveId))
            {
                Debug.Log(gameObject.name + " was already activated");
                //openable.OpenAction();
                _isActive.Value = true;
            }
            else
            {
                //Debug.Log(gameObject.name + " OnNetworkSpawn");
            }
        }
        
        private void OnStateChange(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                openable.OpenAction();
                DungeonProgressManager.instance.ButtonChangeState(saveId, true);
            }
            else
                openable.CloseAction();
        }

        public void OnButtonPressed()
        {
            _buttonPressedCounter++;

            Debug.Log("button pressed");
            //if both the buttons are active
            if (_buttonPressedCounter == 2)
            {
                ChangeStatusProcedure(true);
            }
        }

        public void OnButtonRelease()
        {
            _buttonPressedCounter--;
            
            Debug.Log("button released");
            //if both the buttons are active
            if (_buttonPressedCounter < 2)
            {
                ChangeStatusProcedure(false);
            }
        }

        //Procedure to follow when the status change
        private void ChangeStatusProcedure(bool isActive)
        {
            if (IsHost)
            {
                //direct change
                Debug.Log("changing value");
                _isActive.Value = isActive;
            }
            else
            {
                //ask host to change
                Debug.Log("ask host to change");
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
        
        [ContextMenu("Generate guid")]
        private void GenerateGuid()
        {
            saveId = System.Guid.NewGuid().ToString();
        }
    }
}