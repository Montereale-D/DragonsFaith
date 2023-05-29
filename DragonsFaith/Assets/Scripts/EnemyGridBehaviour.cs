using System.Collections.Generic;
using Inventory.Items;
using UnityEngine;

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
    private int _health = 100;

    private void Awake()
    {
        _enemyPlan = behaviourType == EnemyBehaviourType.Melee ? MeleePlan : RangedPlan;
    }

    public void PlanAction(List<PlayerGridMovement> characterList)
    {
        _enemyPlan.Invoke(characterList);
    }

    public void Damage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            _health = 0;
            Die();
        }
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
        var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
        //check if the are both alive
        //check if are both melee attacked
        //if both check players distance
        
        //move near the target (or as close as you can)
        //if near attack
    }

    #endregion
    
    #region RangedBehaviour

    private void RangedPlan(List<PlayerGridMovement> characterList)
    {
        //todo
        Debug.Log("Ranged Enemy turn");
        var players = characterList.FindAll(x => x.GetTeam() == PlayerGridMovement.Team.Players);
        //check if the are both alive
        //if both check players distance
        
        //move in the range area (or as close as you can)
        //if in range attack
    }
    
    #endregion
}