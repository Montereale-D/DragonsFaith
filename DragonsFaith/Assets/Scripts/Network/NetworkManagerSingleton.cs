using System;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkManagerSingleton : MonoBehaviour
    {
        public static NetworkManagerSingleton instance { get; private set; }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(instance.gameObject);
                instance = this;
                GetComponent<NetworkManager>().SetSingleton();
                DontDestroyOnLoad(this);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
        }
    }
}
