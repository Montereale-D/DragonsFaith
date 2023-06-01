using System;
using System.Collections;
using System.Linq;
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

public class GameHandler : NetworkBehaviour
{
    public static GameHandler instance { get; private set; }
    public Camera mainCamera { get; private set; }
    public GameState state { get; private set; }
    public ChangeGameStateEvent onChangeGameState = new ChangeGameStateEvent();

    private PlayerGridMovement[] _characters;

    private void SetGameState(GameState inState)
    {
        state = inState;
        onChangeGameState?.Invoke(state);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
            return;
        }

        //Assign main camera
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("No main camera assigned");

        SetGameState(GameState.Battle);
        CharacterManager.Instance.SetPlayerGridMode();
    }

    //Find and setup all characters, then setup the CombatSystem
    public void Setup()
    {
        _characters = FindObjectsOfType<PlayerGridMovement>();
        for (var i = 0; i < _characters.Length; i++)
        {
            var character = _characters[i];
            character.SetGridPosition();
            if (!character.SetMovement())
            {
                AskPlayerMovement(i);
            }

            SetTileUnderCharacter(character);
        }

        StartCoroutine(WaitCharacterSetupAndContinue(_characters));
        //CombatSystem.instance.Setup(characters);
    }

    private void AskPlayerMovement(int askedIndex)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            AskMovementClientRpc(askedIndex);
        }
        else
        {
            AskMovementServerRpc(askedIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AskMovementServerRpc(int askedIndex)
    {
        if (!NetworkManager.Singleton.IsHost) return;

        var movement = (int)CharacterManager.Instance.GetTotalAgi();
        ReplyMovementClientRpc(askedIndex, movement);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReplyMovementServerRpc(int askedIndex, int movement)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        _characters[askedIndex].movement = movement;
        Debug.Log(_characters[askedIndex].gameObject.name + " movement is " + movement);
    }

    [ClientRpc]
    private void AskMovementClientRpc(int askedIndex)
    {
        if (NetworkManager.Singleton.IsHost) return;

        var movement = (int)CharacterManager.Instance.GetTotalAgi();
        ReplyMovementServerRpc(askedIndex, movement);
    }

    [ClientRpc]
    private void ReplyMovementClientRpc(int askedIndex, int movement)
    {
        if (NetworkManager.Singleton.IsHost) return;
        _characters[askedIndex].movement = movement;
        Debug.Log(_characters[askedIndex].gameObject.name + " movement is " + movement);
    }

    private IEnumerator WaitCharacterSetupAndContinue(PlayerGridMovement[] characters)
    {
        yield return null;

        var charactersReady = false;
        while (!charactersReady)
        {
            if (characters.Any(x => x.movement == 0))
            {
                yield return new WaitForSecondsRealtime(1f);
            }
            else
            {
                charactersReady = true;
            }
        }


        CombatSystem.instance.Setup(characters);
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