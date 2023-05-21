using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Player;

public class Character : MonoBehaviour
{
    //public static Character instance { get; private set; }
    private GameObject selectedGameObject;
    public Tile onTile { get; private set; }
    public int movement { get; private set; }
    public bool isMoving = false;
    [SerializeField] private float movementSpeed = 5f;
    private CharacterSO characterSheet;
    private Dictionary<AttributeType, AttributeScore> attributes = new Dictionary<AttributeType, AttributeScore>();
    [SerializeField] private Team team;
    public State state { get; private set; }

    public enum Team
    {
        Blue,
        Red
    }

    public enum State
    {
        Normal,
        Moving,
        Attacking
    }

    private void Awake()
    {
        
        //instance = this;
        movement = 3; //placeholder for testing, it will depend from character stats and equipment
        state = State.Normal;
        
    }

    private void Start()
    {
        Vector2Int playerStartPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        Dictionary<Vector2Int, Tile> map = MapHandler.instance.GetMap();
        SetTile(map[playerStartPosition]);
        GameHandler.instance.onChangeGameState.AddListener(OnChangeGameState);

    }

    

    private IEnumerator InterpToTile(Tile tile)
    {
        Vector3 destination = tile.transform.position;
        this.state=State.Moving;

        while (Vector3.Distance(destination, transform.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * movementSpeed);
            yield return null; 
        }
        this.state=State.Moving;
        SetTile(tile);
        StartCoroutine(UpdateMovementAnimation());
    }

    // Called to update movement animation after one frame from movement end
    private IEnumerator UpdateMovementAnimation()
    {
        yield return null;
       /* if (!isMoving)
        {
            isMoving = false;
        } Should not be necessary anymore*/
    }
   

    public void SetTile(Tile tile)
    {
        this.onTile = tile;
        transform.position = tile.transform.position;

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
        Debug.Log("Requestemend movement to tile " + tile.mapPosition);
        MapHandler.instance.HideAllTiles();
        List<Tile> toExamine = MapHandler.instance.GetTilesInRange(onTile, movement);
        List<Tile> path = FindPath(onTile, tile, toExamine);
        StartCoroutine(MoveAlongPath(path));
    }

    
    private IEnumerator MoveAlongPath(List<Tile> path)
    {
        if (path.Count < 1) Debug.LogWarning("Path has 0 elements");

        while (path.Count > 0)
        {
            yield return StartCoroutine(InterpToTile(path[0]));
            path.RemoveAt(0);
        }

        //MapHandler.instance.ShowNavigableTiles();
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
        return Mathf.Abs(start.mapPosition.x - tile.mapPosition.x) + Mathf.Abs(start.mapPosition.y - tile.mapPosition.y);
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

    private void OnChangeGameState(GameState state)
    {
        switch (state)
        {
            case GameState.FreeRoaming:
                movement = 3;
                break;
            case GameState.Battle:
                movement = 3;
                break;
            default:
                Debug.LogError("Game state changed to an invalid game state.");
                break;
        }
    }

    // Returns final attribute value (status effects applied)
    public int GetAttributeValue(AttributeType attribute)
    {
        if (!attributes.ContainsKey(attribute))
        {
            Debug.LogError(gameObject.name + " does not has the attribute " + attribute);
            return 0;
        }

        int result = (int)attributes[attribute];
        return Mathf.Max(result, 1);
    }

    // Returns final character movement (status effects applied)
    public int GetMovement()
    {
        if (characterSheet == null)
        {
            Debug.LogError(gameObject.name + " does not have a character sheet assigned");
            return 0;
        }

        int movement = (int)characterSheet.attributes[1].score; //attributes[1] takes Dex as the attribute deciding movement, this is merely for testing and non an actual in-game rule
        return movement;
    }

    public Team GetTeam()
    {
        return team;
    }

    public bool IsEnemy(Character c)
    {
        return c.GetTeam() != team;
    }

    public bool CanAttackUnit(Character c)
    {
        return Vector2Int.Distance(this.onTile.mapPosition, c.onTile.mapPosition) < 50f;
    }

    public void Attack(Character c) {
        Debug.Log("Attacco riuscito");
    }

 
}