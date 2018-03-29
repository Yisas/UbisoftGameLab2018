using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    public int[] buildIndexWithoutPlayerSpawn;

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

#if UNITY_EDITOR
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
            return;
#endif

        GetComponent<CustomNetworkManagerHUD>().showGUI = false;
        GameObject.FindGameObjectWithTag("MenuUI").GetComponent<Canvas>().enabled = true;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {

        base.OnClientConnect(conn);

#if UNITY_EDITOR
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
            return;
#endif

        GetComponent<CustomNetworkManagerHUD>().showGUI = false;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        for(int i =0; i < buildIndexWithoutPlayerSpawn.Length; i++)
        {
            if(SceneManager.GetActiveScene().buildIndex == buildIndexWithoutPlayerSpawn[i])
            {
                return;
            }
        }

        base.OnServerAddPlayer(conn, playerControllerId);
    }
}
