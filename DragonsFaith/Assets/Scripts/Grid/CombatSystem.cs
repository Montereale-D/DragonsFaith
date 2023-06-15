using System;
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

namespace Grid
{
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
        private bool _isUsingSkill;
        private Tile _selectedTile;

        private PlayerGridMovement _target;

        public int? otherPlayerSpriteIdx;
        [HideInInspector] public string otherPlayerName;
        private PlayerUI _playerUI;
        private MapHandler _mapHandler;
        private TurnUI _turnUI;

        private GameObject _localPlayer;

        [SerializeField] private LayerMask coverLayerMaskHit;
        /*private float _turnDelay; //needs to be the same as the length of the turn UI animation
        private float _turnDelayCounter;*/

        private List<Tile> _skillRange;

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
            SendPlayerName();
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

            var localPlayer = _localPlayer.GetComponent<PlayerGridMovement>();

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
                    //character.GetComponent<EnemyGridBehaviour>().SetUI();
                    enemyPosIndex++;
                }
                else
                {
                    if (character == localPlayer)
                    {
                        character.SetGridPosition(NetworkManager.Singleton.IsHost ? hostPos : clientPos);
                        NotifyPopUpInfoToPlayer(character.GetComponent<CharacterInfo>().characterName,
                            character.GetComponent<CharacterInfo>().GetMaxHealth());
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

        private void NotifyPopUpInfoToPlayer(string charName, int health)
        {
            if (IsHost)
            {
                NotifyPopUpInfoToPlayerClientRpc(charName, health);
            }
            else
            {
                NotifyPopUpInfoToPlayerServerRpc(charName, health);
            }
        }
    
        [ServerRpc(RequireOwnership = false)]
        private void NotifyPopUpInfoToPlayerServerRpc(string charName, int health)
        {
            if (!IsHost) return;
            var players = 
                characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
            foreach (var player in players.Where(player => player != _localPlayer.GetComponent<PlayerGridMovement>()))
            {
                player.GetComponent<CharacterGridPopUpUI>().SetUI(charName, health);
            }
        }

        [ClientRpc]
        private void NotifyPopUpInfoToPlayerClientRpc(string charName, int health)
        {
            if (IsHost) return;
            var players = 
                characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
            foreach (var player in players.Where(player => player != _localPlayer.GetComponent<PlayerGridMovement>()))
            {
                player.GetComponent<CharacterGridPopUpUI>().SetUI(charName, health);
            }
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

            if (_isUsingSkill)
            {
                foreach (var t in _skillRange)
                {
                    t.ShowTile();
                }
            }
            else if (_canMoveThisTurn) _mapHandler.ShowNavigableTiles(activeUnit.onTile, activeUnit.movement);
            
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

                // Right mouse click to deselect the tile
                if (Input.GetMouseButtonDown(1))
                {
                    UnselectTile();
                }
            }
            
            if (!_canAttackThisTurn && !_canMoveThisTurn) SkipTurn();
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
            _isUsingSkill = false;
            activeUnit.GetComponent<CharacterInfo>().isBlocking = false;
            activeUnit.GetComponent<CharacterGridPopUpUI>().HideShield();

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
                //character.OnDeath();
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

        #region ReloadAction
        
        public void ButtonReloadAction()
        {
            if (activeUnit != _localPlayer.GetComponent<PlayerGridMovement>()) return;
            ReloadAction();
        }
        
        public void ReloadAction()
        {
            if (!_canAttackThisTurn)
            {
                _playerUI.ShowMessage("Already performed action.");
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
                _playerUI.ShowMessage("Weapon reloaded.");
            }
            else if (activeUnit.GetTeam() == PlayerGridMovement.Team.Enemies)
            {
                NotifyEnemyReload();
            }

            _canAttackThisTurn = false;
        }

        private void NotifyEnemyReload()
        {
            activeUnit.GetComponent<EnemyGridBehaviour>().TriggerReloadAnimation();
        
            if (IsHost)
            {
                NotifyEnemyReloadToPlayerClientRpc();
            }
            else
            {
                NotifyEnemyReloadToPlayerServerRpc();
            }
        }
    
        [ServerRpc(RequireOwnership = false)]
        private void NotifyEnemyReloadToPlayerServerRpc()
        {
            if (!IsHost) return;
            activeUnit.GetComponent<EnemyGridBehaviour>().TriggerReloadAnimation();
        }

        [ClientRpc]
        private void NotifyEnemyReloadToPlayerClientRpc()
        {
            if (IsHost) return;
            activeUnit.GetComponent<EnemyGridBehaviour>().TriggerReloadAnimation();
        }
    
        #endregion

        #region BlockAction

        public void ButtonBlockAction()
        {
            if (activeUnit != _localPlayer.GetComponent<PlayerGridMovement>()) return;
            BlockAction();
        }

        public void BlockAction()
        {
            if (!_canAttackThisTurn)
            {
                _playerUI.ShowMessage("Already performed action.");
                return;
            }
            activeUnit.GetComponent<CharacterGridPopUpUI>().ShowShield();
        
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

            activeUnit.GetComponent<CharacterGridPopUpUI>().ShowShield();
            activeUnit.GetComponent<CharacterInfo>().isBlocking = true;
        }

        [ClientRpc]
        private void NotifyBlockClientRpc()
        {
            if (IsHost) return;

            activeUnit.GetComponent<CharacterGridPopUpUI>().ShowShield();
            activeUnit.GetComponent<CharacterInfo>().isBlocking = true;
        }

        #endregion

        #region ReviveAction

        public void PlayerReviveAction()
        {
            var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
            var otherPlayer = players[0] == activeUnit ? players[1] : players[0];
            otherPlayer.GetComponent<CharacterInfo>().Revive();
            //otherPlayer.OnRevive();
            _turnUI.OnRevive(otherPlayer);
            _canAttackThisTurn = false;
        }

        private void ReceiveRevive()
        {
            CharacterManager.Instance.ReceiveRevive();
            //_localPlayer.GetComponent<PlayerGridMovement>().OnRevive();
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
        
            // Turns character toward the selected tile
            activeUnit.TurnTowardTile(_selectedTile);
            _playerUI.SkillButtonAction("Show");
            _isUsingSkill = false;

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
                        _playerUI.SetCombatPopUp(true, "Target is an ally.");
                    }
                    else
                    {
                        // Clicked on an Enemy of the current unit
                        var weapon = GetActiveUnitWeapon();
                        var text = weapon.weaponType == Weapon.WeaponType.Melee ? 
                            (int)(weapon.damage + CharacterManager.Instance.GetTotalStr()) :
                            (int)(weapon.damage + CharacterManager.Instance.GetTotalDex());

                        if (!_canAttackThisTurn)
                        {
                            _playerUI.SetCombatPopUp(true, "Already performed action.");
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
                                "Strike target and deal " + text + " DMG.");
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
                    _playerUI.SetCombatPopUp(true, "Already performed action.");
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
                    _playerUI.SetCombatPopUp(true, "Cell outside movement range.");
                }
                else
                {
                    _playerUI.SetCombatPopUp(true, "Cell within movement range " + activeUnit.movement + ".");
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
            if (activeUnit != _localPlayer.GetComponent<PlayerGridMovement>()) return;
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
            Debug.Log("PerformEnemyMovement");
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

        public bool CanProceedToAction()
        {
            return !_moveInProgress && !_attackInProgress;
        }

        public bool _moveInProgress;
        
        private void PerformMovement(Tile toTile, bool animatePath)
        {
            _moveInProgress = true;
            
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
                _moveInProgress = false;
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


        //The demo has only Fire and Air, with both these skills using a cone AOE;
        //the switch is "useless" because of this, but would be needed if other faiths are included
        public void CheckSkillRange()
        {
            if (!_selectedTile) _playerUI.ShowMessage("A cell needs to be selected before using a skill.");

            var distance = PlayerGridMovement.GetManhattanDistance(activeUnit.onTile, _selectedTile);
            if (distance >= 5) _playerUI.SetCombatPopUp(true, "Cell outside skill range.");

            //I chose 4 as fixed value because it produces a nice AOE without being able to reach targets too far in a straight line; 3 should be tested too

            var searchable = MapHandler.instance.GetTilesInRange(activeUnit.onTile, 4);
            switch (PlayerUI.instance.chosenFaith)
            {
                //Exhale a fiery breath in a cone area that deals fire damage and has a chance of setting enemies on fire.
                case PlayerUI.Element.Fire:
                    {
                        _skillRange = ConeAttack(searchable);
                         _isUsingSkill = true;
                        _playerUI.SkillButtonAction("Unleash");
                        break;
                    }

                //not available in demo
                case PlayerUI.Element.Water:
                    {
                        break;
                    }

                //Launch an air wave in a cone area that deals damage and pushes away enemies. Has a chance to make enemies fall to the ground
                case PlayerUI.Element.Air:
                    {
                        _skillRange = ConeAttack(searchable);
                            _isUsingSkill = true;
                        _playerUI.SkillButtonAction("Unleash");
                        break;
                    }

                //not available in demo
                case PlayerUI.Element.Earth:
                    {
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CheckSkillAttack()
        {
            if (!_isUsingSkill || !_canAttackThisTurn) return;
            _isUsingSkill = false;
            _canAttackThisTurn = false;
            switch (PlayerUI.instance.chosenFaith)
            {
                //Exhale a fiery breath in a cone area that deals fire damage and has a chance of setting enemies on fire.
                case PlayerUI.Element.Fire:
                    {
                        foreach (var t in _skillRange)
                        {
                            if (t.GetCharacter() == null) continue;
                            if (!activeUnit.IsOppositeTeam(t.GetCharacter())) continue;
                            var damage = (int)CharacterManager.Instance.GetTotalStr() + (int)CharacterManager.Instance.GetTotalAgi();
                            NotifyAttackToEnemy(t.GetCharacter(), damage, "Skill", "Fire");
                            Debug.Log("Enemy hit by skill");
                        }
                        break;
                    }

                //not available in demo
                case PlayerUI.Element.Water:
                    {
                        break;
                    }

                //Launch an air wave in a cone area that deals damage and pushes away enemies. Has a chance to make enemies fall to the ground
                case PlayerUI.Element.Air:
                    {
                        foreach (var t in _skillRange)
                        {
                            if (t.GetCharacter() == null) continue;
                            if (!activeUnit.IsOppositeTeam(t.GetCharacter())) continue;
                            var damage = (int)CharacterManager.Instance.GetTotalDex() + (int)CharacterManager.Instance.GetTotalAgi();
                            NotifyAttackToEnemy(t.GetCharacter(), damage, "Skill", "Fire");
                            Debug.Log("Enemy hit by skill");
                        }
                      
                        break;
                    }

                //not available in demo
                case PlayerUI.Element.Earth:
                    {
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void ButtonAttackAction()
        {
            if (activeUnit != _localPlayer.GetComponent<PlayerGridMovement>()) return;
            CheckAction(_target);
        }
    
        //// <returns>false if no action has been performed</returns>
        public void CheckAction(PlayerGridMovement target, Tile fromTile = null)
        {
            // Clicked on top of a Unit
            if (!activeUnit.IsOppositeTeam(target))
            {
                //TODO: this code is never executed
                Debug.Log("Target is an ally of active unit");
                //_playerUI.ShowMessage("Cannot attack an ally");
                return;
            }

            // Can Attack Enemy
            if (!_canAttackThisTurn)
            {
                Debug.Log("Already performed action.");
                _playerUI.ShowMessage("Already performed action.");
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

        private bool _attackInProgress;
        private void Attack(PlayerGridMovement target, Weapon weapon)
        {
            Debug.Log("Attack!");
            _attackInProgress = true;

            if (weapon.weaponType == Weapon.WeaponType.Range)
            {
                weapon.UseAmmo();
                if (activeUnit == _localPlayer.GetComponent<PlayerGridMovement>()) _playerUI.SetAmmoCounter(weapon.GetAmmo()); // reduce UI counter by 1
            }

            var isCovered = IsTargetCovered(target);

            if (target.GetTeam() == PlayerGridMovement.Team.Enemies)
            {
                var damage = weapon.weaponType == Weapon.WeaponType.Melee ? 
                        (int)(weapon.damage + CharacterManager.Instance.GetTotalStr()) :
                        (int)(weapon.damage + CharacterManager.Instance.GetTotalDex());

                if (target.GetComponent<CharacterInfo>().isBlocking)
                {
                    //Debug.Log("target is blocking");
                    damage /= 2;
                }

                if (isCovered)
                {
                    //Debug.Log("target is covered");
                    damage /= 2;
                }

                //weapon.itemName;
                NotifyAttackToEnemy(target, damage, weapon.itemName);
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

            _attackInProgress = false;
        }

        private bool IsTargetCovered(PlayerGridMovement target)
        {
            Vector2 from = activeUnit.onTile.transform.position;
            Vector2 to = target.onTile.transform.position;
            Debug.Log("Raycast from " + from + " to " + to);

            var targetLayer = LayerMask.LayerToName(target.gameObject.layer);
            Debug.Log("Enemy layer is " + targetLayer);

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
            activeUnit.GetComponent<EnemyGridBehaviour>().TriggerAttackAnimation(target);
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
                //_localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
            }
            else
            {
                target.GetComponent<CharacterInfo>().Damage(damage);
            }
            target.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage, false);
        }

        public void NotifyAttackToEnemy(PlayerGridMovement target, int damage, string weaponName = "", string skillElement = "")
        {
            var targetIndex = characterList.IndexOf(target);
            target.GetComponent<EnemyGridBehaviour>().Damage(damage);
            target.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage, false);
            target.GetComponent<CharacterGridPopUpUI>().ShowSkillEffect(skillElement);
            activeUnit.GetComponent<PlayerGridMovement>().TriggerAttackAnimation(weaponName);

            if (IsHost)
            {
                NotifyAttackFromHostToEnemyClientRpc(targetIndex, damage, weaponName, skillElement);
            }
            else
            {
                NotifyAttackFromClientToEnemyServerRpc(targetIndex, damage, weaponName, skillElement);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void NotifyAttackFromClientToEnemyServerRpc(int targetIndex, int damage, string weaponName, string skillElement = "")
        {
            if (!IsHost) return;
            characterList[targetIndex].GetComponent<EnemyGridBehaviour>().Damage(damage);
            characterList[targetIndex].GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage, false);
            characterList[targetIndex].GetComponent<CharacterGridPopUpUI>().ShowSkillEffect(skillElement);
            activeUnit.GetComponent<PlayerGridMovement>().TriggerAttackAnimation(weaponName);
        }

        [ClientRpc]
        private void NotifyAttackFromHostToEnemyClientRpc(int targetIndex, int damage, string weaponName, string skillElement = "")
        {
            if (IsHost) return;
            characterList[targetIndex].GetComponent<EnemyGridBehaviour>().Damage(damage);
            characterList[targetIndex].GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage, false);
            characterList[targetIndex].GetComponent<CharacterGridPopUpUI>().ShowSkillEffect(skillElement);
            activeUnit.GetComponent<PlayerGridMovement>().TriggerAttackAnimation(weaponName);
        }


        [ServerRpc(RequireOwnership = false)]
        private void NotifyAttackFromEnemyToPlayerServerRpc(int targetIndex, int damage)
        {
            if (!IsHost) return;

            //var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
            activeUnit.GetComponent<EnemyGridBehaviour>().TriggerAttackAnimation(characterList[targetIndex]);

            if (characterList[targetIndex].gameObject == _localPlayer)
            {
                CharacterManager.Instance.Damage(damage);
                //_localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
            }
            else
            {
                characterList[targetIndex].GetComponent<CharacterInfo>().Damage(damage);
            }
            characterList[targetIndex].GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage, false);
        }

        [ClientRpc]
        private void NotifyAttackFromEnemyToPlayerClientRpc(int targetIndex, int damage)
        {
            if (IsHost) return;

            //var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
            activeUnit.GetComponent<EnemyGridBehaviour>().TriggerAttackAnimation(characterList[targetIndex]);

            if (characterList[targetIndex].gameObject == _localPlayer)
            {
                CharacterManager.Instance.Damage(damage);
                //_localPlayer.GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage);
            }
            else
            {
                characterList[targetIndex].GetComponent<CharacterInfo>().Damage(damage);
            }
            characterList[targetIndex].GetComponent<CharacterGridPopUpUI>().ShowDamageCounter(damage, false);
        }

        public void ButtonDestroyAction()
        {
            DestroyObstacle(_selectedTile.GetObstacle());
        }

        private void DestroyObstacle(Obstacle obstacle)
        {
            if (!obstacle)
            {
                Debug.LogError("Selected tile has no obstacle");
                return;
            }
        
            NotifyDestroyedObstacle(obstacle);
            obstacle.onTile.ClearTile();
            Destroy(obstacle.gameObject);

            _canAttackThisTurn = false;
        }

        private void NotifyDestroyedObstacle(Obstacle obstacle)
        {
            var pos = obstacle.onTile.mapPosition;
            if (IsHost)
            {
                DestroyedObstacleClientRpc(pos.x, pos.y);
            }
            else
            {
                DestroyedObstacleServerRpc(pos.x, pos.y);
            }
        }

        [ClientRpc]
        private void DestroyedObstacleClientRpc(int x, int y)
        {
            if(IsHost) return;
        
            var toPosition = new Vector2Int(x, y);
            var tile = _mapHandler.GetMap()[toPosition];

            var obstacle = tile.GetObstacle();
            tile.ClearTile();
            Destroy(obstacle.gameObject);
        }

        [ServerRpc (RequireOwnership = false)]
        private void DestroyedObstacleServerRpc(int x, int y)
        {
            if(!IsHost) return;
        
            var toPosition = new Vector2Int(x, y);
            var tile = _mapHandler.GetMap()[toPosition];
        
            var obstacle = tile.GetObstacle();
            tile.ClearTile();
            Destroy(obstacle.gameObject);
        }

        #endregion

        #region SkipAction

        public void ButtonSkipTurn()
        {
            if (activeUnit != _localPlayer.GetComponent<PlayerGridMovement>()) return;
            SkipTurn();
        }

        public void SkipTurn()
        {
            _isUsingSkill = false;
            if (IsHost)
            {
                //UnselectTile();
                _playerUI.SetCombatPopUp(false);
                // hides the ui of the selected char when skipping the turn
                if (activeUnit.GetTeam() != PlayerGridMovement.Team.Enemies && _selectedTile)
                {
                    var character = _selectedTile.GetCharacter();
                    if (character && character != activeUnit)
                    {
                        // hides the UI of the enemies on deselection of the cell
                        character.GetComponent<CharacterGridPopUpUI>().HideUI();
                    }
                }
                HostHasSkippedClientRpc();
                SelectNextActiveUnit();
            }
            else
            {
                //UnselectTile();
                _playerUI.SetCombatPopUp(false);
                // hides the ui of the selected char when skipping the turn
                if (activeUnit.GetTeam() != PlayerGridMovement.Team.Enemies && _selectedTile)
                {
                    var character = _selectedTile.GetCharacter();
                    if (character && character != activeUnit)
                    {
                        // hides the UI of the enemies on deselection of the cell
                        character.GetComponent<CharacterGridPopUpUI>().HideUI();
                    }
                }
                ClientHasSkippedServerRpc();
                SelectNextActiveUnit();
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
            otherPlayerSpriteIdx = portraitIdx;
        }

        [ClientRpc]
        private void SendPortraitSpriteClientRpc(int portraitIdx)
        {
            if (IsHost) return;
            otherPlayerSpriteIdx = portraitIdx;
        }
        
        private void SendPlayerName()
        {
            if (IsHost)
            {
                SendPlayerNameClientRpc(_playerUI.nameText.text);
            }
            else
            {
                SendPlayerNameServerRpc(_playerUI.nameText.text);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendPlayerNameServerRpc(string playerName)
        {
            if (!IsHost) return;
            otherPlayerName = playerName;
        }

        [ClientRpc]
        private void SendPlayerNameClientRpc(string playerName)
        {
            if (IsHost) return;
            otherPlayerName = playerName;
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
            if (weapon) return weapon;
            weapon = ScriptableObject.CreateInstance<Weapon>();
            weapon.range = 1;
            weapon.weaponType = Weapon.WeaponType.Melee;
            weapon.damage = 1;

            return weapon;
        }

    public List<Tile> ConeAttack(List<Tile> searchable)
    {
        List<Tile> aoe = new List<Tile>();
        List<Tile> upperCone = new List<Tile>();
        List<Tile> lowerCone = new List<Tile>();
        List<Tile> rightCone = new List<Tile>();
        List<Tile> leftCone = new List<Tile>();
        if (_selectedTile.mapPosition.y > activeUnit.onTile.mapPosition.y) { upperCone = createUpperCone(searchable); }
        else if (_selectedTile.mapPosition.y < activeUnit.onTile.mapPosition.y) { lowerCone = createLowerCone(searchable); }
        if (_selectedTile.mapPosition.x > activeUnit.onTile.mapPosition.x) { rightCone = createRightCone(searchable); }
        else if (_selectedTile.mapPosition.x < activeUnit.onTile.mapPosition.x) { leftCone = createLeftCone(searchable); }

        //this is used to pick the best cone (the one containing most enemies)
        int maxEnemiesInRange = 0;

        if (upperCone.Count > 0 )
        {
            int enemyInRange = 0; 
            foreach (Tile t in upperCone)
            {
                if (t.GetCharacter())
                {
                    if (activeUnit.IsOppositeTeam(t.GetCharacter())) enemyInRange++;
                }
            }
            if (enemyInRange >= maxEnemiesInRange)
            {
                maxEnemiesInRange = enemyInRange;
                aoe = upperCone;
            }
        }

        else if (lowerCone.Count > 0)
        {
            int enemyInRange = 0;
            foreach (Tile t in lowerCone)
            {
                if (t.GetCharacter())
                {
                    if (activeUnit.IsOppositeTeam(t.GetCharacter())) enemyInRange++;
                }
            }
            if (enemyInRange >= maxEnemiesInRange)
            {
                maxEnemiesInRange = enemyInRange;
                aoe = lowerCone;
            }
        }

        if (rightCone.Count > 0)
        {
            int enemyInRange = 0;
            foreach (Tile t in rightCone)
            {
                if (t.GetCharacter())
                {
                    if (activeUnit.IsOppositeTeam(t.GetCharacter())) enemyInRange++;
                }
            }
            if (enemyInRange >= maxEnemiesInRange)
            {
                maxEnemiesInRange = enemyInRange;
                aoe = rightCone;
            }
        }

        else if (leftCone.Count > 0)
        {
            int enemyInRange = 0;
            foreach (Tile t in leftCone)
            {
                if (t.GetCharacter())
                {
                    if (activeUnit.IsOppositeTeam(t.GetCharacter())) enemyInRange++;
                }
            }
            if (enemyInRange >= maxEnemiesInRange)
            {
                maxEnemiesInRange = enemyInRange;
                aoe = leftCone;
            }
        }

        return aoe;
    }

    public List<Tile> createUpperCone(List<Tile> tiles)
    {
        List<Tile> cone = new List<Tile>();
        foreach (Tile t in tiles)
        {
            if (t.mapPosition.y > activeUnit.onTile.mapPosition.y) cone.Add(t);
        }
            return cone;
    }

    public List<Tile> createLeftCone(List<Tile> tiles)
    {
        List<Tile> cone = new List<Tile>();
        foreach (Tile t in tiles)
        {
            if (t.mapPosition.x < activeUnit.onTile.mapPosition.x) cone.Add(t);
        }
            return cone;
    }

    public List<Tile> createLowerCone(List<Tile> tiles)
    {
        List<Tile> cone = new List<Tile>();
        foreach (Tile t in tiles)
        {
            if (t.mapPosition.y < activeUnit.onTile.mapPosition.y) cone.Add(t);
        }
            return cone;
    }

    public List<Tile> createRightCone(List<Tile> tiles)
    {
        List<Tile> cone = new List<Tile>();
        foreach (Tile t in tiles)
        {
            if (t.mapPosition.x > activeUnit.onTile.mapPosition.x) cone.Add(t);
        }
            return cone;
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
}