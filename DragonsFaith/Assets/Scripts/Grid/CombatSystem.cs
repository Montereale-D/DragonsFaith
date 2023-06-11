using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Inventory.Items;
using Player;
using Save;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatSystem : NetworkBehaviour
{
    public static CombatSystem instance;
    [HideInInspector] public List<PlayerGridMovement> characterList;
    [HideInInspector] public List<Obstacle> obstacleList;
    private int _indexCharacterTurn = -1;

    public PlayerGridMovement activeUnit { get; private set; }
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

    private GameObject _localPlayer;

    [SerializeField] private LayerMask coverLayerMaskHit;
    /*private float _turnDelay; //needs to be the same as the length of the turn UI animation
    private float _turnDelayCounter;*/

    #region Core

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;

        _playerUI = PlayerUI.instance;
        _mapHandler = MapHandler.instance;
        _localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        /*_turnDelay = 2.1f;
        _turnDelayCounter = 0;*/
    }

    public void Setup(IEnumerable<PlayerGridMovement> characters, IEnumerable<Obstacle> obstacles)
    {
        characterList = characters.OrderByDescending(x => x.movement)
            .ThenBy(x => x.name)
            .ThenBy(x => x.GetHashCode())
            .ToList();

        obstacleList = obstacles.ToList();

        StartCoroutine(SetUpUITurns());
        StartCoroutine(WaitSetupToStart());

        //_isCombatReady = true;
    }

    private IEnumerator SetUpUITurns()
    {
        SendPortraitSprite();
        while (otherPlayerSpriteIdx == null)
        {
            yield return new WaitForSeconds(0.2f);
        }

        _playerUI.ToggleCombatUI(characterList);
        _turnUI = _playerUI.GetCombatUI().GetTurnUI();
        _isUIReady = true;
    }

    private IEnumerator WaitSetupToStart()
    {
        //while (!_isCombatReady || !_isUIReady)
        while (!_isUIReady)
        {
            yield return new WaitForSeconds(0.2f);
        }

        var localPlayer =
            NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerGridMovement>();

        var hostPos = SpawnPointerGrid.instance.GetPlayerSpawnPoint(GameData.PlayerType.Host);
        var clientPos = SpawnPointerGrid.instance.GetPlayerSpawnPoint(GameData.PlayerType.Client);
        var enemiesPos = SpawnPointerGrid.instance.GetEnemySpawnPoint();
        var enemyPosIndex = 0;
        var obstaclesPos = SpawnPointerGrid.instance.GetObstaclesSpawnPoint();
        var obstaclePosIndex = 0;

        foreach (var obstacle in obstacleList)
        {
            obstacle.SetGridPosition(obstaclesPos[obstaclePosIndex]);
            obstaclePosIndex++;
        }


        foreach (var character in characterList)
        {
            if (character.GetTeam() == PlayerGridMovement.Team.Enemies)
            {
                character.SetGridPosition(enemiesPos[enemyPosIndex]);
                enemyPosIndex++;
            }
            else
            {
                if (character == localPlayer)
                {
                    character.SetGridPosition(NetworkManager.Singleton.IsHost ? hostPos : clientPos);
                }
                else
                {
                    character.SetGridPosition(NetworkManager.Singleton.IsHost ? clientPos : hostPos);
                }
            }
        }

        _mapHandler.HideAllTiles();
        ResetTurnActions();
    }

    public static void SetTileUnderCharacter(PlayerGridMovement playerGridMovement)
    {
        if (!playerGridMovement.onTile)
        {
            Debug.LogError("No c.onTile found");
            return;
        }

        playerGridMovement.onTile.SetCharacterOnTile(playerGridMovement);
    }

    private void Update()
    {
        //wait for settings
        //if (!_isCombatReady || !_isUIReady) return;
        if (!_isUIReady) return;

        _mapHandler.HideAllTiles();

        /*if (_turnDelayCounter > 0)
        {
            _turnDelayCounter -= Time.deltaTime;
            return;
        }*/

        if (!_isThisPlayerTurn) return;

        if (_canMoveThisTurn) _mapHandler.ShowNavigableTiles(activeUnit.onTile, activeUnit.movement);
        if (_selectedTile)
        {
            _selectedTile.SelectTile();
        }

        //check if mouse is hovering at least one tile, then check player action
        var hoveredHit = _mapHandler.GetHoveredRaycast();
        if (hoveredHit.HasValue)
        {
            var tile = hoveredHit.Value.collider.GetComponent<Tile>();
            if (tile)
            {
                tile.HoverTile();

                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    SelectTile(tile);
                }
            }

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

    private void SelectNextActiveUnit()
    {
        ResetTurnActions();
        _turnUI.NextTurn();
        /*_turnDelayCounter = _turnDelay;*/
    }

    private void ResetTurnActions()
    {
        activeUnit = GetNextActiveUnit();

        Debug.Log("Turn of " + activeUnit.name);

        _canMoveThisTurn = true;
        _canAttackThisTurn = true;
        _selectedTile = null;
        activeUnit.GetComponent<CharacterInfo>().isBlocking = false;

        if (activeUnit.GetTeam() == PlayerGridMovement.Team.Players &&
            activeUnit.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            _isThisPlayerTurn = true;
            //_playerUI.SetMovementCounter(_activeUnit.movement);
        }
        else
        {
            _isThisPlayerTurn = false;

            //if the active unity is enemy and I am host, I should notify the enemy
            if (activeUnit.GetTeam() == PlayerGridMovement.Team.Enemies && NetworkManager.Singleton.IsHost)
            {
                activeUnit.GetComponent<EnemyGridBehaviour>().PlanAction(characterList);
            }
        }
    }

    private PlayerGridMovement GetNextActiveUnit()
    {
        var candidate = NextUnit();

        if (candidate.GetTeam() == PlayerGridMovement.Team.Players &&
            !candidate.GetComponent<CharacterInfo>().IsAlive())
        {
            Debug.Log("The selected player is dead");

            //TODO: this way, the dead player's turn is completely skipped, the animation too
            // there needs to be added a delay so that the animation is still visible
            _turnUI.NextTurn();
            /*_turnDelayCounter = _turnDelay;
            while (_turnDelayCounter > 0){}*/

            candidate = NextUnit(); //assert only a player can be dead
        }
        else
        {
            Debug.Log("The selected player is alive");
        }

        return candidate;
    }

    private PlayerGridMovement NextUnit()
    {
        _indexCharacterTurn++;

        if (_indexCharacterTurn >= characterList.Count)
            _indexCharacterTurn = 0;

        return characterList[_indexCharacterTurn];
    }

    #endregion

    public void CharacterDied(PlayerGridMovement character)
    {
        _turnUI.OnDeath(character);

        if (character.GetTeam() == PlayerGridMovement.Team.Players)
        {
            var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
            if (players.Count(x => !x.GetComponent<CharacterInfo>().IsAlive()) > 1)
            {
                GameHandler.instance.GameOver();
            }

            return;
        }

        characterList.Remove(character);
        _indexCharacterTurn = characterList.IndexOf(activeUnit);

        if (!characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Enemies).Any())
        {
            GameHandler.instance.CombatWin();
        }
    }

    public void ReloadAction()
    {
        if (!_canAttackThisTurn)
        {
            _playerUI.ShowMessage("Action already done");
            return;
        }

        var weapon = GetActiveUnitWeapon();
        if (weapon.IsFullyLoaded())
        {
            _playerUI.ShowMessage("Weapon already fully loaded.");
            return;
        }

        weapon.Reload();

        if (activeUnit == _localPlayer.GetComponent<PlayerGridMovement>())
        {
            _playerUI.SetAmmoCounter(weapon.GetAmmo());
        }

        _canAttackThisTurn = false;
    }

    #region BlockAction

    public void BlockAction()
    {
        if (!_canAttackThisTurn)
        {
            _playerUI.ShowMessage("Action already done");
            return;
        }
        
        NotifyBlock();
        activeUnit.GetComponent<CharacterInfo>().isBlocking = true;
        _canAttackThisTurn = false;
    }

    private void NotifyBlock()
    {
        if (IsHost)
        {
            NotifyBlockClientRpc();
        }
        else
        {
            NotifyBlockServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyBlockServerRpc()
    {
        if (!IsHost) return;

        activeUnit.GetComponent<CharacterInfo>().isBlocking = true;
    }

    [ClientRpc]
    private void NotifyBlockClientRpc()
    {
        if (IsHost) return;

        activeUnit.GetComponent<CharacterInfo>().isBlocking = true;
    }

    #endregion

    #region ReviveAction

    public void PlayerReviveAction()
    {
        var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
        var otherPlayer = players[0] == activeUnit ? players[1] : players[0];
        otherPlayer.GetComponent<CharacterInfo>().Revive();
        _turnUI.OnRevive(otherPlayer);
        _canAttackThisTurn = false;
    }

    private void ReceiveRevive()
    {
        CharacterManager.Instance.ReceiveRevive();
        _turnUI.OnRevive(_localPlayer.GetComponent<PlayerGridMovement>());
    }

    public void NotifyRevive()
    {
        if (IsHost)
        {
            NotifyReviveClientRpc();
        }
        else
        {
            NotifyReviveServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyReviveServerRpc()
    {
        if (!IsHost) return;

        ReceiveRevive();
    }

    [ClientRpc]
    private void NotifyReviveClientRpc()
    {
        if (IsHost) return;

        ReceiveRevive();
    }

    #endregion

    #region Tile

    private void SelectTile(Tile tile)
    {
        if (_selectedTile)
        {
            UnselectTile();
        }

        _selectedTile = tile;
        _selectedTile.SelectTile();

        var character = _selectedTile.GetCharacter();
        var obstacle = _selectedTile.GetObstacle();
        if (character)
        {
            if (character != activeUnit)
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
                    _playerUI.ToggleMoveAttackButton("Attack");
                }
            }
        }
        //clicked on an obstacle
        else if (obstacle)
        {
            var weapon = GetActiveUnitWeapon();
            if (!_canAttackThisTurn)
            {
                _playerUI.SetCombatPopUp(true, "Already attacked this turn.");
            }
            else if (!IsWithinRange(obstacle, weapon))
            {
                _playerUI.SetCombatPopUp(true, "Target outside weapon range.");
            }
            else
            {
                if (obstacle.destroyable)
                {
                    _playerUI.SetCombatPopUp(true,
                        "Target is within weapon range " + weapon.range + "." + System.Environment.NewLine +
                        "Strike object to destroy it");
                    _playerUI.ToggleMoveAttackButton("Destroy");
                }
                else
                {
                    _playerUI.SetCombatPopUp(true,
                        "This obstacle can't be destroyed");
                }
            }
        }
        else
        {
            if (!_canMoveThisTurn)
            {
                _playerUI.SetCombatPopUp(true, "Already moved this turn.");
            }
            else if (!_mapHandler.GetTilesInRange(activeUnit.onTile, activeUnit.movement).Contains(_selectedTile))
            {
                _playerUI.SetCombatPopUp(true, "Cell is too far away.");
            }
            else
            {
                _playerUI.SetCombatPopUp(true, "Cell is within movement range " + activeUnit.movement + ".");
            }

            _playerUI.ToggleMoveAttackButton("Move");
        }
    }

    private void UnselectTile()
    {
        _selectedTile.ShowTile();
        _playerUI.SetCombatPopUp(false);
        var character = _selectedTile.GetCharacter();
        if (character && character != activeUnit)
        {
            // hides the UI of the enemies on deselection of the cell
            character.GetComponent<CharacterGridPopUpUI>().HideUI();
        }

        _selectedTile = null;
    }

    #endregion

    #region Movement

    public void ButtonCheckMovement()
    {
        PerformPlayerMovement(_selectedTile);
    }

    public void PerformPlayerMovement(Tile toTile)
    {
        var isValidMovement = CheckMovement(toTile);
        if (!isValidMovement) return;

        PerformMovement(toTile, true);

        if (IsHost)
        {
            NotifyMovementClientRpc(toTile.mapPosition.x, toTile.mapPosition.y, false);
        }
        else
        {
            NotifyMovementServerRpc(toTile.mapPosition.x, toTile.mapPosition.y);
        }
    }

    public void PerformEnemyMovement(Tile toTile)
    {
        if (!IsHost)
        {
            Debug.LogWarning("Client should not be here");
            return;
        }

        var isValidMovement = CheckMovement(toTile);
        if (!isValidMovement) return;

        PerformMovement(toTile, true);

        NotifyMovementClientRpc(toTile.mapPosition.x, toTile.mapPosition.y, true);
    }

    private bool CheckMovement(Tile toTile)
    {
        if (!_mapHandler.GetTilesInRange(activeUnit.onTile, activeUnit.movement).Contains(toTile))
        {
            //_playerUI.ShowMessage("Cell is too far away.");
            //_playerUI.SetCombatPopUp(true, "Cell is too far away.");
            return false;
        }

        if (!_canMoveThisTurn)
        {
            //_playerUI.ShowMessage("Already moved this turn.");
            return false;
        }

        if (!toTile.navigable)
        {
            Debug.Log("You cannot go there because of obstacle");
            return false;
        }

        return true;
    }

    private void PerformMovement(Tile toTile, bool animatePath)
    {
        _canMoveThisTurn = false;
        //_playerUI.SetMovementCounter(0);

        // Set entire Tilemap to Invisible
        _mapHandler.HideAllTiles();

        // Remove Unit from tile
        activeUnit.onTile.ClearTile();

        // Set Unit on target Grid Object
        toTile.SetCharacterOnTile(activeUnit);

        if (animatePath)
        {
            activeUnit.MoveToTile(toTile);
        }
        else
        {
            activeUnit.SetTile(toTile);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyMovementServerRpc(int x, int y)
    {
        if (!IsHost) return;

        var toPosition = new Vector2Int(x, y);
        var tile = _mapHandler.GetMap()[toPosition];

        PerformMovement(tile, false);
    }

    [ClientRpc]
    private void NotifyMovementClientRpc(int x, int y, bool updatePosition)
    {
        if (IsHost) return;

        var toPosition = new Vector2Int(x, y);
        var tile = _mapHandler.GetMap()[toPosition];

        PerformMovement(tile, updatePosition);
    }

    #endregion

    #region AttackAction

    public void ButtonAttackAction()
    {
        CheckAction(_target);
    }


    //// <returns>false if no action has been performed</returns>
    public void CheckAction(PlayerGridMovement target, Tile fromTile = null)
    {
        // Clicked on top of a Unit
        if (!activeUnit.IsOppositeTeam(target))
        {
            Debug.Log("Target is an ally of active unit");
            //_playerUI.ShowMessage("Cannot attack an ally");
            return;
        }

        // Can Attack Enemy
        if (!_canAttackThisTurn)
        {
            Debug.Log("Already attacked in this turn");
            //_playerUI.ShowMessage("Already attacked in this turn.");
            return;
        }

        var weapon = GetActiveUnitWeapon();

        if (fromTile)
        {
            if (!IsWithinRange(target, fromTile, weapon))
            {
                Debug.Log("Target outside range of weapon");
                //_playerUI.ShowMessage("Target outside range of weapon.");
                return;
            }
        }
        else
        {
            if (!IsWithinRange(target, weapon))
            {
                Debug.Log("Target outside range of weapon");
                //_playerUI.ShowMessage("Target outside range of weapon.");
                return;
            }
        }


        if (!weapon.CanFire())
        {
            Debug.Log("No ammo");
            _playerUI.ShowMessage("No ammo.");
            return;
        }

        // Attack Enemy
        _canAttackThisTurn = false;
        Attack(target, weapon);
    }

    private void Attack(PlayerGridMovement target, Weapon weapon)
    {
        Debug.Log("Attack!");

        if (weapon.weaponType == Weapon.WeaponType.Range)
        {
            weapon.UseAmmo();
            _playerUI.SetAmmoCounter(weapon.GetAmmo()); // reduce UI counter by 1
        }

        var isCovered = IsTargetCovered(target);

        if (target.GetTeam() == PlayerGridMovement.Team.Enemies)
        {
            var damage = (int)(weapon.damage + CharacterManager.Instance.GetTotalStr());

            if (target.GetComponent<CharacterInfo>().isBlocking)
            {
                damage /= 2;
            }

            if (isCovered)
            {
                damage /= 2;
            }

            NotifyAttackToEnemy(target, damage);
        }
        else
        {
            var damage = weapon.damage;

            if (target.GetComponent<CharacterInfo>().isBlocking)
            {
                damage /= 2;
            }

            if (isCovered)
            {
                damage /= 2;
            }

            NotifyAttackToPlayer(target, (int)damage);
        }
    }

    private bool IsTargetCovered(PlayerGridMovement target)
    {
        Vector2 from = activeUnit.onTile.transform.position;
        Vector2 to = target.onTile.transform.position;
        Debug.Log("Raycast from " + from + " to " + to);

        var targetLayer = LayerMask.LayerToName(target.gameObject.layer);
        Debug.Log("Enemy layer is " + targetLayer);


        //var layerMask = LayerMask.GetMask(targetLayer, "Walls", "Obstacles");

        /*layerMask |= (1 << LayerMask.GetMask("Obstacles"));
        layerMask |= (1 << LayerMask.GetMask("Walls"));
        layerMask |= (1 << target.gameObject.layer);*/

        var hit = Physics2D.Raycast(from, (to - from).normalized, Mathf.Infinity, coverLayerMaskHit);

        if (!hit)
        {
            Debug.Log("RayCast is null");
            return false;
        }

        if (hit.transform)
        {
            Debug.Log("Hit " + hit.transform.name);
        }

        if (!hit.collider)
        {
            Debug.Log("Hit has no collider");
            return false;
        }

        if (hit.transform.gameObject.layer == target.gameObject.layer)
        {
            Debug.Log("RayCast hit target: " + gameObject.name + " with layer " +
                      LayerMask.LayerToName(gameObject.layer));
            return false;
        }

        Debug.Log("RayCast hit " + hit.transform.gameObject.name);
        var adjObstaclesPos = new Vector2(hit.transform.position.x, hit.transform.position.y);
        var adjTargetPos = new Vector2(target.transform.position.x, target.transform.position.y);

        if (Vector2.Distance(adjObstaclesPos, adjTargetPos) < 1.5f)
        {
            Debug.Log("RayCastHit distance to target is < 1.5f");
            return true;
        }

        Debug.Log("IsCovered return false");

        return false;
    }

    private void NotifyAttackToPlayer(PlayerGridMovement target, int damage)
    {
        var targetIndex = characterList.IndexOf(target);
        //var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        if (IsHost)
        {
            NotifyAttackFromEnemyToPlayerClientRpc(targetIndex, damage);
        }
        else
        {
            NotifyAttackFromEnemyToPlayerServerRpc(targetIndex, damage);
        }

        if (target.gameObject == _localPlayer)
        {
            CharacterManager.Instance.Damage(damage);
            _localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
        }
        else
        {
            target.GetComponent<CharacterInfo>().Damage(damage);
        }
    }

    public void NotifyAttackToEnemy(PlayerGridMovement target, int damage)
    {
        var targetIndex = characterList.IndexOf(target);
        target.GetComponent<EnemyGridBehaviour>().Damage(damage);
        target.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);

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


    [ServerRpc(RequireOwnership = false)]
    private void NotifyAttackFromEnemyToPlayerServerRpc(int targetIndex, int damage)
    {
        if (!IsHost) return;

        //var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (characterList[targetIndex].gameObject == _localPlayer)
        {
            CharacterManager.Instance.Damage(damage);
            _localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
        }
        else
        {
            characterList[targetIndex].GetComponent<CharacterInfo>().Damage(damage);
        }
    }

    [ClientRpc]
    private void NotifyAttackFromEnemyToPlayerClientRpc(int targetIndex, int damage)
    {
        if (IsHost) return;

        //var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        if (characterList[targetIndex].gameObject == _localPlayer)
        {
            CharacterManager.Instance.Damage(damage);
            _localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
        }
        else
        {
            characterList[targetIndex].GetComponent<CharacterInfo>().Damage(damage);
        }
    }

    public void ButtonDestroyAction()
    {
        DestroyObstacle(_selectedTile.GetObstacle());
    }

    public void DestroyObstacle(Obstacle o)
    {
        if (o == null) Debug.Log("Something went wrong");
        o.onTile.ClearTile();
        _canAttackThisTurn = false;
        Destroy(o.gameObject);
    }

    #endregion

    #region SkipAction

    public void SkipTurn()
    {
        if (IsHost)
        {
            HostHasSkippedClientRpc();
            SelectNextActiveUnit();
            _playerUI.SetCombatPopUp(false);
        }
        else
        {
            ClientHasSkippedServerRpc();
            SelectNextActiveUnit();
            _playerUI.SetCombatPopUp(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientHasSkippedServerRpc()
    {
        if (!IsHost) return;

        SelectNextActiveUnit();
        //_playerUI.SetCombatPopUp(false);
    }

    [ClientRpc]
    private void HostHasSkippedClientRpc()
    {
        if (IsHost) return;

        SelectNextActiveUnit();
        //_playerUI.SetCombatPopUp(false);
    }

    #endregion

    #region TurnUI

    private void SendPortraitSprite()
    {
        if (IsHost)
        {
            //Debug.Log("I'm host and i'm sending idx: " + _playerUI.portraitIdx);
            SendPortraitSpriteClientRpc(_playerUI.portraitIdx);
        }
        else
        {
            //Debug.Log("I'm client and i'm sending idx: " + _playerUI.portraitIdx);
            SendPortraitSpriteServerRpc(_playerUI.portraitIdx);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPortraitSpriteServerRpc(int portraitIdx)
    {
        if (!IsHost) return;
        //Debug.Log("I'm host and i received: " + portraitIdx);
        otherPlayerSpriteIdx = portraitIdx;
        //Debug.Log("host: my portraitIdx=" + _playerUI.portraitIdx + " otherPlayerIdx=" + otherPlayerSpriteIdx);
    }

    [ClientRpc]
    private void SendPortraitSpriteClientRpc(int portraitIdx)
    {
        if (IsHost) return;
        //Debug.Log("I'm client and i received: " + portraitIdx);
        otherPlayerSpriteIdx = portraitIdx;
        //Debug.Log("client: my portraitIdx=" + _playerUI.portraitIdx + " otherPlayerIdx=" + otherPlayerSpriteIdx);
    }

    #endregion

    #region Helper

    private bool IsWithinRange(PlayerGridMovement target, Weapon weapon)
    {
        Debug.Log("Distance " + PlayerGridMovement.GetManhattanDistance(activeUnit.onTile, target.onTile) +
                  ", WeaponRange " + weapon.range);
        return PlayerGridMovement.GetManhattanDistance(activeUnit.onTile, target.onTile) <= weapon.range;
    }

    private bool IsWithinRange(Obstacle target, Weapon weapon)
    {
        Debug.Log("Distance " + PlayerGridMovement.GetManhattanDistance(activeUnit.onTile, target.onTile) +
                  ", WeaponRange " + weapon.range);
        return PlayerGridMovement.GetManhattanDistance(activeUnit.onTile, target.onTile) <= weapon.range;
    }

    private bool IsWithinRange(PlayerGridMovement target, Tile tile, Weapon weapon)
    {
        Debug.Log("Distance " + PlayerGridMovement.GetManhattanDistance(tile, target.onTile) +
                  ", WeaponRange " + weapon.range);
        return PlayerGridMovement.GetManhattanDistance(tile, target.onTile) <= weapon.range;
    }

    private Weapon GetActiveUnitWeapon()
    {
        var weapon = activeUnit.GetTeam() == PlayerGridMovement.Team.Players
            ? InventoryManager.Instance.GetWeapon()
            : activeUnit.GetComponent<EnemyGridBehaviour>().weapon;

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

    #endregion

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