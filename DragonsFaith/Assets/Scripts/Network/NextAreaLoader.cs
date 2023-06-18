using System;
using System.Collections;
using UI;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Network
{
    [RequireComponent(typeof(Collider2D))]
    public class NextAreaLoader : MonoBehaviour
    {
        [Header("Debug")] [SerializeField] private bool activateOnFirstTrigger;
        
        [SerializeField] private string sceneName;
        [SerializeField] private int numberOfDungeons;
        private int _playersReady;
        public SpriteRenderer door;
        public Sprite openDoorSprite;
        public bool isBlocked;
        public bool toDungeon;
        public bool toBoss;
        public SpriteRenderer activationArea;

        private bool _offSetActive;
        private static int lastDungeon;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            activationArea.color = Color.red;

            if (!toDungeon) return;
            if (lastDungeon == 0)
            {
                lastDungeon = Random.Range(1, numberOfDungeons + 1);
            }
            else
            {
                lastDungeon++;
                if (lastDungeon > numberOfDungeons)
                    lastDungeon = 1;
            }

            sceneName = "Dungeon_" + lastDungeon;
            //sceneName = "Dungeon_1";
        }

        private void Start()
        {
            StartCoroutine(WaitToActivate());
        }

        private IEnumerator WaitToActivate()
        {
            yield return new WaitForSeconds(2f);
            _offSetActive = true;

            if (DungeonProgressManager.instance != null)
            {
                Debug.Log("IsMinibossDefeated? " + DungeonProgressManager.instance.IsMinibossDefeated());
            
                if (DungeonProgressManager.instance.IsMinibossDefeated())
                {
                    Unlock();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            Debug.Log("TriggerEnter");
            if (isBlocked) return;

            if (activateOnFirstTrigger)
            {
                OnPlayersReady();
            }

            _playersReady++;
            switch (_playersReady)
            {
                case < 2:
                    PlayerUI.instance.ShowMessage("Waiting for other player...");
                    break;
                case > 2:
                    _playersReady = 2;
                    break;
            }

            if (_playersReady != 2) return;
            if (toDungeon) PlayerUI.instance.ShowMessage("Entering dungeon.");
            else if (toBoss) PlayerUI.instance.ShowMessage("Entering final area.");
            else PlayerUI.instance.ShowMessage("Returning to hub.");
            OnPlayersReady();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Debug.Log("TriggerExit");
            if (isBlocked) return;

            _playersReady--;
            if (_playersReady < 0) _playersReady = 0;
        }

        private void OnPlayersReady()
        {
            if(!_offSetActive)
                return;
            _offSetActive = false;
            
            if (door)
            {
                door.sprite = openDoorSprite;
                activationArea.color = Color.green;
                AudioManager.instance.PlayOpenGateSound();
            }

            if (NetworkManager.Singleton.IsHost)
            {
                SceneManager.instance.LoadSceneSingleDungeon(sceneName);
            }
        }

        public void Unlock()
        {
            // To call when miniboss is killed
            isBlocked = false;
            if (toBoss) PlayerUI.instance.ShowMessage("Final Area unlocked.");
            else PlayerUI.instance.ShowMessage("Return to hub unlocked.");
        }

        [ContextMenu("ForceNextAreaLoader")]
        public void ForceNextAreaLoader()
        {
            OnPlayersReady();
        }
    }
}