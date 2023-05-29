using System.Collections.Generic;
using System.Linq;
using Inventory;
using Inventory.Items;
using Player;
using Unity.Netcode;
using UnityEngine;

public class CombatSystem : NetworkBehaviour
{
    public static CombatSystem instance;
    public List<PlayerGridMovement> characterList;

    private int _indexCharacterTurn = -1;
    private PlayerGridMovement _activeUnit;
    private bool _canMoveThisTurn;
    private bool _canAttackThisTurn;
    private bool _isThisPlayerTurn;
    private bool _isReady;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    public void Setup(IEnumerable<PlayerGridMovement> characters)
    {
        characterList = characters.OrderByDescending(x => x.movement).ThenBy(x => x.name).ThenBy(x => x.GetHashCode())
            .ToList();

        SelectNextActiveUnit();

        _isReady = true;
    }

    private void SelectNextActiveUnit()
    {
        _activeUnit = GetNextActiveUnit();

        Debug.Log("Turn of " + _activeUnit.name);

        _canMoveThisTurn = true;
        _canAttackThisTurn = true;

        if (_activeUnit.GetTeam() == PlayerGridMovement.Team.Players &&
            _activeUnit.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            _isThisPlayerTurn = true;
        }
        else
        {
            _isThisPlayerTurn = false;

            //if the active unity is enemy and I am host, I should notify the enemy
            if (_activeUnit.GetTeam() == PlayerGridMovement.Team.Enemies && NetworkManager.Singleton.IsHost)
            {
                _activeUnit.GetComponent<EnemyGridBehaviour>().PlanAction(characterList);
            }
        }
    }

    private PlayerGridMovement GetNextActiveUnit()
    {
        //todo aggiungere controlli sulla scelta del next

        _indexCharacterTurn++;
        if (_indexCharacterTurn >= characterList.Count)
            _indexCharacterTurn = 0;

        return characterList[_indexCharacterTurn];
    }


    private void Update()
    {
        //wait for settings
        if (!_isReady) return;

        MapHandler.instance.HideAllTiles();

        if (!_isThisPlayerTurn) return;

        MapHandler.instance.ShowNavigableTiles(_activeUnit.onTile, _activeUnit.movement);

        //check if mouse is hovering at least one tile, then check player action
        var hoveredHit = MapHandler.instance.GetHoveredRaycast();
        if (hoveredHit.HasValue)
        {
            var tile = hoveredHit.Value.collider.GetComponent<Tile>();
            if (tile)
            {
                tile.ShowTile();

                var characterOnTile = tile.GetCharacter();
                if (Input.GetMouseButtonDown(0))
                {
                    if (characterOnTile)
                    {
                        Debug.Log("Click for Action");
                        CheckAction(characterOnTile);
                    }
                    else
                    {
                        Debug.Log("Click for Movement");
                        CheckMovement(tile);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                SkipTurn();
            }
        }
    }

    private void CheckMovement(Tile tile)
    {
        if (!MapHandler.instance.GetTilesInRange(_activeUnit.onTile, _activeUnit.movement).Contains(tile)) return;

        if (!_canMoveThisTurn) return;

        _canMoveThisTurn = false;

        // Set entire Tilemap to Invisible
        MapHandler.instance.HideAllTiles();

        // Remove Unit from tile
        _activeUnit.onTile.ClearTile();

        // Set Unit on target Grid Object
        tile.SetCharacterOnTile(_activeUnit);

        if (IsHost)
        {
            //NB uso int perché servono primitive per Rpc
            NotifyMovementClientRpc(tile.mapPosition.x, tile.mapPosition.y,
                !_activeUnit.GetComponent<NetworkObject>().IsLocalPlayer);
        }
        else
        {
            //NB uso int perché servono primitive per Rpc
            NotifyMovementServerRpc(tile.mapPosition.x, tile.mapPosition.y);
        }

        _activeUnit.MoveToTile(tile);
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyMovementServerRpc(int x, int y)
    {
        if (!IsHost) return;

        var toPosition = new Vector2Int(x, y);
        var tile = MapHandler.instance.GetMap()[toPosition];
        _activeUnit.MoveToTile(tile);
    }

    [ClientRpc]
    private void NotifyMovementClientRpc(int x, int y, bool move)
    {
        if (IsHost) return;

        var toPosition = new Vector2Int(x, y);
        var tile = MapHandler.instance.GetMap()[toPosition];

        if (move)
        {
            _activeUnit.MoveToTile(tile);
        }
    }

    //// <returns>false if no action has been performed</returns>
    private void CheckAction(PlayerGridMovement target)
    {
        // Clicked on top of a Unit
        if (_activeUnit.IsEnemy(target))
        {
            CheckActionOnEnemy(target);
        }
        else
        {
            CheckActionOnPlayer(target);
        }
    }

    private void CheckActionOnPlayer(PlayerGridMovement characterOnTile)
    {
        //todo aggiungere azioni sul player
    }

    private void CheckActionOnEnemy(PlayerGridMovement characterOnTile)
    {
        // Can Attack Enemy
        if (!_canAttackThisTurn) return;

        // Clicked on an Enemy of the current unit
        var weapon = _activeUnit.GetTeam() == PlayerGridMovement.Team.Players
            ? InventoryManager.Instance.GetWeapon()
            : _activeUnit.GetComponent<EnemyGridBehaviour>().weapon;
        if (!weapon)
        {
            weapon = ScriptableObject.CreateInstance<Weapon>();
            weapon.range = 1;
            weapon.weaponType = Weapon.WeaponType.Melee;
            weapon.damage = 1;
        }

        if (CanAttackUnit(characterOnTile, weapon)) return;

        // Attack Enemy
        _canAttackThisTurn = false;
        Attack(characterOnTile);
    }

    public bool CanAttackUnit(PlayerGridMovement target, Weapon weapon)
    {
        return Vector2Int.Distance(_activeUnit.onTile.mapPosition, target.onTile.mapPosition) <= weapon.range;
    }

    public void Attack(PlayerGridMovement target)
    {
        Debug.Log("Attack!");
    }

    private void SkipTurn()
    {
        if (IsHost)
        {
            HostHasSkippedClientRpc();
            SelectNextActiveUnit();
        }
        else
        {
            ClientHasSkippedServerRpc();
            SelectNextActiveUnit();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientHasSkippedServerRpc()
    {
        if (!IsHost) return;

        SelectNextActiveUnit();
    }

    [ClientRpc]
    private void HostHasSkippedClientRpc()
    {
        if (IsHost) return;

        SelectNextActiveUnit();
    }


    public void SetUnitGridCombat(PlayerGridMovement unitGridCombat)
    {
        _activeUnit = unitGridCombat;
    }

    public void ClearUnitGridCombat()
    {
        SetUnitGridCombat(null);
    }

    public PlayerGridMovement GetUnitGridCombat()
    {
        return _activeUnit;
    }

    [ContextMenu("Force skip")]
    public void ForceSkipDebug()
    {
        Debug.Log("Skip from context menu");
        if (IsHost)
        {
            HostHasSkippedClientRpc();
            SelectNextActiveUnit();
        }
        else
        {
            ClientHasSkippedServerRpc();
            SelectNextActiveUnit();
        }
    }
}