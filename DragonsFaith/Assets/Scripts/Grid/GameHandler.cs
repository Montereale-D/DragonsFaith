using System;
using System.Collections;
using System.Linq;
using Network;
using Player;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GameHandler : NetworkBehaviour
{
    public static GameHandler instance { get; private set; }
    public Camera mainCamera { get; private set; }
    //public GameState state { get; private set; }
    //public ChangeGameStateEvent onChangeGameState = new ChangeGameStateEvent();

    private PlayerGridMovement[] _characters;

    /*private void SetGameState(GameState inState)
    {
        state = inState;
        //onChangeGameState?.Invoke(state);
    }*/

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
        
        CharacterManager.Instance.SetPlayerGridMode();
    }

    //Find and setup all characters, then setup the CombatSystem
    public void Setup()
    {
        _characters = FindObjectsOfType<PlayerGridMovement>();
        for (var i = 0; i < _characters.Length; i++)
        {
            var character = _characters[i];
            //character.SetGridPosition();
            if (!character.SetMovement())
            {
                AskPlayerMovement(i);
            }

            //SetTileUnderCharacter(character);
        }
        
        StartCoroutine(WaitMovementInfo(_characters));
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


    private IEnumerator WaitMovementInfo(PlayerGridMovement[] characters)
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

    public void GameOver()
    {
        Debug.Log("GAME OVER");
        //todo UI show GameOver screen
        //FindObjectOfType<SceneManager>().LoadSceneAdditive("Menu");
    }

    public void CombatWin()
    {
        Debug.Log("COMBAT WIN");
        
        foreach (var popUpUI in FindObjectsOfType<CharacterGridPopUpUI>())
        {
            popUpUI.HideUI();
        }
        
        CharacterManager.Instance.SetPlayerFreeMode();
        PlayerUI.instance.HideCombatUI();
        //todo aggiornare con level manager
        SceneManager.instance.LoadSceneSingle("Dungeon_1");
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
}