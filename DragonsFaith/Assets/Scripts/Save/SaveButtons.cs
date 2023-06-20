using System.Collections;
using System.Collections.Generic;
using Save;
using UnityEngine;

public class SaveButtons : MonoBehaviour
{
    public void OnSaveClick()
    {
        DataManager.instance.SaveGameRequest();
    }
    
    public void OnLoadClick()
    {
        DataManager.instance.LoadGameRequest();
    }
}
