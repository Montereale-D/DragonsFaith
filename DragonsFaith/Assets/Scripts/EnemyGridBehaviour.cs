using System.Collections.Generic;
using Inventory.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyGridBehaviour : MonoBehaviour
{
    public enum EnemyBehaviourType
    {
        Melee, Ranged
    }

    private delegate void EnemyPlan(List<PlayerGridMovement> characterList);
    private EnemyPlan _enemyPlan;
    public EnemyBehaviourType behaviourType;
    public Weapon weapon;

    public int agility = 1;
    public int healthMax = 100;
    private int _health;

    private EnemyGridPopUpUI _popUpUI;
    private void Awake()
    {
        _enemyPlan = behaviourType == EnemyBehaviourType.Melee ? MeleePlan : RangedPlan;
        _health = healthMax;
        
        _popUpUI = GetComponent<EnemyGridPopUpUI>();
        _popUpUI.SetUI(healthMax);
    }

    public void PlanAction(List<PlayerGridMovement> characterList)
    {
        Debug.Log("PlanAction");
        //_enemyPlan.Invoke(characterList);
    }

    public void Damage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            _health = 0;
            Die();
        }
        
        _popUpUI.UpdateUI(_health);
    }

    private void Die()
    {
        //todo
    }

    #region MeleeBehaviour

    private void MeleePlan(List<PlayerGridMovement> characterList)
    {
        //todo
        Debug.Log("Melee Enemy turn");
        List<PlayerGridMovement> players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
        PlayerGridMovement target = null;
        //check if they are both alive
        int alive = 0;
        PlayerGridMovement whoAlive = null;
        Tile onTile = GetComponent<PlayerGridMovement>().onTile;
        foreach (PlayerGridMovement p in players)
        {
            //if(!p.isDead()) {alive ++; whoAlive=p;}  At the moment there is no HP system in PlayerGridMovement or a reference in the CharacterSO   
        }
        //if only one is alive attack him
        if (alive == 1) target = whoAlive;
        //if both alive check players distance and attacks the most distant
        else
        {
            float distance = 100;
            PlayerGridMovement nearest = null;
            foreach (PlayerGridMovement p in players)
            {
                float d = Vector2Int.Distance(onTile.mapPosition, p.onTile.mapPosition);
                if (distance > d)
                {
                    distance = d;
                    nearest = p;
                }
            }
            target = nearest;
        }
        //move in the range area (or as close as you can)
        Tile tileToReach = target.onTile;
        List<Tile> reachables = new List<Tile>();
        while (reachables.Count < 1)
        {
            reachables = MapHandler.instance.GetNeighbourTiles(tileToReach, MapHandler.instance.GetTilesInRange(onTile, agility));
            float d = 100.0f;
            Tile nextToReach = null;
            foreach (Tile t in MapHandler.instance.GetNeighbourTiles(tileToReach, new List<Tile>()))
            {
                if (Vector2Int.Distance(onTile.mapPosition, t.mapPosition) < d)
                {
                    d = Vector2Int.Distance(onTile.mapPosition, t.mapPosition);
                    nextToReach = t;
                }
            }
            tileToReach = nextToReach;
        }
        
        //GetComponent<PlayerGridMovement>().MoveToTile(tileToReach);
        CombatSystem.instance.CheckMovement(tileToReach);
        
        //if in range attack
        onTile = GetComponent<PlayerGridMovement>().onTile;  //not sure if we need this, or if is auto-updated post movement
        
        foreach (Tile t in MapHandler.instance.GetNeighbourTiles(onTile, new List<Tile>()) )
        {
            if (t.GetCharacter() == target)
            {
                //CombatSystem.instance.Attack(target, weapon);
                CombatSystem.instance.CheckAction(target);
                break;
            }

        }
    }

    #endregion

    #region RangedBehaviour

    private void RangedPlan(List<PlayerGridMovement> characterList)
    {
        //todo
        Debug.Log("Ranged Enemy turn");
        List<PlayerGridMovement> players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
        PlayerGridMovement target = null;
        //check if they are both alive
        int alive=0;
        PlayerGridMovement whoAlive=null;
        Tile onTile = GetComponent<PlayerGridMovement>().onTile;
        foreach (PlayerGridMovement p in players)
        {
            //if(!p.isDead()) {alive ++; whoAlive=p;}  At the moment there is no HP system in PlayerGridMovement or a reference in the CharacterSO   
        }
        //if only one is alive attack him
        if (alive == 1) target=whoAlive;
        //if both alive check players distance and attacks the most distant
        else
        {
            float distance = 0;
            PlayerGridMovement farthest = null;
            foreach (PlayerGridMovement p in players)
            {
                float d = Vector2Int.Distance(onTile.mapPosition, p.onTile.mapPosition);
                if (distance < d)
                {
                    distance = d;
                    farthest = p;
                }
            }
            target = farthest;
        }
        //move in the range area (or as close as you can)
        Tile tileToReach = target.onTile;
        List<Tile> reachables = new List<Tile>();
        while (reachables.Count < 1) {
            reachables = MapHandler.instance.GetNeighbourTiles(tileToReach, MapHandler.instance.GetTilesInRange(onTile, agility));
            float d = 100.0f;
            Tile nextToReach = null;
            foreach( Tile t in MapHandler.instance.GetNeighbourTiles(tileToReach, new List<Tile>()) ){
                if (Vector2Int.Distance(onTile.mapPosition, t.mapPosition) < d)
                {
                    d = Vector2Int.Distance(onTile.mapPosition, t.mapPosition);
                    nextToReach = t;
                }
            }
            tileToReach = nextToReach;
        }
        GetComponent<PlayerGridMovement>().MoveToTile(tileToReach);
        //if in range attack
        onTile = GetComponent<PlayerGridMovement>().onTile;  //not sure if we need this, or if is auto-updated post movement
        foreach (Tile t in MapHandler.instance.GetTilesInRange(onTile, (int)weapon.range))
        {
            if (t.GetCharacter() == target)
            {
                CombatSystem.instance.Attack(target, weapon);
                break;
            }
   
        }
    }
    
    #endregion
}