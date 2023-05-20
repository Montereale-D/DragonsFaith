using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;



public class MapHandler : MonoBehaviour
{
    public static MapHandler instance { get; private set; }

    [SerializeReference] private Tile tileClass;

    private Dictionary<Vector2Int, Tile> map = new Dictionary<Vector2Int, Tile>();

    public Dictionary<Vector2Int, Tile> GetMap() {
        return map;
    }

    private GameObject container;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        container = new GameObject("OverlayContainer");
        Tilemap tileMap = gameObject.GetComponentInChildren<Tilemap>();
        BoundsInt bounds = tileMap.cellBounds;
        for (int x=bounds.min.x; x<bounds.max.x; x++)
        {
            for (int y=bounds.min.y; y<bounds.max.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                Vector2Int tilePosition2d = new Vector2Int(x, y);
                if (tileMap.HasTile(tilePosition) && !map.ContainsKey(tilePosition2d))
                {
                    Vector3 tileWorldPosition = tileMap.GetCellCenterWorld(tilePosition);
                    Tile tile = Instantiate(tileClass, container.transform);
                    tile.mapPosition = tilePosition2d;
                    tile.transform.position = tileWorldPosition;

                    map.Add(tilePosition2d, tile);

                }
            }
        }
    }



    public RaycastHit2D? GetHoveredTile()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("mainCamera not found: ASSIGN IT!");
            return null;
        }
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosition2d = new Vector2(mousePosition.x, mousePosition.y);
        RaycastHit2D[] hitResult = Physics2D.RaycastAll(mousePosition2d, Vector2.zero);
        if (hitResult.Length > 0)
        {
            return hitResult.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    //return a list of neighbour tile to one given as input (serching in a tile list if passed or in all map if not)
    public List<Tile> GetNeighbourTiles(Tile current, List<Tile> toExamine)
    {
        Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        if (toExamine.Count > 0)
        {
            foreach (Tile tile in toExamine)
            {
                tiles.Add(tile.mapPosition, tile);
            }
        }
        else tiles = map; // if toExamine is empty we check ALL tiles in map

        List<Tile> neighbours = new List<Tile>();


        // Check the tile over the current one
        Vector2Int positionToCheck = current.mapPosition + Vector2Int.up;
        if (tiles.ContainsKey(positionToCheck))
        {
            neighbours.Add(tiles[positionToCheck]);
        }

        // Check the tile under the current one
        positionToCheck = current.mapPosition + Vector2Int.down;
        if (tiles.ContainsKey(positionToCheck))
        {
            neighbours.Add(tiles[positionToCheck]);
        }

        // Check the tile at the left of the current one
        positionToCheck = current.mapPosition + Vector2Int.left;
        if (tiles.ContainsKey(positionToCheck))
        {
            neighbours.Add(tiles[positionToCheck]);
        }

        // Check the tile at the right of the current one
        positionToCheck = current.mapPosition + Vector2Int.right;
        if (tiles.ContainsKey(positionToCheck))
        {
            neighbours.Add(tiles[positionToCheck]);
        }

        return neighbours;

    }

    //return a list of all tiles in range from a starting tile
    public List<Tile> GetTilesInRange(Tile start, int range)
    {
        List<Tile> inRange = new List<Tile>();
        List<Tile> previousStep = new List<Tile>();
        int step = 0;
        inRange.Add(start);
        previousStep.Add(start);

        while (step < range)
        {
            List<Tile> neighbourhood = new List<Tile>();
            foreach (Tile tile in previousStep)
            {
                neighbourhood.AddRange(GetNeighbourTiles(tile, new List<Tile>()));
            }

            inRange.AddRange(neighbourhood);
            previousStep = inRange.Distinct().ToList();
            step++;
        }
        return inRange.Distinct().ToList();
    }

    public void HideAllTiles()
    {
        foreach(Tile tile in map.Values)
        {
            tile.HideTile();
        }
    }   
    
    //shows all tiles player can move to
    public void ShowNavigableTiles()
    {
        Character character = CombatSystem.instance.GetUnitGridCombat();
        List<Tile> tiles = GetTilesInRange(character.onTile, character.movement);
        foreach (Tile tile in tiles) tile.ShowTile();
    }

    private void onChangeGameState(GameState state)
    {
        switch (state)
        {
            case GameState.FreeRoaming:
                HideAllTiles();
                break;

            case GameState.Battle:
                ShowNavigableTiles();
                break;

            default:
                Debug.LogError("Game state changed to an invalid one");
                break;
        }
    }
}
