using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;


public class MapHandler : MonoBehaviour
{
    public static MapHandler instance { get; private set; }

    [SerializeReference] private Tile tileClass;
    private LayerMask _layerMask;

    private Dictionary<Vector2Int, Tile> map = new Dictionary<Vector2Int, Tile>();

    public Dictionary<Vector2Int, Tile> GetMap()
    {
        return map;
    }

    private GameObject container;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        _layerMask = LayerMask.GetMask("Tiles");
        container = new GameObject("OverlayContainer");
        //var tileMap = gameObject.GetComponentInChildren<Tilemap>();
        var tileMap = gameObject.GetComponentsInChildren<Tilemap>()[0];
        var bounds = tileMap.cellBounds;
        for (var x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (var y = bounds.min.y; y < bounds.max.y; y++)
            {
                var tilePosition = new Vector3Int(x, y, 0);
                var tilePosition2d = new Vector2Int(x, y);
                if (tileMap.HasTile(tilePosition) && !map.ContainsKey(tilePosition2d))
                {
                    var tileWorldPosition = tileMap.GetCellCenterWorld(tilePosition);
                    var tile = Instantiate(tileClass, container.transform);
                    tile.mapPosition = tilePosition2d;
                    tile.transform.position = tileWorldPosition;

                    map.Add(tilePosition2d, tile);
                }
            }
        }
    }

    public RaycastHit2D? GetHoveredRaycast()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("mainCamera not found: ASSIGN IT!");
            return null;
        }

        var mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var mousePosition2d = new Vector2(mousePosition.x, mousePosition.y);
        var hitResult = Physics2D.RaycastAll(mousePosition2d, Vector2.zero, Mathf.Infinity, _layerMask);
        if (hitResult.Length > 0)
        {
            return hitResult.OrderByDescending(i => i.collider.transform.position.z).First();
        }

        return null;
    }

    //return a list of neighbour tile to one given as input (serching in a tile list if passed or in all map if not)
    public List<Tile> GetNeighbourTiles(Tile current, List<Tile> toExamine)
    {
        var tiles = new Dictionary<Vector2Int, Tile>();
        if (toExamine.Count > 0)
        {
            foreach (var tile in toExamine)
            {
                tiles.Add(tile.mapPosition, tile);
            }
        }
        else tiles = map; // if toExamine is empty we check ALL tiles in map

        var neighbours = new List<Tile>();


        // Check the tile over the current one
        var positionToCheck = current.mapPosition + Vector2Int.up;
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

    public List<Tile> GetWalkableNeighbourTiles(Tile current, List<Tile> toExamine)
    {
        var tiles = new Dictionary<Vector2Int, Tile>();
        if (toExamine.Count > 0)
        {
            foreach (var tile in toExamine)
            {
                tiles.Add(tile.mapPosition, tile);
            }
        }
        else tiles = map; // if toExamine is empty we check ALL tiles in map

        var neighbours = new List<Tile>();


        // Check the tile over the current one
        var positionToCheck = current.mapPosition + Vector2Int.up;
        if (tiles.ContainsKey(positionToCheck))
        {
            if (!tiles[positionToCheck].IsOccupied()) neighbours.Add(tiles[positionToCheck]);
        }

        // Check the tile under the current one
        positionToCheck = current.mapPosition + Vector2Int.down;
        if (tiles.ContainsKey(positionToCheck))
        {
            if (!tiles[positionToCheck].IsOccupied()) neighbours.Add(tiles[positionToCheck]);
        }

        // Check the tile at the left of the current one
        positionToCheck = current.mapPosition + Vector2Int.left;
        if (tiles.ContainsKey(positionToCheck))
        {
            if (!tiles[positionToCheck].IsOccupied()) neighbours.Add(tiles[positionToCheck]);
        }

        // Check the tile at the right of the current one
        positionToCheck = current.mapPosition + Vector2Int.right;
        if (tiles.ContainsKey(positionToCheck))
        {
            if (!tiles[positionToCheck].IsOccupied()) neighbours.Add(tiles[positionToCheck]);
        }

        return neighbours;
    }

    //return a list of all tiles in range from a starting tile
    public List<Tile> GetTilesInRange(Tile start, int range)
    {
        var inRange = new List<Tile>();
        var previousStep = new List<Tile>();
        var step = 0;
        inRange.Add(start);
        previousStep.Add(start);

        while (step < range)
        {
            var neighbourhood = new List<Tile>();
            foreach (var tile in previousStep)
            {
                neighbourhood.AddRange(GetNeighbourTiles(tile, new List<Tile>()));
            }

            inRange.AddRange(neighbourhood);
            previousStep = inRange.Distinct().ToList();
            step++;
        }

        return inRange.Distinct().ToList();
    }

    //return a list of all tiles in range from a starting tile
    public List<Tile> GetTilesInMovementRange(Tile start, int range)
    {
        var inRange = new List<Tile>();
        var previousStep = new List<Tile>();
        var step = 0;
        inRange.Add(start);
        previousStep.Add(start);

        while (step < range)
        {
            var neighbourhood = new List<Tile>();
            foreach (var tile in previousStep)
            {
                neighbourhood.AddRange(GetWalkableNeighbourTiles(tile, new List<Tile>()));
            }

            inRange.AddRange(neighbourhood);
            previousStep = inRange.Distinct().ToList();
            step++;
        }

        return inRange.Distinct().ToList();
    }

    public void HideAllTiles()
    {
        foreach (var tile in map.Values)
        {
            tile.HideTile();
        }
    }

    //shows all tiles player can move to
    public void ShowNavigableTiles(Tile characterTile, int movement)
    {
        var tiles = GetTilesInMovementRange(characterTile, movement);
        foreach (var tile in tiles)
        {
            if(tile.navigable)
                tile.ShowTile();
        }
    }
}