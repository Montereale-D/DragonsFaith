using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Network
{
    [RequireComponent(typeof(Collider2D))]
    public class NextAreaLoader : MonoBehaviour
    {
        [Header("Debug")] [SerializeField] private bool activateOnFirstTrigger;
    
        private SceneManager _sceneManager;
        [SerializeField] private string sceneName;
        private int _playersReady;
        public SpriteRenderer door;
        public Sprite openDoorSprite;
        public bool isBlocked;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            _sceneManager = FindObjectOfType<SceneManager>();
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
            StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            if (door)
            {
                door.sprite = openDoorSprite;
                yield return new WaitForSeconds(0.5f);
            }

            if (_sceneManager)
            {
                _sceneManager.LoadSceneSingle(sceneName);
            }
            else
            {
                Debug.LogWarning("Scene manager is null, Ok is appear in client");
            }
            
        }

        public void Unblock()
        {
            // To call when miniboss is killed
            isBlocked = false;
        }
    }
}