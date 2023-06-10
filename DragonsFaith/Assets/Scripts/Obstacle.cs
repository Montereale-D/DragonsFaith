using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Tile onTile;
    public bool destroyable; 

    public void SetGridPosition()
    {
        var obstaclePosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        Dictionary<Vector2Int, Tile> map = MapHandler.instance.GetMap();

        var tile = map[obstaclePosition];
        SetTile(tile);
        tile.SetObstacleOnTile(this);
    }

    public void SetTile(Tile tile)
    {
        onTile = tile;
        transform.position = tile.transform.position;
    }

}
