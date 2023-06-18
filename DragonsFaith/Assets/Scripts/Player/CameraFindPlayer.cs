using Unity.Netcode;
using UnityEngine;

public class CameraFindPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject cameraHolder;

    private void Start()
    {
        if (IsOwner)
        {
            cameraHolder.SetActive(true);
            cameraHolder.GetComponentInChildren<Camera>().tag = "MainCamera";
        }
    }

    public void ActivateListener()
    {
        cameraHolder.GetComponentInChildren<AudioListener>().enabled = true;
    }
}
