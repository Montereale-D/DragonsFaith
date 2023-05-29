﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Inventory.Items;
using Player;
using Unity.Netcode;

public class PlayerGridMovement : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private float pathMovementSpeed = 5f;
    public State state { get; private set; }
    public Tile onTile { get; private set; }
    public int movement { get; set; }

    public bool isMoving;

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
    }

    public void SetGridPosition()
    {
        var playerStartPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        Dictionary<Vector2Int, Tile> map = MapHandler.instance.GetMap();

        SetTile(map[playerStartPosition]);
        GameHandler.instance.onChangeGameState.AddListener(OnChangeGameState);
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
        StartCoroutine(UpdateMovementAnimation());
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
        if (isMoving) return;
        Debug.Log("Request movement to tile " + tile.mapPosition);
        MapHandler.instance.HideAllTiles();
        List<Tile> toExamine = MapHandler.instance.GetTilesInRange(onTile, movement);
        List<Tile> path = FindPath(onTile, tile, toExamine);
        StartCoroutine(MoveAlongPath(path));
    }


    private IEnumerator MoveAlongPath(List<Tile> path)
    {
        if (path.Count < 1) throw new Exception("Path has 0 elements");

        while (path.Count > 0)
        {
            yield return StartCoroutine(InterpToTile(path[0]));
            path.RemoveAt(0);
        }
    }

    #region Pathfinding

    // Called to get path from tile A to tile B
    private List<Tile> FindPath(Tile start, Tile end, List<Tile> searchableTiles)
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
                if (tile.ShouldBlockCharacter(this) || closedList.Contains(tile))
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
    private int GetManhattanDistance(Tile start, Tile tile)
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

    private void OnChangeGameState(GameState newState)
    {
        switch (newState)
        {
            case GameState.FreeRoaming:
                break;
            case GameState.Battle:
                break;
            default:
                Debug.LogError("Game state changed to an invalid game state.");
                break;
        }
    }

    public void SetTile(Tile tile)
    {
        onTile = tile;
        transform.position = tile.transform.position;
    }

    public Team GetTeam()
    {
        return team;
    }

    public bool IsEnemy(PlayerGridMovement target)
    {
        return target.GetTeam() != team;
    }

    // Called to update movement animation after one frame from movement end
    private IEnumerator UpdateMovementAnimation()
    {
        yield return null;
    }
}