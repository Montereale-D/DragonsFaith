using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color navigableColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color occupiedColor = new Color(1f, 0f, 0f);
    [SerializeField] private Color selectedColor = new Color(0f, 0f, 0f);
    [SerializeField] private Color hoveringColor = new Color(0.3f, 0.3f, 0.3f);
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
    }

    public void ShowTile()
    {
        Color c = _characterOnTile ? occupiedColor : navigableColor;
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
        //navigable = true;
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
}