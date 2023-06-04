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

        private NetworkObject _netObj;
        [SerializeField] private string saveId;

        private void Awake()
        {
            //ensure isTrigger
            GetComponent<Collider2D>().isTrigger = true;
            
            //subscribe to status change event
            _isActive.OnValueChanged += OnButtonPressed;

            _netObj = GetComponent<NetworkObject>();
            _netObj.DestroyWithScene = true;
        }

        public override void OnNetworkSpawn()
        {
            //if(!IsHost) return;
            
            if (standAloneMode)
            {
                if (DungeonProgressManager.instance.IsButtonPressed(saveId))
                {
                    Debug.Log(gameObject.name + " was already activated");
                    //openable.OpenAction();
                    //_isActive.Value = true;
                    OnButtonPressed(false, true);
                }
                else
                {
                    Debug.Log(gameObject.name + " OnNetworkSpawn");
                }
            }
        }

        private void OnButtonPressed(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                ChangeSprite(true);
                openable.OpenAction();
                if (standAloneMode)
                {
                    DungeonProgressManager.instance.ButtonChangeState(saveId, true);
                }
            }
            else
                openable.CloseAction();
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

        [ClientRpc]
        private void ChangeStatusProcedureClientRpc(bool isActive)
        {
            ChangeStatusProcedure(true);
        }
        
        [ContextMenu("Generate guid")]
        private void GenerateGuid()
        {
            saveId = System.Guid.NewGuid().ToString();
        }
    }
}