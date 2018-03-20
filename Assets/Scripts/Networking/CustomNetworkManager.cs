using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        GetComponent<NetworkManagerHUD>().showGUI = false;
        GameObject.FindGameObjectWithTag("MenuUI").GetComponent<Canvas>().enabled = true;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        GetComponent<NetworkManagerHUD>().showGUI = false;
    }
}
