using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class NextAreaLoader : MonoBehaviour
{
    [Header("Debug")] [SerializeField] private bool activateOnFirstTrigger;
    
    private SceneManager _sceneManager;
    [SerializeField] private string sceneName;
    private int _playersReady;
    public SpriteRenderer door;
    public Sprite openDoorSprite;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        _sceneManager = FindObjectOfType<SceneManager>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("TriggerEnter");

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
        _playersReady--;
        if (_playersReady < 0) _playersReady = 0;
    }

    private void OnPlayersReady()
    {
        //_sceneManager.LoadSceneSingle(sceneName);
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        if (door)
        {
            door.sprite = openDoorSprite;
            yield return new WaitForSeconds(1f);
        }
        _sceneManager.LoadSceneSingle(sceneName);
    }
}