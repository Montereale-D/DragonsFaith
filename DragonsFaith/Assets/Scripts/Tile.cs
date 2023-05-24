using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public int g;
    public int h;
    public int f { get { return g + h; } }
    public Tile previous;
    public PlayerGridMovement charaterOnTile;
    public Vector2Int mapPosition;
    public bool navigable;
    

    [SerializeField] private Color navigableColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color occupiedColor = new Color(1f, 0f, 0f);
    [SerializeField, Range(0f, 1f)] private float alpha = 0.5f;
    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void ShowTile() {
        Color c = charaterOnTile == null ? navigableColor : occupiedColor;
        c.a = alpha;
        gameObject.GetComponent<SpriteRenderer>().color = c;
    }


    public void HideTile() {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0f);
    }

    public bool ShouldBlockCharacter(PlayerGridMovement c)
    {
        return navigable || (charaterOnTile != null && charaterOnTile != c);
    }

    public void SetCharacterOnTile(PlayerGridMovement c)
    {
        charaterOnTile = c;
    }

    public void ClearTile()
    {
        SetCharacterOnTile(null);
    }
    public PlayerGridMovement GetCharacterOnTile()
    {
        return charaterOnTile;
    }

}