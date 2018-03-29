﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientSceneReady : NetworkBehaviour
{
    // Use this for initialization
    void Start()
    {
        if (NetworkManager.singleton.GetComponent<CustomNetworkManagerHUD>().useLANCorrection)
        {
            if (!isServer)
            {
                ClientScene.AddPlayer(2);
            }
        }
    }
}
