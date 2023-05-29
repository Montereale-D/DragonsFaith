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
    private Tile _selectedTile;

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
        _selectedTile = null;

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
        if (_selectedTile)
        {
            _selectedTile.SelectTile();
        }

        //check if mouse is hovering at least one tile, then check player action
        var hoveredHit = MapHandler.instance.GetHoveredRaycast();
        if (hoveredHit.HasValue)
        {
            var tile = hoveredHit.Value.collider.GetComponent<Tile>();
            if (tile)
            {
                tile.ShowTile();

                //var characterOnTile = tile.GetCharacter();
                if (Input.GetMouseButtonDown(0))
                {
                    /*if (characterOnTile)
                    {
                        Debug.Log("Click for Action");
                        CheckAction(characterOnTile);
                    }
                    else
                    {
                        Debug.Log("Click for Movement");
                        CheckMovement(tile);
                    }*/

                    SelectTile(tile);
                }
            }

            if (_selectedTile && Input.GetKeyDown(KeyCode.A))
            {
                if (selectMode == SelectTileMode.Action)
                {
                    Debug.Log("Click for Action");
                    CheckAction(_selectedTile.GetCharacter());
                }
                else if (selectMode == SelectTileMode.Movement)
                {
                    Debug.Log("Click for Movement");
                    CheckMovement(_selectedTile);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                SkipTurn();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnselectTile();
            }
        }
    }

    public enum SelectTileMode
    {
        None,
        Movement,
        Action
    }

    public SelectTileMode selectMode;

    private void SelectTile(Tile tile)
    {
        _selectedTile = tile;
        _selectedTile.SelectTile();

        selectMode = tile.GetCharacter() ? SelectTileMode.Action : SelectTileMode.Movement;
    }

    private void UnselectTile()
    {
        _selectedTile.ShowTile();
        _selectedTile = null;
        selectMode = SelectTileMode.None;
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
            Debug.Log("Target is an enemy of active unit");
            CheckActionOnEnemy(target);
        }
    }

    private void CheckActionOnEnemy(PlayerGridMovement characterOnTile)
    {
        // Can Attack Enemy
        if (!_canAttackThisTurn)
        {
            Debug.Log("Already attack in this turn");
            return;
        }

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

        if (!CanAttackUnit(characterOnTile, weapon))
        {
            Debug.Log("Target outside range with this weapon");
            return;
        }

        // Attack Enemy
        _canAttackThisTurn = false;
        Attack(characterOnTile, weapon);
    }

    public bool CanAttackUnit(PlayerGridMovement target, Weapon weapon)
    {
        return Vector2Int.Distance(_activeUnit.onTile.mapPosition, target.onTile.mapPosition) <= weapon.range;
    }

    public void Attack(PlayerGridMovement target, Weapon weapon)
    {
        Debug.Log("Attack!");
        if (target.GetTeam() == PlayerGridMovement.Team.Enemies)
        {
            var damage = (int)(weapon.damage + CharacterManager.Instance.GetTotalStr());
            NotifyAttackToEnemy(target, damage);
        }
        else
        {
            var damage = (int)(target.GetComponent<EnemyGridBehaviour>().weapon.damage);
            NotifyAttackToPlayer(target, damage);
        }
    }

    private void NotifyAttackToEnemy(PlayerGridMovement target, int damage)
    {
        target.GetComponent<EnemyGridBehaviour>().Damage(damage);

        var targetIndex = characterList.IndexOf(target);
        if (IsHost)
        {
            NotifyAttackFromHostToEnemyClientRpc(targetIndex, damage);
        }
        else
        {
            NotifyAttackFromClientToEnemyServerRpc(targetIndex, damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyAttackFromClientToEnemyServerRpc(int targetIndex, int damage)
    {
        if (!IsHost) return;
        characterList[targetIndex].GetComponent<EnemyGridBehaviour>().Damage(damage);
    }

    [ClientRpc]
    private void NotifyAttackFromHostToEnemyClientRpc(int targetIndex, int damage)
    {
        if (IsHost) return;
        characterList[targetIndex].GetComponent<EnemyGridBehaviour>().Damage(damage);
    }

    private void NotifyAttackToPlayer(PlayerGridMovement target, int damage)
    {
        //todo test dopo UI
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (target.gameObject == localPlayer)
        {
            CharacterManager.Instance.Damage(damage);
        }

        var targetIndex = characterList.IndexOf(target);
        if (IsHost)
        {
            NotifyAttackFromEnemyToPlayerClientRpc(targetIndex, damage);
        }
        else
        {
            NotifyAttackFromEnemyToPlayerServerRpc(targetIndex, damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyAttackFromEnemyToPlayerServerRpc(int targetIndex, int damage)
    {
        if (!IsHost) return;

        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (characterList[targetIndex].gameObject == localPlayer)
        {
            CharacterManager.Instance.Damage(damage);
        }
    }

    [ClientRpc]
    private void NotifyAttackFromEnemyToPlayerClientRpc(int targetIndex, int damage)
    {
        if (IsHost) return;

        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (characterList[targetIndex].gameObject == localPlayer)
        {
            CharacterManager.Instance.Damage(damage);
        }
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