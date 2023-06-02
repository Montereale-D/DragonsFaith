using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TurnUI : MonoBehaviour
    {
        public int threshold = 7;
        private int _numberOfCells;
        private int _multiplier;

        public GameObject cellPrefab;
        public RectTransform newTurnUI;
        private List<PlayerGridMovement> _charList;
        private List<GameObject> _cellList = new List<GameObject>();

        private Sprite _otherPlayerSprite;
        private string _otherPlayerName;
        private PlayerGridMovement _localPlayer;
        private int _currentCellIdx;

        public float animFadeInDuration = 1;
        public float animFadeOutDuration = 1;
        private static LTDescr delay;
        
        public void SetUpList(List<PlayerGridMovement> characterList, Sprite otherPlayerSprite)
        {
            _charList = characterList;
            _localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerGridMovement>();
            _otherPlayerSprite = otherPlayerSprite;
            
            if (_charList.Count < threshold)
            {
                _multiplier = threshold / _charList.Count + 1;
                _numberOfCells = _charList.Count * _multiplier;
            }
            else _numberOfCells = _charList.Count;
        
            for (var i = 0; i < _numberOfCells; i++)
            {
                var newCell = Instantiate(cellPrefab, transform, true);
                var cellSprite = newCell.GetComponent<Image>();
                
                if (_charList[i % _charList.Count].GetTeam() == PlayerGridMovement.Team.Players && 
                    _charList[i % _charList.Count] == _localPlayer)
                {
                    cellSprite.sprite = PlayerUI.Instance.portrait.sprite;
                }
                else if (_charList[i % _charList.Count].GetTeam() == PlayerGridMovement.Team.Players && 
                         _charList[i % _charList.Count] != _localPlayer)
                {
                    cellSprite.sprite = _otherPlayerSprite;
                }
                else
                {
                    cellSprite.sprite = _charList[i % _charList.Count].turnSprite;
                }
                _cellList.Add(newCell);
            }
            
            newTurnUI.gameObject.SetActive(true);
            _currentCellIdx = 0;
            newTurnUI.GetComponentInChildren<TextMeshProUGUI>().text = "Turn of " + _charList[_currentCellIdx].name;
            FadeInElement(newTurnUI, animFadeInDuration);
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
                var cellSprite = newCell.GetComponent<Image>();
                
                if (_charList[i % _charList.Count].GetTeam() == PlayerGridMovement.Team.Players && 
                    _charList[i % _charList.Count] == _localPlayer)
                {
                    cellSprite.sprite = PlayerUI.Instance.portrait.sprite;
                }
                else if (_charList[i % _charList.Count].GetTeam() == PlayerGridMovement.Team.Players && 
                         _charList[i % _charList.Count] != _localPlayer)
                {
                    cellSprite.sprite = _otherPlayerSprite;
                }
                else
                {
                    cellSprite.sprite = _charList[i % _charList.Count].turnSprite;
                }
                _cellList.Add(newCell);
            }
        }

        public void NextTurn()
        {
            Destroy(_cellList[0]);
            _cellList.RemoveAt(0);
            
            newTurnUI.gameObject.SetActive(true);
            _currentCellIdx++;
            _currentCellIdx %= _charList.Count;
            newTurnUI.GetComponentInChildren<TextMeshProUGUI>().text = "Turn of " + _charList[_currentCellIdx].name;
            FadeInElement(newTurnUI, animFadeInDuration);
        }
        
        private void FadeInElement(RectTransform rectTransform, float fadeInDuration)
        {
            LeanTween.alpha(rectTransform, 1f, fadeInDuration).setEase(LeanTweenType.linear).setOnComplete(AnimComplete);
        }
        
        private void FadeOutElement(RectTransform rectTransform, float fadeOutDuration)
        {
            LeanTween.alpha(rectTransform, 0f, fadeOutDuration).setEase(LeanTweenType.linear).setOnComplete(DisableUI);
        }

        private void DisableUI()
        {
            newTurnUI.gameObject.SetActive(false);
        }

        private void AnimComplete()
        {
            FadeOutElement(newTurnUI, animFadeOutDuration);
        }
    }
}
