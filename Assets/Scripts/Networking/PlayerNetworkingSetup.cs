using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetworkingSetup : NetworkBehaviour
{
    public GameObject player1Model;
    public GameObject player2Model;
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
            Destroy(player2Model);
        }
        else if (player2Host || player2Client)
        {
            gameObject.name = "Player 2";
            playerMove.PlayerID = 2;
            playerObjectInteraction.dropBox.transform.Translate(new Vector3(0, 0, clientExtraDropGap));

            Destroy(player1Model);
            player2Model.SetActive(true);

            playerMove.animator = player2Model.GetComponent<Animator>();
            playerObjectInteraction.animator = player2Model.GetComponent<Animator>();

            int player2Layer = LayerMask.NameToLayer("Player 2");
            gameObject.layer = player2Layer;
            foreach (Transform child in transform)
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