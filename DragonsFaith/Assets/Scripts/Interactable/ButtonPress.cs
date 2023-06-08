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
            GetComponent<Collider2D>().isTrigger = true;

            _isActive.OnValueChanged += OnStateChange;

            _netObj = GetComponent<NetworkObject>();
            _netObj.DestroyWithScene = true;
        }

        public override void OnNetworkSpawn()
        {
            if (!standAloneMode) return;
            //if (!IsHost) return;

            if (DungeonProgressManager.instance.IsButtonPressed(saveId))
            {
                Debug.Log(gameObject.name + " was already activated");
                //openable.OpenAction();
                //_isActive.Value = true;
                ChangeStatusProcedure(true);
                
            }
            else
            {
                //Debug.Log(gameObject.name + " OnNetworkSpawn");
            }
        }
        
        private void OnStateChange(bool previousValue, bool newValue)
        {
            Debug.Log("OnButtonPressed " + newValue);
            /*if (newValue) {
            ChangeSprite(newValue);
            //openable.OpenAction();
            if (standAloneMode)
            {
                openable.OpenAction();
                DungeonProgressManager.instance.ButtonChangeState(saveId, true);
                Notify();
            }
            }else openable.CloseAction();*/
            
            if (newValue)
            {
                openable.OpenAction();
                DungeonProgressManager.instance.ButtonChangeState(saveId, true);
            }
            else
                openable.CloseAction();
            
            ChangeSprite(newValue);
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
                {
                    //ChangeStatusProcedure(false);
                }
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
            Debug.Log("ChangeStatusProcedure");
            if (standAloneMode && isActive)
            {
                if (IsHost)
                {
                    //direct change
                    _isActive.Value = true;
                    ChangeStatusProcedureClientRpc(true);
                }
                else
                {
                    //ask host to change
                    ChangeStatusProcedureServerRpc();
                    OnStateChange(false, true);
                }
            }
        }

        //Server receive a change status request from client
        [ServerRpc(RequireOwnership = false)]
        private void ChangeStatusProcedureServerRpc()
        {
            if (!IsHost) return;

            OnStateChange(false, true);
        }
        [ClientRpc]
        private void ChangeStatusProcedureClientRpc(bool isActive)
        {
            if (IsHost) return;

            OnStateChange(false, true);
            //if (_isActive.Value != isActive)
            //{
            //_isActive.Value = isActive;
            //}
        }
        

        [ContextMenu("Generate guid")]
        private void GenerateGuid()
        {
            saveId = System.Guid.NewGuid().ToString();
        }
    }
}