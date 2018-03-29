#define LAN_CONNECTION_FIX

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientSceneReady : NetworkBehaviour
{
    // Use this for initialization
    void Start()
    {
#if LAN_CONNECTION_FIX
        if (!isServer)
        {
            ClientScene.AddPlayer(2);
        }
#endif
    }
}
