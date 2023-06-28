using Grid;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TurnUICell : MonoBehaviour
    {
        [HideInInspector] public PlayerGridMovement unit;
        public string charName { get; private set; }
        [HideInInspector] public Image ownImage;

        private void Awake()
        {
            ownImage = GetComponent<Image>();
        }

        public void SetUnit(PlayerGridMovement character)
        {
            unit = character;
            ownImage.sprite = character.turnSprite;
            //charName = character.name;
            charName = character.GetTeam() == PlayerGridMovement.Team.Enemies ? 
                character.GetComponent<EnemyGridBehaviour>().enemyName : 
                character.charName;
            /*charName = character.charName;*/
        }
    }
}
