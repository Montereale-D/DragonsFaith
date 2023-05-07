using System.Collections;
using System.Collections.Generic;
using Save;
using UnityEngine;

public class SaveButtons : MonoBehaviour
{
    public void OnSaveClick()
    {
        DataManager.Instance.SaveGameRequest();
    }
    
    public void OnLoadClick()
    {
        DataManager.Instance.LoadGameRequest();
    }
}
