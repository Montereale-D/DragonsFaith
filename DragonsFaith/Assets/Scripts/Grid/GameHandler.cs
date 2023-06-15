using System;
using System.Collections;
using System.Linq;
using Grid;
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
    
    private PlayerGridMovement[] _characters;

    private Obstacle[] _obstacles;
    

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
        Debug.Log("GameHandle Setup");
        _obstacles = FindObjectsOfType<Obstacle>();
        /*for (var i = 0; i < _obstacles.Length; i++)
        {
            var obstacle = _obstacles[i];
            obstacle.SetGridPosition();
            SetTileUnderObstacle(obstacle);
        }*/

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
        
        StartCoroutine(WaitMovementInfo(_characters, _obstacles));
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


    private IEnumerator WaitMovementInfo(PlayerGridMovement[] characters, Obstacle[] obstacles)
    {
        Debug.Log("GameHandle WaitMovementInfo init");
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


        CombatSystem.instance.Setup(characters, obstacles);
    }

    public void GameOver()
    {
        Debug.Log("GAME OVER");
        //todo UI show GameOver screen
        SceneManager.instance.LoadSceneSingle("GameOver");
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
        
        
        SceneManager.instance.ReloadSceneSingleDungeon();
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

    /*private IEnumerator WaitCharacterSetupAndContinue(PlayerGridMovement[] characters)
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


        CombatSystem.instance.Setup(characters, _obstacles);
    }*/
    
    private static void SetTileUnderObstacle(Obstacle o)
    {
        if (o.onTile == null)
        {
            Debug.LogError("No c.onTile found");
            return;
        }

        o.onTile.SetObstacleOnTile(o);
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
}