using System.Collections;
using System.Collections.Generic;
using Enemy;
using UnityEngine;

public class StartCombat : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!col.gameObject.CompareTag("Player")) return;
        
        Debug.Log("Player start combat!");
        GetComponentInParent<EnemyBehaviour>().OnCombatStart(transform.position);
    }
}
