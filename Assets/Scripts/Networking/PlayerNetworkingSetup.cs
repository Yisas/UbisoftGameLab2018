using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetworkingSetup : NetworkBehaviour
{
    public SkinnedMeshRenderer playerMeshRenderer;
    public Material player2Material;
    public Material player1Material;
    public float clientExtraDropGap;

    private bool player1Host;       // Player 1 in host
    private bool player1Client;     // Player 1 in client
    private bool player2Client;     // Player 2 in client
    private bool player2Host;       // Player 2 in host

    private void Start()
    {
        player1Host = isServer && isLocalPlayer;
        player1Client = !isServer && !isLocalPlayer;
        player2Client = !isServer && isLocalPlayer;
        player2Host = isServer && !isLocalPlayer;

        PlayerMove playerMove = GetComponent<PlayerMove>();
        PlayerObjectInteraction playerObjectInteraction = GetComponent<PlayerObjectInteraction>();

        if (player1Host || player1Client)
        {
            gameObject.name = "Player 1";
             // Additional setup not needed since player 1 is default on prefab
        }
        else if (player2Host || player2Client)
        {
            gameObject.name = "Player 2";
            playerMove.PlayerID = 2;
            playerMeshRenderer.material = player2Material;
            playerObjectInteraction.fakeObjects[(int)PickupableObject.PickupableType.Player].GetComponentInChildren<SkinnedMeshRenderer>().material = player1Material;
            playerObjectInteraction.dropBox.transform.Translate(new Vector3(0, 0, clientExtraDropGap));

            int player2Layer = LayerMask.NameToLayer("Player 2");
            gameObject.layer = player2Layer;
            foreach(Transform child in transform)
            {
                child.gameObject.layer = player2Layer;
            }
        }
        else
        {
            Debug.LogError("Invalid player added through networking");
        }
    }
}