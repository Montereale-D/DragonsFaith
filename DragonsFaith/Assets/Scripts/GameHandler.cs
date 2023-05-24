using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ChangeGameStateEvent : UnityEvent<GameState>
{
}

public enum GameState
{
    FreeRoaming,
    Battle
}

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance { get; private set; }
    public Camera mainCamera { get; private set; }
    public GameState state { get; private set; }
    public ChangeGameStateEvent onChangeGameState = new ChangeGameStateEvent();

    //called to change game state 
    public void SetGameState(GameState inState)
    {
        state = inState;
        onChangeGameState?.Invoke(state);
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        //Assign main camera
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("No main camera assigned");

        SetGameState(GameState.Battle);
        CharacterManager.Instance.SetPlayerGridMode();
    }

    public void Setup()
    {
        //if (!NetworkManager.Singleton.IsHost) return;

        var characters = FindObjectsOfType<PlayerGridMovement>();
        foreach (var character in characters)
        {
            character.SetGridPosition();
            SetTileUnderCharacter(character);
        }

        CombatSystem.instance.SetCharacters(characters);
    }

    private static void SetTileUnderCharacter(PlayerGridMovement playerGridMovement)
    {
        if (playerGridMovement.onTile == null)
        {
            Debug.LogError("No c.onTile found");
            return;
        }

        playerGridMovement.onTile.SetCharacterOnTile(playerGridMovement);
    }

    private void OnGameFinish()
    {
        CharacterManager.Instance.SetPlayerFreeMode();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) DebugChangeGameState();
    }

    //the following method is just for bugging and testing, not used in the actual game: it allows to switch from FreeRoaming to Battle Mode at will
    private void DebugChangeGameState()
    {
        if (state == GameState.FreeRoaming)
        {
            SetGameState(GameState.Battle);
            Debug.Log("Set game state to Battle Mode by debug controls");
        }
        else
        {
            SetGameState(GameState.FreeRoaming);
            Debug.Log("Set game state to Free Roaming by debug controls");
        }
    }
}