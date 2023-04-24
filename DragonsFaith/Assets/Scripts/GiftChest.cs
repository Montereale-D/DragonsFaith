using Inventory;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GiftChest : NetworkBehaviour
{
    [SerializeField] private Item item;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closeSprite;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;

    private readonly NetworkVariable<bool> _isOpen = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;
    }

    public override void OnNetworkSpawn()
    {
        _isOpen.OnValueChanged += (_, newValue) =>
        {
            _spriteRenderer.sprite = newValue ? openSprite : closeSprite;
            _collider.enabled = !newValue;
        };
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.GetComponent<NetworkObject>().IsOwner) return;
        
        if (!_isOpen.Value)
        {
            if(!InventoryManager.Instance.AddItem(item))
            {
                Debug.Log("No space in the inventory!");
                return;
            }
            
            Debug.Log("Get item");
            if (IsHost)
                _isOpen.Value = true;
            else 
                UpdateChestStatusServerRpc(true);
            
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateChestStatusServerRpc(bool b)
    {
        if (!IsHost) return;

        _isOpen.Value = b;
    }
}