using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientSceneReady : NetworkBehaviour
{
    // Use this for initialization
    void Start()
    {
        if (!isServer)
        {
            ClientScene.AddPlayer(2);
            CmdStartGameManagers();
        }
    }

    [Command]
    public void CmdStartGameManagers()
    {
        GManager.Instance.StartGameManagers();
    }
}
