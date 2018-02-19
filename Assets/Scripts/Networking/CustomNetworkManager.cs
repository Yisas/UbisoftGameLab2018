using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {

    public Material player2Material;
    private int playersConnected = 0;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        playersConnected++;
        // DELETEME BEFORE MERGE
        Debug.Log(playersConnected + " players connected");
        base.OnServerAddPlayer(conn, playerControllerId);

        if(playersConnected == 2)
        {
            GameObject[] test =  GameObject.FindGameObjectsWithTag("Player");
            Debug.Log(test.Length);
            test[0].GetComponent<PlayerNetworkingSetup>().SetPlayerMaterial(player2Material);
        }
        else if(playersConnected > 2)
        {
            Debug.LogWarning("More than one player connected somehow!");
        }
    }
}