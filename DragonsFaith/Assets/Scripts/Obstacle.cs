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
        SetGridPosition(obstaclePosition);
    }
    public void SetGridPosition(Vector2Int position)
    {
        Dictionary<Vector2Int, Tile> map = MapHandler.instance.GetMap();

        var tile = map[position];
        SetTile(tile);
        tile.SetObstacleOnTile(this);
        
        //if (!destroyable) GetComponent<SpriteRenderer>().color = Color.gray;
    }

    public void SetTile(Tile tile)
    {
        onTile = tile;
        transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, transform.position.z);
    }

}
