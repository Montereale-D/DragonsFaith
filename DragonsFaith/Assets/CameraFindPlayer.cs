using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Cinemachine;
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
        }
    }
}
