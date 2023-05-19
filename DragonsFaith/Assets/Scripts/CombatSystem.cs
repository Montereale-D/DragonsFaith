using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CharacterTeam
{
    Ally,
    Enemy
}

public class CharacterBattleData
{
    public Character character;
    public CharacterTeam team;
    public int initiative;

    public CharacterBattleData(Character c, CharacterTeam team)
    {
        this.character = c;
        this.team = team;
        initiative = c.GetAttributeValue(AttributeType.Wits);
    }


}


public class CombatSystem : MonoBehaviour
{
  public static CombatSystem instance { get; private set; }
    [Header("Battle Start Prompt")]
    [SerializeField] private float battleStartTime = .6f;
    [SerializeField] private float battleStartInterpTime = .2f;
    [SerializeField] private float battleStartFaderOpacity = .5f;
    [SerializeField] private string battleStartPromptMessage = "FIGHT!";
    [SerializeField] private Sprite battleStartPromptBackgroundSprite;
    private List<CharacterBattleData> characters = new List<CharacterBattleData>();
    private int turnIndex;

    //Called on battle start
    public void StartBattle()
    {
        Debug.Log("Started Battle!");
        GameHandler.instance.SetGameState(GameState.Battle);
        turnIndex = 0;

        Character[] charInFIght = GameObject.FindObjectsOfType<Character>();
        foreach (Character c in charInFIght)
        {
            characters.Add(new CharacterBattleData(c, CharacterTeam.Ally));
        }

        /*Battle start animation sequence
        Sequence sequence = DOTween.Sequence();
        sequence.Append(UIManager.Instance.GetFadeSequence(battleStartFaderOpacity, battleStartTime, battleStartInterpTime));
        sequence.Join(UIManager.Instance.GetPromptSequence(battleStartTime, battleStartInterpTime, battleStartPromptMessage, battleStartPromptBackgroundSprite));
        sequence.Play();*/

        StartNewRound();
        StartNewTurn();
    }

    //called to start a new turn
    public void StartNewTurn() {
        turnIndex++;

        if (turnIndex > characters.Count)
        {
            turnIndex = 0;
            Debug.Log("Starting new round");
        }
    }

    public void StartNewRound()
    { 
        characters.Sort((x, y) => y.initiative.CompareTo(x.initiative));
    }

    // Called when the script instance is being loaded
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
