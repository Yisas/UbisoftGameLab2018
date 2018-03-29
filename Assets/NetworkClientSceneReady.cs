using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientSceneReady : NetworkBehaviour
{
    // Use this for initialization
    void Start()
    {
        if (isClient)
            ClientScene.Ready(NetworkManager.singleton.client.connection);
    }
}
