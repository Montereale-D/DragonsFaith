using Player;
using UnityEngine;

public class AbilityChecker : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!col.gameObject.CompareTag("Player")) return;

        Debug.Log("Ability check " + col.gameObject.GetComponentInChildren<CharacterManager>()
            .AbilityCheck(new Player.Attribute(AttributeType.Strength, AttributeScore.Superhuman)));
    }
}
