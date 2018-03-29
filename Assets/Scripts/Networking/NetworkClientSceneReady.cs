
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
                PlayerMove[] players = GameObject.FindObjectsOfType<PlayerMove>();

                foreach (PlayerMove pm in players)
                {
                    if (pm.PlayerID == 2)
                    {
                        return;
                    }
                }

                ClientScene.AddPlayer(2);
            }
        }
    }
}
