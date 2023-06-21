using Player;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    public class AbilityChecker : KeyInteractable
    {
        [SerializeField] private Attribute abilityToCheck;
        public UnityEvent onSuccess;
        [SerializeField] private string saveId;
        
        
        protected override void Awake()
        {
            onKeyPressedEvent = CheckAbility;
            base.Awake();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            GetComponent<NetworkObject>().DestroyWithScene = true;
            
            if (DungeonProgressManager.instance.IsAbilityPassed(saveId, gameObject))
            {
                Debug.Log(gameObject.name + " was already activated");
                //_isUsed.Value = true;
                SetActive(true);
            }
            else
            {
                Debug.Log(gameObject.name + " OnNetworkSpawn");
            }
        }

        private void CheckAbility(Collider2D col)
        {
            if (!_isUsed.Value)
            {
                if (!CharacterManager.Instance.AbilityCheck(abilityToCheck))
                {
                    ShowNotAble();
                    return;
                }

                DungeonProgressManager.instance.AbilityPassed(saveId, gameObject);
                Notify();
                
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
        
        private void Notify()
        {
            if (IsHost)
            {
                AbilityPassedClientRpc();
            }
            else
            {
                AbilityPassedServerRpc();
            }
        }

        [ClientRpc]
        private void AbilityPassedClientRpc()
        {
            if(IsHost) return;
            
            DungeonProgressManager.instance.AbilityPassed(saveId, gameObject);
        }
        
        [ServerRpc (RequireOwnership = false)]
        private void AbilityPassedServerRpc()
        {
            if(!IsHost) return;
            
            DungeonProgressManager.instance.AbilityPassed(saveId, gameObject);
            _isUsed.Value = true;
            onSuccess?.Invoke();
        }

        private void ShowNotAble()
        {
            PlayerUI.instance.ShowMessage("Not enough intelligence to hack.");
            Debug.Log("Not enough ability!");
        }
        
        
        [ContextMenu("Generate guid")]
        private void GenerateGuid()
        {
            saveId = System.Guid.NewGuid().ToString();
        }
    }
}
