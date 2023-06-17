using System;
using Network;
using Unity.Netcode;
using UnityEngine;

public class GameOverButton : MonoBehaviour
{
    public void OnClick()
    {
        SceneManager.instance.ReturnToMainMenu(NetworkManager.Singleton.IsHost);
    }
}
