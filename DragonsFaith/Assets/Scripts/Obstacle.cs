using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UI;
using Unity.Netcode;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Tile onTile;
    public bool destroyable;
    public bool explosive;
    [OptionalField] public Animator animator;
    private static readonly int Explode = Animator.StringToHash("Explode");

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

    public void TriggerExplosion()
    {
        StartCoroutine(Explosion());
    }

    private IEnumerator Explosion()
    {
        animator.SetTrigger(Explode);
        AudioManager.instance.PlayBarrelExplosionSound();
        yield return new WaitForSeconds(1f);

        if (NetworkManager.Singleton.IsHost)
        {
            Destroy(gameObject);
        }
        
    }

    public void DestroyObj()
    {
        AudioManager.instance.PlayObstacleDestroyedSound();

        if (NetworkManager.Singleton.IsHost)
        {
            Destroy(gameObject);
        }
    }

}
