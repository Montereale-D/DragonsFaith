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
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
        }
    }
}
