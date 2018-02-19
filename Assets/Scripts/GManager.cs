using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GManager : MonoBehaviour
{
    public void ResetAllResetableObjects(bool resetPlayers)
    {
        foreach (ResettableObject ro in GameObject.FindObjectsOfType<ResettableObject>())
        {
            if(!resetPlayers && ro.gameObject.tag == "Player")
            {
                continue;
            }
            ro.Reset();
        }
    }
}
