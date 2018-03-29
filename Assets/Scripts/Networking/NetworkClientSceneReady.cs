
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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

                if(SceneManager.GetActiveScene().buildIndex == 13)
                {
                    Debug.Log("No manual spawning on level 7");
                    return;
                }

                ClientScene.AddPlayer(2);
            }
        }
    }
}
