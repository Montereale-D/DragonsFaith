using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Grid;
using Player;
using UI;
using Unity.Netcode;

public class PlayerGridMovement : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private float pathMovementSpeed = 5f;
    public int enemyLocalOrder;
    public State state { get; private set; }
    public Tile onTile { get; private set; }
    public int movement { get; set; }

    public bool isMoving;

    public Sprite turnSprite;

    [HideInInspector] public string charName;
    
    private Animator _animator;
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int X = Animator.StringToHash("x");
    private static readonly int Downed = Animator.StringToHash("downed");
    private static readonly int Revive = Animator.StringToHash("revive");
    private static readonly int Melee = Animator.StringToHash("Melee");
    private static readonly int Pistol = Animator.StringToHash("Pistol");
    private static readonly int Rifle = Animator.StringToHash("Rifle");
    private static readonly int Skill = Animator.StringToHash("Skill");

    public enum Team
    {
        Players,
        Enemies
    }

    public enum State
    {
        Normal,
        Moving,
        Attacking
    }

    private void Awake()
    {
        state = State.Normal;
        _animator = GetComponentInChildren<Animator>();
    }

    public void SetGridPosition()
    {
        var position = transform.position;
        var playerStartPosition = new Vector2Int((int)position.x, (int)position.y);
        SetGridPosition(playerStartPosition);
    }
    public void SetGridPosition(Vector2Int position)
    {
        var map = MapHandler.instance.GetMap();

        var tile = map[position];
        SetTile(tile);
        tile.SetCharacterOnTile(this);
    }

    public bool SetMovement()
    {
        if (team == Team.Players)
        {
            if (GetComponent<NetworkObject>().IsLocalPlayer)
            {
                movement = (int)CharacterManager.Instance.GetTotalAgi();
            }
            else
            {
                return false;
            }
        }
        else
        {
            movement = GetComponent<EnemyGridBehaviour>().agility;
        }
        
        Debug.Log(gameObject.name + " movement is " + movement);
        return true;
    }


    private IEnumerator InterpToTile(Tile tile)
    {
        Vector3 destination = tile.transform.position;
        state = State.Moving;

        while (Vector3.Distance(destination, transform.position) > 0.05f)
        {
            transform.position =
                Vector3.MoveTowards(transform.position, destination, Time.deltaTime * pathMovementSpeed);
            yield return null;
        }

        state = State.Moving;
        SetTile(tile);
        Debug.Log("PerformEnemyMovement set tile InterpToTile");
        //StartCoroutine(UpdateMovementAnimation());
    }

    public void FreeRoaming(Vector2Int direction)
    {
        if (isMoving) return;
        Dictionary<Vector2Int, Tile> map = MapHandler.instance.GetMap();
        Vector2Int tilePosition = onTile.mapPosition + direction;
        if (!map.ContainsKey(tilePosition)) return;
        Tile tile = map[tilePosition];
        if (tile.ShouldBlockCharacter(this)) return;

        StartCoroutine(InterpToTile(tile));
    }

    public void MoveToTile(Tile tile)
    {
        if (isMoving) return; //TODO: isMoving is never set to true
        Debug.Log("Request movement to tile " + tile.mapPosition);
        MapHandler.instance.HideAllTiles();
        List<Tile> toExamine = MapHandler.instance.GetTilesInRange(onTile, movement);
        Debug.Log("Path from " + onTile.mapPosition + " to " + tile.mapPosition);
        List<Tile> path = FindPath(onTile, tile, toExamine);
        StartCoroutine(MoveAlongPath(path));
    }


    private IEnumerator MoveAlongPath(List<Tile> path)
    {
        if (path.Count < 1)
        {
            Debug.LogError("Path has 0 elements");
            _animator.SetBool(IsMoving, false);
            Debug.Log("PerformEnemyMovement unlock");
            CombatSystem.instance._moveInProgress = false;
            yield break;
        }

        var direction = (path[^1].transform.position - transform.position).normalized.x;
        _animator.SetBool(IsMoving, true);
        _animator.SetFloat(X, direction);
        /*if (team == Team.Players) AudioManager.instance.PlayPlayerWalkSound();
        else AudioManager.instance.PlayEnemyWalkSound();*/
        
        while (path.Count > 0)
        {
            yield return StartCoroutine(InterpToTile(path[0]));
            path.RemoveAt(0);
        }
        
        _animator.SetBool(IsMoving, false);
        /*if (team == Team.Players) AudioManager.instance.StopPlayerWalkSound();
        else AudioManager.instance.PlayEnemyWalkSound();*/
        Debug.Log("PerformEnemyMovement unlock");
        CombatSystem.instance._moveInProgress = false;
    }

    #region Pathfinding

    // Called to get path from tile A to tile B
    public List<Tile> FindPath(Tile start, Tile end, List<Tile> searchableTiles)
    {
        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();

        openList.Add(start);
        while (openList.Count > 0)
        {
            Tile current = openList.OrderBy(x => x.f).First();
            openList.Remove(current);
            closedList.Add(current);

            if (current == end)
            {
                return GetFinishedList(start, end);
            }

            List<Tile> neighbourTiles = MapHandler.instance.GetNeighbourTiles(current, searchableTiles);
            foreach (Tile tile in neighbourTiles)
            {
                if (!tile.navigable || closedList.Contains(tile))
                    continue;

                tile.g = GetManhattanDistance(start, tile);
                tile.h = GetManhattanDistance(end, tile);
                tile.previous = current;

                if (!openList.Contains(tile))
                {
                    openList.Add(tile);
                }
            }
        }

        return new List<Tile>();
    }

    // Return manhattan distance between two tiles
    public static int GetManhattanDistance(Tile start, Tile tile)
    {
        return Mathf.Abs(start.mapPosition.x - tile.mapPosition.x) +
               Mathf.Abs(start.mapPosition.y - tile.mapPosition.y);
    }

    // Return the list of tiles to reach destination
    private List<Tile> GetFinishedList(Tile start, Tile end)
    {
        List<Tile> finishedList = new List<Tile>();
        Tile current = end;

        while (current != start)
        {
            finishedList.Add(current);
            current = current.previous;
        }

        finishedList.Reverse();
        return finishedList;
    }

    #endregion

    public void SetTile(Tile tile)
    {
        onTile = tile;
        transform.position = tile.transform.position;
    }

    public Team GetTeam()
    {
        return team;
    }

    public bool IsOppositeTeam(PlayerGridMovement target)
    {
        return target.GetTeam() != team;
    }

    // Called to update movement animation after one frame from movement end
    /*private IEnumerator UpdateMovementAnimation()
    {
        yield return null;
    }*/

    public void SetTurnSprite(Sprite sprite)
    {
        turnSprite = sprite;
    }

    public void TurnTowardTile(Tile tile)
    {
        var direction = (tile.transform.position - transform.position).normalized.x;
        _animator.SetFloat(X, direction);
    }

    public void OnDeath()
    {
        _animator.SetTrigger(Downed);
    }

    public void OnRevive()
    {
        _animator.SetTrigger(Revive);
    }

    public void TriggerAttackAnimation(string weaponName)
    {
        switch (weaponName)
        {
            case "Pistol":
                _animator.SetTrigger(Pistol);
                AudioManager.instance.PlayPlayerPistolAttackSound();
                break;
            case "Assault Rifle":
                _animator.SetTrigger(Rifle);
                AudioManager.instance.PlayPlayerAssaultAttackSound();
                break;
            case "Shotgun":
                _animator.SetTrigger(Rifle);
                AudioManager.instance.PlayShotgunAttackSound();
                break;
            case "Sniper Rifle":
                _animator.SetTrigger(Rifle);
                AudioManager.instance.PlaySniperAttackSound();
                break;
            case "Skill":
                _animator.SetTrigger(Skill);
                break;
            default:
                _animator.SetTrigger(Melee);
                AudioManager.instance.PlayPlayerKnifeAttackSound();
                break;
        }
    }
    
    public void TriggerReloadSound(string weaponName)
    {
        switch (weaponName)
        {
            case "Pistol":
                AudioManager.instance.PlayPlayerPistolReloadSound();
                break;
            case "Assault Rifle":
                AudioManager.instance.PlayPlayerAssaultReloadSound();
                break;
            case "Shotgun":
                AudioManager.instance.PlayShotgunReloadSound();
                break;
            case "Sniper Rifle":
                AudioManager.instance.PlaySniperReloadSound();
                break;
            default:
                break;
        }
    }
}