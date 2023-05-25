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
        characterList = characters.ToList();
        //order list by agility

        SelectNextActiveUnit();

        _isReady = true;
    }

    private void SelectNextActiveUnit()
    {
        _activeUnit = GetNextActiveUnit();

        if (_activeUnit.GetTeam() == PlayerGridMovement.Team.Players && _activeUnit.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            _canMoveThisTurn = true;
            _canAttackThisTurn = true;
        }
        else
        {
            _canMoveThisTurn = false;
            _canAttackThisTurn = false;
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

        if (GameHandler.instance.state == GameState.Battle)
            MapHandler.instance.ShowNavigableTiles();
        
        //check if mouse is hovering at least one tile, then check player action
        var tileHit = MapHandler.instance.GetHoveredTile();
        if (tileHit.HasValue)
        {
            var tile = tileHit.Value.collider.GetComponent<Tile>();
            tile.ShowTile();
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

        _activeUnit.MoveToTile(tile);
    }

    private void CheckAction(Tile tile)
    {
        // Check if clicking on a unit position
        if (tile.charaterOnTile == null) return;

        // Clicked on top of a Unit
        if (_activeUnit.IsEnemy(tile.charaterOnTile))
        {
            CheckActionOnEnemy(tile);
        }
        else
        {
            CheckActionOnPlayer(tile);
        }
    }

    private void CheckActionOnPlayer(Tile tile)
    {
        throw new System.NotImplementedException();
    }

    private void CheckActionOnEnemy(Tile tile)
    {
        // Clicked on an Enemy of the current unit
        if (!_activeUnit.CanAttackUnit(tile.charaterOnTile)) return;

        // Can Attack Enemy
        if (!_canAttackThisTurn) return;

        // Attack Enemy
        _canAttackThisTurn = false;
        _activeUnit.Attack(tile.charaterOnTile);
    }

    private void SkipTurn()
    {
        if(!_activeUnit.GetComponent<NetworkObject>().IsLocalPlayer)
            return;
        
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
        if(!IsHost) return;
        
        SelectNextActiveUnit();
    }
    
    [ClientRpc]
    private void HostHasSkippedClientRpc()
    {
        if(IsHost) return;
        
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
}