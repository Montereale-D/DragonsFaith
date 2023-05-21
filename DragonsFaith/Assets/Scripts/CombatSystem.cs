using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{

    public static CombatSystem instance;

    [SerializeField] private Character[] characterArray;
    private State state;
    private Character unitGridCombat; //active unit
    private List<Character> blueTeamList;
    private List<Character> redTeamList;
    private int blueTeamActiveUnitIndex;
    private int redTeamActiveUnitIndex;
    private bool canMoveThisTurn;
    private bool canAttackThisTurn;

    private enum State
    {
        Normal,
        Waiting
    }

    private void Awake()
    {
        state = State.Normal;
        instance = this;
    }

    private void Start()
    {
        blueTeamList = new List<Character>();
        redTeamList = new List<Character>();
        blueTeamActiveUnitIndex = -1;
        redTeamActiveUnitIndex = -1;
        
        // Set all UnitGridCombat on their GridPosition
        foreach (Character c in this.characterArray)
        {
           if (c.onTile == null)
            {
                Debug.LogError("No c.onTile found");
                continue;
            } 

           
           c.onTile.SetCharacterOnTile(c);

            if (c.GetTeam() == Character.Team.Blue)
            {
                blueTeamList.Add(c);
            }
            else
            {
                redTeamList.Add(c);
            }
        }

        SelectNextActiveUnit();
        //UpdateValidMovePositions();  We used a different way to calculate this
    }

    private void SelectNextActiveUnit()
    {
        if (unitGridCombat == null || unitGridCombat.GetTeam() == Character.Team.Red)
        {
            unitGridCombat = GetNextActiveUnit(Character.Team.Blue);
        }
        else
        {
            unitGridCombat = GetNextActiveUnit(Character.Team.Red);
        }

        //GameHandler.instance.SetCameraFollowPosition(unitGridCombat.onTile);
        canMoveThisTurn = true;
        canAttackThisTurn = true;
    }

    private Character GetNextActiveUnit(Character.Team team)
    {
        if (team == Character.Team.Blue)
        {
            blueTeamActiveUnitIndex = (blueTeamActiveUnitIndex + 1) % blueTeamList.Count;
            if (blueTeamList[blueTeamActiveUnitIndex] == null ) //|| blueTeamList[blueTeamActiveUnitIndex].IsDead())
            {
                // Unit is Dead, get next one
                return GetNextActiveUnit(team);
            }
            else
            {
                return blueTeamList[blueTeamActiveUnitIndex];
            }
        }
        else
        {
            redTeamActiveUnitIndex = (redTeamActiveUnitIndex + 1) % redTeamList.Count;
            if (redTeamList[redTeamActiveUnitIndex] == null ) //|| redTeamList[redTeamActiveUnitIndex].IsDead())
            {
                // Unit is Dead, get next one
                return GetNextActiveUnit(team);
            }
            else
            {
                return redTeamList[redTeamActiveUnitIndex];
            }
        }
    }


    private void Update()
    {
        MapHandler.instance.HideAllTiles();
        
        if (GameHandler.instance.state == GameState.Battle) MapHandler.instance.ShowNavigableTiles();
        switch (state)
        {
            case State.Normal:
                RaycastHit2D? hit = MapHandler.instance.GetHoveredTile();
                if (hit.HasValue)
                {
                    Tile tile = hit.Value.collider.GetComponent<Tile>();
                    tile.ShowTile();
                    if (Input.GetMouseButtonDown(0))
                    {

                        // Check if clicking on a unit position
                        if (tile.charaterOnTile != null)
                        {
                            // Clicked on top of a Unit
                            if (unitGridCombat.IsEnemy(tile.charaterOnTile))
                            {
                                // Clicked on an Enemy of the current unit
                                if (unitGridCombat.CanAttackUnit(tile.charaterOnTile))
                                {
                                    // Can Attack Enemy
                                    if (canAttackThisTurn)
                                    {
                                        canAttackThisTurn = false;
                                        // Attack Enemy
                                        state = State.Waiting;
                                        unitGridCombat.Attack(tile.charaterOnTile);
                                        state = State.Normal;
                                    }
                                }
                                else
                                {
                                    // Cannot attack enemy
                                }
                                break;
                            }
                            else
                            {
                                // Not an enemy
                            }
                        }
                        else
                        {
                            // No unit here
                        }

                        //No fight try to move
                        if (MapHandler.instance.GetTilesInRange(unitGridCombat.onTile, unitGridCombat.movement).Contains(tile))
                        {
                            if (canMoveThisTurn)
                            {
                                canMoveThisTurn = false;

                                state = State.Waiting;

                                // Set entire Tilemap to Invisible
                                MapHandler.instance.HideAllTiles();


                                // Remove Unit from tile
                                unitGridCombat.onTile.ClearTile();
                                // Set Unit on target Grid Object
                                tile.SetCharacterOnTile(unitGridCombat);

                                unitGridCombat.MoveToTile(tile);

                                state = State.Normal;
                            }
                        }
                    }
                }

                TestTurnOver();

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ForceTurnOver();
                }
                break;
            case State.Waiting:
                break;
        }
    }

    private void TestTurnOver()
    {
        if (!canMoveThisTurn && !canAttackThisTurn)
        {
            // Cannot move or attack, turn over
            ForceTurnOver();
        }
    }

    private void ForceTurnOver()
    {
        SelectNextActiveUnit();
        //UpdateValidMovePositions(); We checked in a different way
    }

        public void SetUnitGridCombat(Character unitGridCombat)
        {
            this.unitGridCombat = unitGridCombat;
        }

        public void ClearUnitGridCombat()
        {
            SetUnitGridCombat(null);
        }

        public Character GetUnitGridCombat()
        {
            return unitGridCombat;
        }

}
