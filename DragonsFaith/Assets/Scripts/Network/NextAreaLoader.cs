using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NextAreaLoader : MonoBehaviour
{
    private SceneManager _sceneManager;
    [SerializeField] private string sceneName;
    private int _playersReady;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        _sceneManager = FindObjectOfType<SceneManager>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("TriggerEnter");
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
        _sceneManager.LoadSceneSingle(sceneName);
    }
}