using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Inventory.Items;
using Player;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatSystem : NetworkBehaviour
{
    public static CombatSystem instance;
    public List<PlayerGridMovement> characterList;

    private int _indexCharacterTurn = -1;
    public PlayerGridMovement _activeUnit { get; private set; }
    private bool _canMoveThisTurn;
    private bool _canAttackThisTurn;
    private bool _isThisPlayerTurn;
    private bool _isCombatReady;
    private bool _isUIReady;
    private Tile _selectedTile;

    private PlayerGridMovement _target;

    public int? otherPlayerSpriteIdx;
    private PlayerUI _playerUI;
    private MapHandler _mapHandler;
    private TurnUI _turnUI;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;

        _playerUI = PlayerUI.Instance;
        _mapHandler = MapHandler.instance;
    }

    public void Setup(IEnumerable<PlayerGridMovement> characters)
    {
        characterList = characters.OrderByDescending(x => x.movement)
            .ThenBy(x => x.name)
            .ThenBy(x => x.GetHashCode())
            .ToList();

        StartCoroutine(SetUpTurns());

        StartCoroutine(WaitSetupToStart());
        //SelectNextActiveUnit();

        //GetPortraitSprite();

        
        
        /*GetPortraitSprite();
        _playerUI.ToggleCombatUI(characterList);
        _turnUI = _playerUI.GetCombatUI().GetTurnUI();*/
        _isCombatReady = true;
    }

    private IEnumerator WaitSetupToStart()
    {
        while (!_isCombatReady || !_isUIReady)
        {
            yield return new WaitForSeconds(0.2f);
        }
        
        SelectNextActiveUnit();
    }

    private void SelectNextActiveUnit()
    {
        _activeUnit = GetNextActiveUnit(); 
        //_turnUI.NextTurn(); //TODO solo comment: qua dentro non va bene perché fa skippare alla UI il primo turno

        Debug.Log("Turn of " + _activeUnit.name);

        _canMoveThisTurn = true;
        _canAttackThisTurn = true;
        _selectedTile = null;

        if (_activeUnit.GetTeam() == PlayerGridMovement.Team.Players &&
            _activeUnit.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            _isThisPlayerTurn = true;
            //_playerUI.SetMovementCounter(_activeUnit.movement);
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
        if (!_isCombatReady || !_isUIReady) return;

        _mapHandler.HideAllTiles();

        if (!_isThisPlayerTurn) return;

        if (_canMoveThisTurn) _mapHandler.ShowNavigableTiles(_activeUnit.onTile, _activeUnit.movement);
        if (_selectedTile)
        {
            _selectedTile.SelectTile();
        }

        /*if (Input.GetKeyDown(KeyCode.T))
        {
            /*ShowMessage("Testing...");#1#
            _playerUI.ToggleCombatUI(characterList);
        }*/

        //check if mouse is hovering at least one tile, then check player action
        var hoveredHit = _mapHandler.GetHoveredRaycast();
        if (hoveredHit.HasValue)
        {
            var tile = hoveredHit.Value.collider.GetComponent<Tile>();
            if (tile)
            {
                tile.HoverTile();

                //var characterOnTile = tile.GetCharacter();
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    SelectTile(tile);
                }
            }

            /*
            if (_selectedTile && Input.GetKeyDown(KeyCode.A))
            {
                if (selectMode == SelectTileMode.Action)
                {
                    Debug.Log("Click for Action");
                    CheckAction(_target);
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
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadAction();
            }
            */
            if (Input.GetMouseButtonDown(1))
            {
                UnselectTile();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                BlockAction();
            }


            if (!_canAttackThisTurn && !_canMoveThisTurn) SkipTurn();
        }
    }

    public void BlockAction()
    {
        //todo block action
        SkipTurn();
    }

    public void ReloadAction()
    {
        var localPlayer =
            NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerGridMovement>();
        if (_activeUnit != localPlayer) return;
        var weapon = GetActiveUnitWeapon();
        if (weapon.IsFullyLoaded())
        {
            _playerUI.ShowMessage("Weapon already fully loaded.");
            return;
        }

        weapon.Reload();
        _playerUI.SetAmmoCounter(weapon.GetAmmo());
        SkipTurn();
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
        if (_selectedTile)
        {
            UnselectTile();
        }

        _selectedTile = tile;
        _selectedTile.SelectTile();

        var character = _selectedTile.GetCharacter();
        if (character && character != _activeUnit)
        {
            character.GetComponent<CharacterGridPopUpUI>().ShowUI();
            if (character.GetTeam() == PlayerGridMovement.Team.Players)
            {
                _playerUI.ToggleMoveAttackButton("Move");
            }
            else
            {
                // Clicked on an Enemy of the current unit
                var weapon = GetActiveUnitWeapon();

                if (!_canAttackThisTurn)
                {
                    _playerUI.SetCombatPopUp(true, "Already attacked this turn.");
                }
                else if (!IsWithinRange(character, weapon))
                {
                    //Debug.Log("Target outside range of weapon");
                    //_playerUI.ShowMessage("Target outside range of weapon.");
                    _playerUI.SetCombatPopUp(true, "Target outside weapon range.");
                }
                else
                {
                    _playerUI.SetCombatPopUp(true,
                        "Target is within weapon range " + weapon.range + "." + System.Environment.NewLine +
                        "Strike target and deal " + (weapon.damage + CharacterManager.Instance.GetTotalStr()) +
                        " DMG.");
                }

                _target = character;
                selectMode = SelectTileMode.Action;
                _playerUI.ToggleMoveAttackButton("Attack");
            }
        }
        else
        {
            if (!_canMoveThisTurn)
            {
                _playerUI.SetCombatPopUp(true, "Already moved this turn.");
            }
            else if (!_mapHandler.GetTilesInRange(_activeUnit.onTile, _activeUnit.movement).Contains(_selectedTile))
            {
                _playerUI.SetCombatPopUp(true, "Cell is too far away.");
            }
            else
            {
                _playerUI.SetCombatPopUp(true, "Cell is within movement range " + _activeUnit.movement + ".");
            }

            selectMode = SelectTileMode.Movement;
            _playerUI.ToggleMoveAttackButton("Move");
        }

        /*if (character)
        {
            _target = tile.GetCharacter();
            selectMode = SelectTileMode.Action;
            _playerUI.ToggleMoveAttackButton("Attack");
        }
        else
        {
            selectMode = SelectTileMode.Movement;
            _playerUI.ToggleMoveAttackButton("Move");
        }*/
        //selectMode = tile.GetCharacter() ? SelectTileMode.Action : SelectTileMode.Movement;
    }

    private void UnselectTile()
    {
        _selectedTile.ShowTile();
        _playerUI.SetCombatPopUp(false);
        var character = _selectedTile.GetCharacter();
        if (character && character != _activeUnit)
        {
            // hides the UI of the enemies on deselection of the cell
            character.GetComponent<CharacterGridPopUpUI>().HideUI();
        }

        _selectedTile = null;
        selectMode = SelectTileMode.None;
    }

    /*public PlayerGridMovement GetTarget()
    {
        return _selectedTile.GetCharacter();
    }*/

    public void ButtonCheckMovement()
    {
        CheckMovement(_selectedTile, false);
    }

    public void CheckMovement(Tile tile, bool updatePosition)
    {
        if (!_mapHandler.GetTilesInRange(_activeUnit.onTile, _activeUnit.movement).Contains(tile))
        {
            //_playerUI.ShowMessage("Cell is too far away.");
            //_playerUI.SetCombatPopUp(true, "Cell is too far away.");
            return;
        }

        if (!_canMoveThisTurn)
        {
            //_playerUI.ShowMessage("Already moved this turn.");
            return;
        }

        _canMoveThisTurn = false;
        //_playerUI.SetMovementCounter(0);

        // Set entire Tilemap to Invisible
        _mapHandler.HideAllTiles();

        // Remove Unit from tile
        _activeUnit.onTile.ClearTile();

        // Set Unit on target Grid Object
        tile.SetCharacterOnTile(_activeUnit);

        if (IsHost)
        {
            //NB uso int perché servono primitive per Rpc
            NotifyMovementClientRpc(tile.mapPosition.x, tile.mapPosition.y, updatePosition);
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
        var tile = _mapHandler.GetMap()[toPosition];
        _activeUnit.MoveToTile(tile);
    }

    [ClientRpc]
    private void NotifyMovementClientRpc(int x, int y, bool move)
    {
        if (IsHost) return;

        var toPosition = new Vector2Int(x, y);
        var tile = _mapHandler.GetMap()[toPosition];

        if (move)
        {
            _activeUnit.MoveToTile(tile);
        }
    }

    public void ButtonCheckAction()
    {
        CheckAction(_target);
    }

    //// <returns>false if no action has been performed</returns>
    public void CheckAction(PlayerGridMovement target)
    {
        // Clicked on top of a Unit
        if (_activeUnit.IsOppositeTeam(target))
        {
            Debug.Log("Target is an enemy of active unit");
            CheckActionOnOppositeTeam(target);
        }
    }

    private void CheckActionOnOppositeTeam(PlayerGridMovement characterOnTile)
    {
        // Can Attack Enemy
        if (!_canAttackThisTurn)
        {
            Debug.Log("Already attacked in this turn");
            //_playerUI.ShowMessage("Already attacked in this turn.");
            return;
        }

        // Clicked on an Enemy of the current unit
        var weapon = GetActiveUnitWeapon();
        /*if (!weapon)
        {
            weapon = ScriptableObject.CreateInstance<Weapon>();
            weapon.range = 1;
            weapon.weaponType = Weapon.WeaponType.Melee;
            weapon.damage = 1;
        }*/

        if (!IsWithinRange(characterOnTile, weapon))
        {
            Debug.Log("Target outside range of weapon");
            //_playerUI.ShowMessage("Target outside range of weapon.");
            return;
        }

        if (!weapon.CanFire())
        {
            Debug.Log("No ammo");
            _playerUI.ShowMessage("No ammo.");
            return;
        }

        // Attack Enemy
        _canAttackThisTurn = false;
        Attack(characterOnTile, weapon);
    }

    private Weapon GetActiveUnitWeapon()
    {
        var weapon = _activeUnit.GetTeam() == PlayerGridMovement.Team.Players
            ? InventoryManager.Instance.GetWeapon()
            : _activeUnit.GetComponent<EnemyGridBehaviour>().weapon;

        //for testing
        if (!weapon)
        {
            weapon = ScriptableObject.CreateInstance<Weapon>();
            weapon.range = 1;
            weapon.weaponType = Weapon.WeaponType.Melee;
            weapon.damage = 1;
        }

        return weapon;
    }

    public bool IsWithinRange(PlayerGridMovement target, Weapon weapon)
    {
        Debug.Log("Distance " + Vector2Int.Distance(_activeUnit.onTile.mapPosition, target.onTile.mapPosition) + ", WeaponRange " + weapon.range);
        return Vector2Int.Distance(_activeUnit.onTile.mapPosition, target.onTile.mapPosition) <= weapon.range;
    }

    public void Attack(PlayerGridMovement target, Weapon weapon)
    {
        Debug.Log("Attack!");
        // TODO: add UI feedback

        if (weapon.weaponType == Weapon.WeaponType.Range)
        {
            weapon.UseAmmo();
            _playerUI.SetAmmoCounter(weapon.GetAmmo()); // reduce UI counter by 1
        }

        if (target.GetTeam() == PlayerGridMovement.Team.Enemies)
        {
            var damage = (int)(weapon.damage + CharacterManager.Instance.GetTotalStr());
            NotifyAttackToEnemy(target, damage);
        }
        else
        {
            NotifyAttackToPlayer(target, (int)weapon.damage);
        }
    }

    private void NotifyAttackToEnemy(PlayerGridMovement target, int damage)
    {
        target.GetComponent<EnemyGridBehaviour>().Damage(damage);
        target.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);

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
        characterList[targetIndex].GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
    }

    [ClientRpc]
    private void NotifyAttackFromHostToEnemyClientRpc(int targetIndex, int damage)
    {
        if (IsHost) return;
        characterList[targetIndex].GetComponent<EnemyGridBehaviour>().Damage(damage);
        characterList[targetIndex].GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
    }

    private void NotifyAttackToPlayer(PlayerGridMovement target, int damage)
    {
        //todo enemy attack player test dopo UI
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (target.gameObject == localPlayer)
        {
            CharacterManager.Instance.Damage(damage);
            localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
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
            localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
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
            localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
        }
    }

    public void SkipTurn()
    {
        //todo controllare che il bottone di skip faccia il controllo commentato e non questo metodo
        /* return;*/
        
        if (IsHost)
        {
            HostHasSkippedClientRpc();
            SelectNextActiveUnit();
            _playerUI.SetCombatPopUp(false);
            _turnUI.NextTurn();
        }
        else
        {
            ClientHasSkippedServerRpc();
            SelectNextActiveUnit();
            _playerUI.SetCombatPopUp(false);
            _turnUI.NextTurn();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientHasSkippedServerRpc()
    {
        if (!IsHost) return;

        SelectNextActiveUnit();
        //_playerUI.SetCombatPopUp(false);
        _turnUI.NextTurn();
    }

    [ClientRpc]
    private void HostHasSkippedClientRpc()
    {
        if (IsHost) return;

        SelectNextActiveUnit();
        //_playerUI.SetCombatPopUp(false);
        _turnUI.NextTurn();
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
            _turnUI.NextTurn();

            //todo null se nemico è il primo _turnUI.NextTurn();
            //TODO: perche?
        }
        else
        {
            ClientHasSkippedServerRpc();
            SelectNextActiveUnit();
            _turnUI.NextTurn();

            //todo null se nemico è il primo _turnUI.NextTurn();
            //TODO: perche?
        }
    }

    private IEnumerator SetUpTurns()
    {
        GetPortraitSprite();
        //yield return new WaitUntil(otherPlayerSpriteIdx != null);
        while (otherPlayerSpriteIdx == null)
        {
            yield return new WaitForSeconds(0.2f);
        }

        _playerUI.ToggleCombatUI(characterList);
        _turnUI = _playerUI.GetCombatUI().GetTurnUI();
        _isUIReady = true;
    }

    public void GetPortraitSprite()
    {
        if (IsHost)
        {
            //Debug.Log("I'm host and i'm sending idx: " + _playerUI.portraitIdx);
            GetPortraitSpriteClientRpc(_playerUI.portraitIdx);
        }
        else
        {
            //Debug.Log("I'm client and i'm sending idx: " + _playerUI.portraitIdx);
            GetPortraitSpriteServerRpc(_playerUI.portraitIdx);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetPortraitSpriteServerRpc(int portraitIdx)
    {
        if (!IsHost) return;
        //Debug.Log("I'm host and i received: " + portraitIdx);
        otherPlayerSpriteIdx = portraitIdx;
        //Debug.Log("host: my portraitIdx=" + _playerUI.portraitIdx + " otherPlayerIdx=" + otherPlayerSpriteIdx);
    }

    [ClientRpc]
    private void GetPortraitSpriteClientRpc(int portraitIdx)
    {
        if (IsHost) return;
        //Debug.Log("I'm client and i received: " + portraitIdx);
        otherPlayerSpriteIdx = portraitIdx;
        //Debug.Log("client: my portraitIdx=" + _playerUI.portraitIdx + " otherPlayerIdx=" + otherPlayerSpriteIdx);
    }
}