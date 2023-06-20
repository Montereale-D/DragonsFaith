﻿using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color navigableColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color occupiedColor = new Color(1f, 0f, 0f);
    [SerializeField] private Color selectedColor = new Color(0f, 0f, 0f);
    [SerializeField] private Color hoveringColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color damageAreaColor = new Color(0.8f, 0.3f, 0.0f);
    [SerializeField, Range(0f, 1f)] private float alpha = 0.5f;

    public Tile previous;
    private PlayerGridMovement _characterOnTile;
    public Vector2Int mapPosition;
    private Obstacle _obstacleOnTile;

    //comment or rename => g,h,f
    public int g;
    public int h;
    public int f => g + h;
    
    public bool navigable;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        navigable = true;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.layer = LayerMask.NameToLayer("Tiles");
        
    }

    public void ShowTile()
    {
        Color c = _characterOnTile ? occupiedColor : navigableColor;
        c.a = alpha;
        _spriteRenderer.color = c;
    }
    public void ShowDamageArea()
    {
        Color c = damageAreaColor;
        c.a = alpha;
        _spriteRenderer.color = c;
    }

    public void SkillTile()
    {
        Color c = new Color(0f, 0f, 1f);
        c.a = alpha;
        _spriteRenderer.color = c;

    }


    public void HoverTile()
    {
        Color c = _characterOnTile ? occupiedColor : hoveringColor;
        c.a = alpha;
        _spriteRenderer.color = c;
    }
    public void SelectTile()
    {
        Color c = selectedColor;
        c.a = alpha;
        _spriteRenderer.color = c;
    }

    
    public void HideTile()
    {
        _spriteRenderer.color = new Color(0f, 0f, 0f, 0f);
    }

    public bool ShouldBlockCharacter(PlayerGridMovement c)
    {
        return navigable || (_characterOnTile && _characterOnTile != c);
    }

    public void SetCharacterOnTile(PlayerGridMovement c)
    {
        _characterOnTile = c;
        //navigable = false;
    }

    public void ClearTile()
    {
        SetCharacterOnTile(null);
        SetObstacleOnTile(null);
        navigable = true;
    }

    public PlayerGridMovement GetCharacter()
    {
        return _characterOnTile;
    }

    public void SetObstacleOnTile(Obstacle o)
    {
        _obstacleOnTile = o;
        navigable = false;
    }

    public Obstacle GetObstacle()
    {
        return _obstacleOnTile;
    }

    public bool IsOccupied()
    {
        return GetCharacter() != null || GetObstacle() != null;
    }

    public override string ToString()
    {
        return mapPosition + " is occupied: " + IsOccupied();
    }
}