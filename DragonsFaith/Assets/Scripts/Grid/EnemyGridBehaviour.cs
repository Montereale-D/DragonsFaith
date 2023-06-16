using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory.Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Grid
{
    public class EnemyGridBehaviour : MonoBehaviour
    {
        public enum EnemyBehaviourType
        {
            Melee,
            Ranged
        }

        public string enemyName = "EnemyName";
        [SerializeField] private EnemyBehaviourType behaviourType;
        public int agility = 1;
        public int healthMax = 100;
        public Weapon weapon;
        public bool skipTurn;

        private delegate void EnemyPlan(List<PlayerGridMovement> characterList);

        private EnemyPlan _enemyPlan;
        private CharacterInfo _characterInfo;
        private CharacterGridPopUpUI _popUpUI;

        private int _health;
        private Animator _animator;
        //private OwnerNetworkAnimator _networkAnimator;
        private static readonly int Dead = Animator.StringToHash("Dead");
        private static readonly int Hurt = Animator.StringToHash("Hurt");
        private static readonly int X = Animator.StringToHash("x");
        private static readonly int Ranged = Animator.StringToHash("Ranged");
        private static readonly int Melee = Animator.StringToHash("Melee");
        private static readonly int Reload = Animator.StringToHash("Reload");

        private void Awake()
        {
            _enemyPlan = behaviourType == EnemyBehaviourType.Melee ? MeleePlan : RangedPlan;
            _health = healthMax;

            _popUpUI = GetComponent<CharacterGridPopUpUI>();
            _popUpUI.SetUI(enemyName, healthMax);

            _characterInfo = GetComponent<CharacterInfo>();
            _characterInfo.characterName = enemyName;

            _animator = GetComponentInChildren<Animator>();
            //_networkAnimator = GetComponentInChildren<OwnerNetworkAnimator>();
        }

        /*public void SetUI()
    {
        _popUpUI.SetUI(enemyName, healthMax);
    }*/

        public void PlanAction(List<PlayerGridMovement> characterList)
        {
            StartCoroutine(WaitAndPlan(characterList));
        }

        private IEnumerator WaitAndPlan(List<PlayerGridMovement> characterList)
        {
            yield return new WaitForSecondsRealtime(2f);
            if (skipTurn)
            {
                CombatSystem.instance.SkipTurn();
            }
            else
            {
                _enemyPlan.Invoke(characterList);
            }
        }

        public void Damage(int damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                _health = 0;
                Die();
            }

            _animator.SetTrigger(Hurt);
            _popUpUI.UpdateHealth(_health);
        }

        private void Die()
        {
            StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            CombatSystem.instance.CharacterDied(GetComponent<PlayerGridMovement>());
            _animator.SetTrigger(Dead);
            yield return new WaitForSeconds(3f);

            if (NetworkManager.Singleton.IsHost)
            {
                Destroy(gameObject);
            }
        }

        [ContextMenu("ForceDieDebug")]
        public void ForceDieDebug()
        {
            CombatSystem.instance.NotifyAttackToEnemy(GetComponent<PlayerGridMovement>(), int.MaxValue);
        }

        private IEnumerator AttackThenMove(Tile moveOnTile, PlayerGridMovement target, Tile attackFromTile)
        {
            Debug.Log("Attack and move");

            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.CheckAction(target, attackFromTile);
        
            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.PerformEnemyMovement(moveOnTile);
        
            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.SkipTurn();
        }
        private static IEnumerator MoveThenAttack(Tile onTile, PlayerGridMovement target)
        {
            Debug.Log("Move and attack");

            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.PerformEnemyMovement(onTile);
        
            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.CheckAction(target, onTile);
        
            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.SkipTurn();
        }
        private static IEnumerator Move(Tile onTile)
        {
            Debug.Log("Move");

            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.PerformEnemyMovement(onTile);
        
            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.SkipTurn();
        }
        private static IEnumerator Attack(PlayerGridMovement target, Tile fromTile)
        {
            Debug.Log("Attack");

            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.CheckAction(target, fromTile);
        
            yield return new WaitUntil(CombatSystem.instance.CanProceedToAction);
        
            CombatSystem.instance.SkipTurn();
        }
    
    

        #region MeleeBehaviour

        private void MeleePlan(List<PlayerGridMovement> characterList)
        {
            Debug.Log("Melee Enemy turn");
            var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);

            int alive = 0;
            PlayerGridMovement whoAlive = null;
            foreach (var p in players.Where(p => p.GetComponent<CharacterInfo>().IsAlive()))
            {
                alive++;
                whoAlive = p;
            }

            Tile onTile = GetComponent<PlayerGridMovement>().onTile;
            PlayerGridMovement target;
        
            //target the only alive player, otherwise attack the nearest one
            if (alive == 1)
            {
                target = whoAlive;
            }
            else
            {
                players = players.OrderBy(x => PlayerGridMovement.GetManhattanDistance(onTile, x.onTile)).ToList();
            
            
                Debug.Log(gameObject.name + " " + onTile.mapPosition + " at distance " + PlayerGridMovement.GetManhattanDistance(onTile, onTile));
                Debug.Log(players[0].name + " " + players[0].onTile.mapPosition + " at distance " + PlayerGridMovement.GetManhattanDistance(onTile, players[0].onTile));
                Debug.Log(players[1].name + " " + players[1].onTile.mapPosition + " at distance " + PlayerGridMovement.GetManhattanDistance(onTile, players[1].onTile));
            
                var nearPlayer = players[0];
                var nearPlayerDistance = PlayerGridMovement.GetManhattanDistance(onTile, nearPlayer.onTile);

                if (nearPlayerDistance <= weapon.range)
                {
                    Debug.Log("Weapon range, just attack");
                    //CombatSystem.instance.CheckAction(nearPlayer, onTile);
                    //CombatSystem.instance.SkipTurn();
                    StartCoroutine(Attack(nearPlayer, onTile));
                    return;
                }
                else
                {
                    target = players.OrderBy(x => PlayerGridMovement.GetManhattanDistance(onTile, x.onTile))
                        .FirstOrDefault();
                }
            }

            Assert.IsNotNull(target);
            Debug.Log("The target is " + target.name);

            //move in the range area (or as close as you can)
            Tile targetTile = target.onTile;
            Tile tileToReach;

            var agilityMaxMovement = MapHandler.instance.GetTilesInRange(onTile, agility);
            List<Tile> reachable =
                MapHandler.instance.GetNeighbourTiles(targetTile, agilityMaxMovement);

            reachable.RemoveAll(x => x.IsOccupied());


            if (reachable.Count == 0)
            {
                Debug.Log("Target not reachable");
                tileToReach = MoveTowardTarget(targetTile, agilityMaxMovement);
                //CombatSystem.instance.PerformEnemyMovement(tileToReach);
                //CombatSystem.instance.SkipTurn();
                if (tileToReach)
                {
                    StartCoroutine(Move(tileToReach));
                }
                else
                {
                    CombatSystem.instance.SkipTurn();
                }
            }
            else
            {
                Debug.Log("Target reachable");
                tileToReach = MoveNearTarget(targetTile, reachable);
                //CombatSystem.instance.PerformEnemyMovement(tileToReach);
                //CombatSystem.instance.CheckAction(target, tileToReach);
                //CombatSystem.instance.SkipTurn();
                if (tileToReach)
                {
                    StartCoroutine(MoveThenAttack(tileToReach, target));
                }
                else
                {
                    CombatSystem.instance.SkipTurn();
                }
            }
        }

        #endregion

        #region RangedBehaviour

        private void RangedPlan(List<PlayerGridMovement> characterList)
        {
            Debug.Log("Ranged Enemy turn");

            if (weapon.GetAmmo() <= 0)
            {
                Debug.Log("No ammo, reloading ...");
                CombatSystem.instance.ReloadAction();
                CombatSystem.instance.SkipTurn();
                return;
            }
        
        
            var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
            PlayerGridMovement target;

            int alive = 0;
            PlayerGridMovement whoAlive = null;
            Tile onTile = GetComponent<PlayerGridMovement>().onTile;
            foreach (var p in players.Where(p => p.GetComponent<CharacterInfo>().IsAlive()))
            {
                alive++;
                whoAlive = p;
            }

            //target the only alive player, otherwise attack the farthest one
            if (alive == 1)
            {
                target = whoAlive;
            }
            else
            {
                players = players.OrderBy(x => PlayerGridMovement.GetManhattanDistance(onTile, x.onTile)).ToList();
                var nearPlayer = players[0];
                var nearPlayerDistance = PlayerGridMovement.GetManhattanDistance(onTile, nearPlayer.onTile);

                if (nearPlayerDistance <= weapon.range)
                {
                    Debug.Log("Weapon range, just attack");
                    //CombatSystem.instance.CheckAction(nearPlayer, nearPlayer.onTile);
                    //CombatSystem.instance.SkipTurn();
                    StartCoroutine(Attack(nearPlayer, nearPlayer.onTile));
                    return;
                }
                else
                {
                    target = players.OrderByDescending(x => PlayerGridMovement.GetManhattanDistance(onTile, x.onTile))
                        .FirstOrDefault();
                }
            }

            Assert.IsNotNull(target);
            Debug.Log("The target is " + target.name);
            Debug.Log("The distance is " + PlayerGridMovement.GetManhattanDistance(onTile, target.onTile));


            Tile targetTile = target.onTile;
            var targetDistance = PlayerGridMovement.GetManhattanDistance(onTile, target.onTile);

            if (targetDistance <= weapon.range + agility)
            {
                Debug.Log("Reach the weapon range then attack");

                var weaponRangeFromTarget = MapHandler.instance.GetTilesInRange(targetTile, weapon.range);
                weaponRangeFromTarget.RemoveAll(x => x.IsOccupied());

                Debug.Log("Weapon range");
                foreach (var tile in weaponRangeFromTarget)
                {
                    Debug.Log(tile);
                }

                List<Tile> movementRange = MapHandler.instance.GetTilesInRange(onTile, agility);
                movementRange.RemoveAll(x => x.IsOccupied());
                
                Debug.Log("Movement range");
                foreach (var tile in movementRange)
                {
                    Debug.Log(tile);
                }

                var commonTiles = weaponRangeFromTarget.Where(t => movementRange.Contains(t)).ToList();
                
                Debug.Log("Common tiles");
                foreach (var tile in commonTiles)
                {
                    Debug.Log(tile);
                }
                
                var moveTile = commonTiles
                    .OrderByDescending(x => PlayerGridMovement.GetManhattanDistance(x, targetTile)).FirstOrDefault();
                if (moveTile)
                {
                    //CombatSystem.instance.PerformEnemyMovement(moveTile);
                    //CombatSystem.instance.CheckAction(target, moveTile);
                    //CombatSystem.instance.SkipTurn();
                    StartCoroutine(MoveThenAttack(moveTile, target));
                    return;
                }
            }
            else
            {
                Debug.Log("Move towards not reachable target, then check if the other player is in weapon range");
                var tileToReach = MoveTowardTarget(targetTile, MapHandler.instance.GetTilesInRange(onTile, agility));
                //CombatSystem.instance.PerformEnemyMovement(tileToReach);
                if (!tileToReach)
                {
                    CombatSystem.instance.SkipTurn();
                    return;
                }

                var otherPlayer = players[0] == target ? players[1] : players[0];
                if (PlayerGridMovement.GetManhattanDistance(onTile, otherPlayer.onTile) <= weapon.range)
                {
                    //CombatSystem.instance.CheckAction(otherPlayer);
                    //CombatSystem.instance.SkipTurn();
                    StartCoroutine(MoveThenAttack(tileToReach, otherPlayer));
                }
                else
                {
                    //CombatSystem.instance.SkipTurn();
                    StartCoroutine(Move(tileToReach));
                }
            }
        }

        #endregion

        private Tile MoveNearTarget(Tile targetTile, List<Tile> reachableTiles)
        {
            reachableTiles.RemoveAll(x => x.IsOccupied());
            
            reachableTiles = reachableTiles.OrderBy(x => PlayerGridMovement.GetManhattanDistance(x, targetTile))
                .ToList();
        
            Debug.Log("Test: target" + targetTile.mapPosition);

            return reachableTiles[0];
        }

        private Tile MoveTowardTarget(Tile targetTile, List<Tile> reachableTiles)
        {
            reachableTiles.RemoveAll(x => x.IsOccupied());
            
            reachableTiles = reachableTiles.OrderBy(x => PlayerGridMovement.GetManhattanDistance(x, targetTile))
                .ToList();

            return reachableTiles[0];
        }

        #region Animation
    
        public void TriggerAttackAnimation(PlayerGridMovement target)
        {
            var direction = (target.transform.position - transform.position).normalized.x;
            _animator.SetFloat(X, direction);
            _animator.SetTrigger(behaviourType == EnemyBehaviourType.Ranged ? Ranged : Melee);
        }

        public void TriggerReloadAnimation()
        {
            _animator.SetTrigger(Reload);
        }
    
        #endregion
    }
}