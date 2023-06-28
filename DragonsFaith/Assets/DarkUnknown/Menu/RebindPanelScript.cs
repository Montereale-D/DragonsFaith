using System;
using UI;
using UnityEngine;

public class RebindPanelScript : MonoBehaviour
{
    private void OnEnable()
    {
        MenuManager.isChangingKey = true;
        Debug.Log("enabled");
    }

    private void OnDisable()
    {
        MenuManager.isChangingKey = false;
        Debug.Log("disabled");
    }
}
