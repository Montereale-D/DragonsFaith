using System.Collections.Generic;
using System.Linq;
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
        characterList = characters.OrderByDescending(x => x.movement).ThenBy(x => x.GetHashCode()).ToList();

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
                _activeUnit.GetComponent<EnemyGridBehaviour>().PlanAction();
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
        var tileHit = MapHandler.instance.GetHoveredTile();
        if (tileHit.HasValue)
        {
            var tile = tileHit.Value.collider.GetComponent<Tile>();
            if (tile)
            {
                tile.ShowTile();
            }

            if (Input.GetMouseButtonDown(0))
            {
                CheckAction(tile);

                //No fight try to move
                CheckMovement(tile);
            }
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkipTurn();
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


    private void CheckAction(Tile tile)
    {
        // Check if clicking on a unit position
        var characterOnTile = tile.GetCharacter();
        if (!characterOnTile) return;

        // Clicked on top of a Unit
        if (_activeUnit.IsEnemy(characterOnTile))
        {
            CheckActionOnEnemy(tile, characterOnTile);
        }
        else
        {
            CheckActionOnPlayer(tile, characterOnTile);
        }
    }

    private void CheckActionOnPlayer(Tile tile, PlayerGridMovement characterOnTile)
    {
        //todo
    }

    private void CheckActionOnEnemy(Tile tile, PlayerGridMovement characterOnTile)
    
    {
        // Clicked on an Enemy of the current unit
        if (!_activeUnit.CanAttackUnit(characterOnTile)) return;

        // Can Attack Enemy
        if (!_canAttackThisTurn) return;

        // Attack Enemy
        _canAttackThisTurn = false;
        _activeUnit.Attack(characterOnTile);
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