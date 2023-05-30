using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    //for testing
    //TODO retrieve list from Combat System and make it networked
    public int threshold = 7;
    private int _numberOfCells;
    private int _multiplier;

    public GameObject cellPrefab;
    private List<PlayerGridMovement> _charList;
    private List<GameObject> _cellList = new List<GameObject>();

    /*private void Start()
    {
        _charList = CombatSystem.instance.characterList;
    }*/

    public void SetUpList(List<PlayerGridMovement> characterList)
    {
        _charList = characterList;
        
        if (_charList.Count < threshold)
        {
            _multiplier = threshold / _charList.Count + 1;
            _numberOfCells = _charList.Count * _multiplier;
        }
        else _numberOfCells = _charList.Count;
        
        for (var i = 0; i < _numberOfCells; i++)
        {
            var newCell = Instantiate(cellPrefab, transform, true);
            newCell.GetComponent<Image>().sprite = _charList[i % _charList.Count].GetTeam() == PlayerGridMovement.Team.Players ? 
                PlayerUI.Instance.portrait.sprite : _charList[i % _charList.Count].turnSprite;
            _cellList.Add(newCell);
        }
    }

    public void DestroyList()
    {
        var listLength = _cellList.Count;
        for (var i = 0; i < listLength; i++)
        {
            Destroy(_cellList[0]);
            _cellList.RemoveAt(0);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            NextTurn();
        }

        if (_cellList.Count < threshold)
        {
            FetchNewCells();
        }
    }

    private void FetchNewCells()
    {
        for (var i = 0; i < _charList.Count; i++)
        {
            var newCell = Instantiate(cellPrefab, transform, true); 
            newCell.GetComponent<Image>().sprite = _charList[i % _charList.Count].GetTeam() == PlayerGridMovement.Team.Players ? 
                PlayerUI.Instance.portrait.sprite : _charList[i % _charList.Count].turnSprite;
            _cellList.Add(newCell);
        }
    }

    public void NextTurn()
    {
        Destroy(_cellList[0]);
        _cellList.RemoveAt(0);
    }
}
