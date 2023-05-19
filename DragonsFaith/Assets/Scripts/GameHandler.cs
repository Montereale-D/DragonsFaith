using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChangeGameStateEvent : UnityEvent<GameState> { }

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

        //Assign main camera
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("No main camera assigned");
        DontDestroyOnLoad(gameObject);
        state = GameState.FreeRoaming;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) DebugChangeGameState();
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
