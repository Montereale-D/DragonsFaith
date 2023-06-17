using System;
using System.Collections;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Network
{
    [RequireComponent(typeof(Collider2D))]
    public class NextAreaLoader : MonoBehaviour
    {
        [Header("Debug")] [SerializeField] private bool activateOnFirstTrigger;

        private SceneManager _sceneManager;
        [SerializeField] private string sceneName;
        [SerializeField] private int numberOfDungeons;
        private int _playersReady;
        public SpriteRenderer door;
        public Sprite openDoorSprite;
        public bool isBlocked;
        public bool toDungeon;

        private bool _offSetActive;
        private static int lastDungeon;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            _sceneManager = FindObjectOfType<SceneManager>();

            if (toDungeon)
            {
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
            if (_playersReady > 2) _playersReady = 2;
            if (_playersReady == 2) OnPlayersReady();
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
            
            if (door)
            {
                door.sprite = openDoorSprite;
            }

            if (_sceneManager)
            {
                _sceneManager.LoadSceneSingleDungeon(sceneName);
            }
            else
            {
                Debug.LogWarning("Scene manager is null, Ok is appear in client");
            }
            //StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            TransitionBackground.instance.FadeOut();
            yield return new WaitForSeconds(1f);
            //TransitionBackground.instance.FadeIn();

            if (_sceneManager)
            {
                _sceneManager.LoadSceneSingle(sceneName);
            }
            else
            {
                Debug.LogWarning("Scene manager is null, Ok is appear in client");
            }
        }

        public void Unlock()
        {
            // To call when miniboss is killed
            isBlocked = false;
            PlayerUI.instance.ShowMessage("Final Area unlocked.");
        }

        [ContextMenu("ForceNextAreaLoader")]
        public void ForceNextAreaLoader()
        {
            OnPlayersReady();
        }
    }
}