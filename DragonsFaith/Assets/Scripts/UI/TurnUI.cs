using System.Collections.Generic;
using System.Linq;
using Grid;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

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
        private PlayerGridMovement _dead;
        private bool _playerIsDead;
        private bool _isUpdating;

        public float animFadeInDuration = 1;
        public float animFadeOutDuration = 1;
        private static LTDescr delay;
        
        public void SetUpList(List<PlayerGridMovement> characterList)
        {
            //isCombatEnd = false;
            _charList = characterList;
            _localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerGridMovement>();
            Debug.Assert(CombatSystem.instance.otherPlayerSpriteIdx != null, 
                "CombatSystem.instance.otherPlayerSpriteIdx != null");
            _otherPlayerSprite = PlayerUI.instance.portraitSprites[(int)CombatSystem.instance.otherPlayerSpriteIdx];
            _otherPlayerName = CombatSystem.instance.otherPlayerName;

            foreach (var character in _charList.Where(character => 
                         character.GetTeam() == PlayerGridMovement.Team.Players))
            {
                character.SetTurnSprite(character == _localPlayer ? PlayerUI.instance.portrait.sprite : _otherPlayerSprite);
                character.charName =
                    character == _localPlayer ? character.GetComponent<CharacterInfo>().characterName : _otherPlayerName;
            }
            
            if (_charList.Count < threshold)
            {
                _multiplier = threshold / _charList.Count + 1;
                _numberOfCells = _charList.Count * _multiplier;
            }
            else _numberOfCells = _charList.Count;
        
            for (var i = 0; i < _numberOfCells; i++)
            {
                var newCell = Instantiate(cellPrefab, transform, true);
                var cell = newCell.GetComponent<TurnUICell>();
                cell.SetUnit(_charList[i % _charList.Count]);
                /*if (_charList[i % _charList.Count].GetTeam() == PlayerGridMovement.Team.Players && 
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
                }*/
                _cellList.Add(newCell);
            }
            
            newTurnUI.gameObject.SetActive(true);
            newTurnUI.GetComponentInChildren<TextMeshProUGUI>().text = 
                "Turn of " + _cellList[0].GetComponent<TurnUICell>().charName;
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

        //public bool isCombatEnd;
        private void Update()
        {
            //if(isCombatEnd) return;
            
            if (_cellList.Count < threshold && !_isUpdating)
            {
                FetchNewCells();
            }
        }

        private void FetchNewCells()
        {
            for (var i = 0; i < _charList.Count; i++)
            {
                var newCell = Instantiate(cellPrefab, transform, true);
                var cell = newCell.GetComponent<TurnUICell>();
                cell.SetUnit(_charList[i]);

                if (_playerIsDead && _charList[i] == _dead)
                {
                    cell.ownImage.color = Color.gray;
                }
                /*if (_charList[i % _charList.Count].GetTeam() == PlayerGridMovement.Team.Players && 
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
                }*/
                _cellList.Add(newCell);
            }
        }

        public void NextTurn()
        {
            Destroy(_cellList[0]);
            _cellList.RemoveAt(0);
            
            newTurnUI.gameObject.SetActive(true);
            newTurnUI.GetComponentInChildren<TextMeshProUGUI>().text =
                "Turn of " + _cellList[0].GetComponent<TurnUICell>().charName;
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

        public void OnDeath(PlayerGridMovement unit)
        {
            var deadUnit = _charList.Find(x => x == unit);
            if (deadUnit.GetTeam() == PlayerGridMovement.Team.Players)
            {
                _dead = deadUnit;
                _playerIsDead = true;
                foreach (var c in _cellList.Where(c => c.GetComponent<TurnUICell>().unit == _dead))
                {
                    c.GetComponent<TurnUICell>().ownImage.color = Color.gray;
                }
            }
            else
            {
                _isUpdating = true;
                var listLength = _cellList.Count; 
                for (var i = listLength-1; i > 0; i--)
                {
                    if (_cellList[i].GetComponent<TurnUICell>().unit != deadUnit) continue;
                    Destroy(_cellList[i]);
                    _cellList.RemoveAt(i);
                }

                _charList.Remove(deadUnit);
                _isUpdating = false;
            }
        }

        public void OnRevive(PlayerGridMovement unit)
        {
            foreach (var cell in _cellList.Where(t => t.GetComponent<TurnUICell>().unit == unit))
            {
                cell.GetComponent<TurnUICell>().ownImage.color = Color.white;
            }

            _playerIsDead = false;
        }
    }
}
