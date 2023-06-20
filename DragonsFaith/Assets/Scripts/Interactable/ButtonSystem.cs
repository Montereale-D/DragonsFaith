﻿using System;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace Interactable
{
    public class ButtonSystem : NetworkBehaviour
    {
        [Tooltip("Reference to the openable object")] [SerializeField]
        private Openable openable;

        private bool _playSound = true;

        //counter of buttons active
        private int _buttonPressedCounter;

        //synchronize state between host and client
        private readonly NetworkVariable<bool> _isActive = new(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private NetworkObject _netObj;

        [SerializeField] private string saveId;

        private void Awake()
        {
            //subscribe to status change event
            _isActive.OnValueChanged += OnStateChange;

            _netObj = GetComponent<NetworkObject>();
            _netObj.DestroyWithScene = true;
        }

        public override void OnNetworkSpawn()
        {
            //if(!IsHost) return;

            if (DungeonProgressManager.instance.IsButtonPressed(saveId, gameObject))
            {
                Debug.Log(gameObject.name + " was already activated");
                _playSound = false;
                ChangeStatusProcedure(true);
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
                if (_playSound)
                {
                    if (openable.GetComponent<Door>())
                    {
                        AudioManager.instance.PlayOpenGateSound();
                    }
                    else if (openable.GetComponent<HiddenArea>())
                    {
                        AudioManager.instance.PlayOpenHiddenSound();
                    }
                    _playSound = false;
                }
                DungeonProgressManager.instance.ButtonChangeState(saveId, true, gameObject);
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
            Debug.Log("ChangeStatusProcedure");
            if (isActive)
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
                    OnStateChange(false, true);
                }
            }
        }
        
        [ClientRpc]
        private void ChangeStatusProcedureClientRpc(bool isActive)
        {
            if (IsHost) return;

            OnStateChange(false, true);
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

        public void KeyOpenableChangeStatusProcedure()
        {
            ChangeStatusProcedure(true);
        }
    }
}