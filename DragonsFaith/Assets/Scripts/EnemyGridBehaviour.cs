using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory.Items;
using UnityEngine;
using UnityEngine.Assertions;

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

    private void Awake()
    {
        _enemyPlan = behaviourType == EnemyBehaviourType.Melee ? MeleePlan : RangedPlan;
        _health = healthMax;

        _popUpUI = GetComponent<CharacterGridPopUpUI>();
        _popUpUI.SetUI(enemyName, healthMax);

        _characterInfo = GetComponent<CharacterInfo>();
        _characterInfo.characterName = enemyName;
    }

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

        _popUpUI.UpdateHealth(_health);
    }

    private void Die()
    {
        CombatSystem.instance.CharacterDied(GetComponent<PlayerGridMovement>());
        Destroy(gameObject);
    }

    [ContextMenu("ForceDieDebug")]
    public void ForceDieDebug()
    {
        CombatSystem.instance.NotifyAttackToEnemy(GetComponent<PlayerGridMovement>(), int.MaxValue);
    }

    #region MeleeBehaviour

    private void MeleePlan(List<PlayerGridMovement> characterList)
    {
        Debug.Log("Melee Enemy turn");
        var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
        PlayerGridMovement target;

        //todo check if they are both alive
        int alive = 0;
        PlayerGridMovement whoAlive = null;
        Tile onTile = GetComponent<PlayerGridMovement>().onTile;
        foreach (PlayerGridMovement p in players)
        {
            //if(!p.isDead()) {alive ++; whoAlive=p;}  At the moment there is no HP system in PlayerGridMovement or a reference in the CharacterSO   
        }

        //target the only alive player, otherwise attack the nearest one
        if (alive == 1)
        {
            target = whoAlive;
        }
        else
        {
            target = players.OrderBy(x => Vector2Int.Distance(onTile.mapPosition, x.onTile.mapPosition))
                .FirstOrDefault();
        }

        Assert.IsNotNull(target);
        Debug.Log("The target is " + target.name);

        //move in the range area (or as close as you can)
        Tile targetTile = target.onTile;
        Tile tileToReach;

        var agilityMaxMovement = MapHandler.instance.GetTilesInRange(onTile, agility);
        List<Tile> reachable =
            MapHandler.instance.GetNeighbourTiles(targetTile, agilityMaxMovement);

        reachable.RemoveAll(x => x.GetCharacter() != null);

        //todo aggiungere solo attacco se sono già a portata del target

        if (reachable.Count == 0)
        {
            Debug.Log("Target not reachable");
            tileToReach = MoveTowardTarget(targetTile, agilityMaxMovement);
            CombatSystem.instance.CheckMovement(tileToReach, true);
            CombatSystem.instance.SkipTurn();
        }
        else
        {
            Debug.Log("Target reachable");
            tileToReach = MoveNearTarget(targetTile, reachable);
            CombatSystem.instance.CheckMovement(tileToReach, true);
            CombatSystem.instance.Attack(target, weapon);
            CombatSystem.instance.SkipTurn();
        }
    }

    #endregion

    #region RangedBehaviour

    private void RangedPlan(List<PlayerGridMovement> characterList)
    {
        Debug.Log("Ranged Enemy turn");
        var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
        PlayerGridMovement target;

        //todo check if they are both alive
        int alive = 0;
        PlayerGridMovement whoAlive = null;
        Tile onTile = GetComponent<PlayerGridMovement>().onTile;
        foreach (PlayerGridMovement p in players)
        {
            //if(!p.isDead()) {alive ++; whoAlive=p;}  At the moment there is no HP system in PlayerGridMovement or a reference in the CharacterSO   
        }

        //target the only alive player, otherwise attack the farthest one
        if (alive == 1)
        {
            target = whoAlive;
        }
        else
        {
            target = players.OrderByDescending(x => Vector2Int.Distance(onTile.mapPosition, x.onTile.mapPosition))
                .FirstOrDefault();
        }

        Assert.IsNotNull(target);
        Debug.Log("The target is " + target.name);
        
        Tile targetTile = target.onTile;
        var targetDistance = Vector2Int.Distance(targetTile.mapPosition, onTile.mapPosition);

        if (targetDistance <= weapon.range)
        {
            Debug.Log("Weapon range, just attack");
            CombatSystem.instance.Attack(target, weapon);
            CombatSystem.instance.SkipTurn();
        }
        else if(targetDistance <= weapon.range + agility)
        {
            Debug.Log("Reach the weapon range then attack");
            var weaponRangeFromTarget = MapHandler.instance.GetTilesInRange(targetTile, weapon.range);
            weaponRangeFromTarget.RemoveAll(x => x.GetCharacter() != null);
            
            //var farthestTile = weaponRangeFromTarget.OrderByDescending(x => Vector2Int.Distance(x.mapPosition, targetTile.mapPosition)).FirstOrDefault();
            
            //todo bug: non si muove in una casella da cui puo' attaccare (fuori dal range dell'arma)
            /*CombatSystem.instance.CheckMovement(farthestTile, true);
            CombatSystem.instance.CheckAction(target);
            CombatSystem.instance.SkipTurn();*/
        }
        else
        {
            Debug.Log("Move towards not reachable target, then check if the other player is in weapon range");
            var tileToReach = MoveTowardTarget(targetTile, MapHandler.instance.GetTilesInRange(onTile, agility));
            CombatSystem.instance.CheckMovement(tileToReach, true);

            var otherPlayer = players[0] == target ? players[1] : players[0];
            if (Vector2Int.Distance(otherPlayer.onTile.mapPosition, onTile.mapPosition) <= weapon.range)
            {
                CombatSystem.instance.CheckAction(otherPlayer);
                CombatSystem.instance.SkipTurn();
            }
            else
            {
                CombatSystem.instance.SkipTurn();
            }
        }
    }

    #endregion

    private Tile MoveNearTarget(Tile targetTile, List<Tile> reachableTiles)
    {
        reachableTiles = reachableTiles.OrderBy(x => Vector2Int.Distance(targetTile.mapPosition, x.mapPosition))
            .ToList();
        return reachableTiles[0];
    }

    private Tile MoveTowardTarget(Tile targetTile, List<Tile> reachableTiles)
    {
        reachableTiles = reachableTiles.OrderBy(x => Vector2Int.Distance(targetTile.mapPosition, x.mapPosition))
            .ToList();
        return reachableTiles[0];
    }
}