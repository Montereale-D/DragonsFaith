using System.Collections;
using System.Collections.Generic;
using Enemy;
using UnityEngine;

public class StartCombat : MonoBehaviour
{
    [SerializeField] private EnemyBehaviour enemyBehaviour;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!col.gameObject.CompareTag("Player")) return;
        
        Debug.Log("Player start combat!");
        enemyBehaviour.OnCombatStart(transform.position);
    }
}
