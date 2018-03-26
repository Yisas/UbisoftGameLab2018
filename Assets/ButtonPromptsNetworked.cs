using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;

public class ButtonPromptsNetworked : NetworkBehaviour /*MonoBehaviour*/
{

    /**NOTE FOR NETWORKING VERSION:
    *   For this version, I'm removing all the player 2 stuff.
    *   Because from my understanding, in the networking version there will be only one
    *   player and they will have their own camera, and canvases, etc.
    *   Therefore, each character will have there own scripts ;)
    */

    [SerializeField]
    Canvas Canvas_Junk;
    [SerializeField]
    Canvas Canvas_PresurePlate;
    [SerializeField]
    Canvas Canvas_Player;

    public enum ButtonPromptOn { pressureplate, junk, player }
    public ButtonPromptOn buttonprompt;

    public Image JumpImg;
    public Image InteractImg;

    PlayerMove player;
    int playerID;
    NetworkIdentity playerNetID;    //NOTE: Instead we should use the PlayerMove player and get the player.isLocalPlayer??
                                    //ATM: The code uses playerNetID--might be redunant though
    [SyncVar]
    /*private*/
    bool isBeingControlled = false;

    ResettableObject isBeingHeld;

    private void Awake()
    {
        if (gameObject.name == "PushablePromptTrigger" || gameObject.name == "PickupPromptTrigger") //Includes the junk
        {
            Canvas_PresurePlate = null;
        }
        if (gameObject.name == "PlayerButtonPrompt")
        {
            Canvas_PresurePlate = null;
            Canvas_Junk = null;
        }
        if (gameObject.name == "JumpPromptTrigger")
        {
            Canvas_Player = null;
            Canvas_PresurePlate = null;
            Canvas_Junk = null;
            JumpImg = null;
            InteractImg = null;
        }
    }

    void Start()
    {
        //We do not see the prompts above the Pressureplate and Junk items at the start of the game
        if (Canvas_Junk != null)
        {
            Canvas_Junk.enabled = false;
        }
        if (Canvas_PresurePlate != null)
        {
            Canvas_PresurePlate.enabled = false;
        }
        if (Canvas_Player != null)
        {
            Canvas_Player.enabled = false;
        }
    }

    /// <summary>
    /// References to the player canvas need to be aquired at runtime
    /// </summary>
    /// <param name="canvasPlayer"></param>
    public void NetworkPlayerPromptReferenceStart(Canvas canvasPlayer)
    {
        Canvas_Player = canvasPlayer;

        //Also set the image of the Prompts
        Image[] imgType = canvasPlayer.GetComponentsInChildren<Image>();
        if (imgType[0].name == "Jump Img")
        {
            JumpImg = imgType[0];
        }
        if (imgType[1].name == "Interact Img ")
        {
            InteractImg = imgType[1];
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject.GetComponent<PlayerMove>();
            playerID = other.gameObject.GetComponent<PlayerMove>().PlayerID;
            playerNetID = other.GetComponentInParent<NetworkIdentity>();    //Getting the players network identity...
            TurnOnPrompt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject.GetComponent<PlayerMove>();
            playerID = other.gameObject.GetComponent<PlayerMove>().PlayerID;
            TurnOffPrompt();
        }
    }

    private void TurnOnPrompt()
    {
        /**
        Debug.Log("happened?");
        if (playerNetID.isLocalPlayer)
        {
            Debug.Log("LOCAL " + gameObject.name);
        }

        if (playerNetID.isServer)
        {
            Debug.Log("SERVER" + gameObject.name);
        }

        if (playerNetID.isClient)
        {
            Debug.Log("CLIENT " + gameObject.name);
        }
        */

        if (buttonprompt == ButtonPromptOn.pressureplate)  // If the prompt is suppose to appear above the preasure plate
        {
            //Here we are going to make the prompt on the Pressureplate only visible to the local player. 
            if (player.IsHoldingPickup == true) //Checking to see if a player is holding a Pickupable block
            {
                if (playerNetID.isLocalPlayer)
                {
                    Canvas_PresurePlate.gameObject.GetComponent<Canvas>().enabled = true;
                }

                if (!player.isLocalPlayer)
                {
                    Canvas_PresurePlate.gameObject.GetComponent<Canvas>().enabled = false;
                }
            }
        }
        else if (buttonprompt == ButtonPromptOn.junk)   // If the prompt is suppose to appear above the Junk Items
        {
            //Here we are going to make the prompt on the Junk Items only visible to the local player. 
            if (playerNetID.isLocalPlayer)
            {
                Canvas_Junk.gameObject.GetComponent<Canvas>().enabled = true;
            }

            if (!player.isLocalPlayer)
            {
                Canvas_Junk.gameObject.GetComponent<Canvas>().enabled = false;
            }
        }
        else if (buttonprompt == ButtonPromptOn.player) // If the prompt is suppose to appear above the Player
        {
            //Switching the image prompts           
            if (gameObject.name == "JumpPromptTrigger")     //(A: Jumpp)
            {
                if (player.jumpPromptConter > 0)
                {
                    JumpImg.enabled = true;
                    InteractImg.enabled = false;

                    if (playerNetID.isLocalPlayer && playerID == 1)
                    {
                        Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                    }

                    if (playerNetID.isLocalPlayer && playerID == 2)
                    {
                        Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                    }
                }
                else
                {
                    JumpImg.enabled = false;
                }
            }
            else                                            //(B: Interact)
            {
                if (JumpImg != null && InteractImg != null)
                {
                    JumpImg.enabled = false;
                    InteractImg.enabled = true;
                }
            }

            //REQUIRES SOME TYPE OF FIX-- THE CONDITIONS ARE NOT BEING STATISFIED BY THE PLAYER INDUVIDUALLY eg. isBeingHeld
            isBeingHeld = gameObject.GetComponentInParent<ResettableObject>();
            if (buttonprompt == ButtonPromptOn.player)
            {
                if (gameObject.name == "PushablePromptTrigger" || gameObject.name == "PickupPromptTrigger" || gameObject.name == "PlayerButtonPrompt")
                {
                    NetworkIdentity gameObjectNetID = gameObject.GetComponentInParent<NetworkIdentity>();
                    if (isBeingControlled == false && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                    {
                        Debug.Log("1: player- " + playerID + " name of object: " + gameObject.name + " isBeingControlled: " + isBeingControlled
                                    + "\n...SERVER? " + playerNetID.isServer + "   CLIENT? " + playerNetID.isClient);
                        if (playerNetID.isLocalPlayer && playerID == 1)
                        {
                            Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                        }

                        if (playerNetID.isLocalPlayer && playerID == 2)
                        {
                            Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                        }
                        //isBeingControlled = true;   
                        BeingControlled(true, playerNetID.isServer);
                        Debug.Log("1: IS BEING CONTROLLED IS " + isBeingControlled + "Player: " + playerID);

                        #region TRYING COMMAND & CLIENTRPC ----Comments
                        //if (!playerNetID.isServer)
                        //{
                        //    return;
                        //}

                        //if (playerNetID.isLocalPlayer && playerNetID.isServer)
                        //    RpcUpdatePrompt(isBeingControlled, playerNetID.isServer);
                        //else if (playerNetID.isLocalPlayer && !playerNetID.isServer)
                        //    CmdPrompt(isBeingControlled);

                        //CmdPrompt(isBeingControlled);
                        #endregion
                    }
                    else if (isBeingControlled == false && (player.IsGrabingPushable == false || player.IsHoldingPickup == false)) //!player.IsHoldingPickup == false
                    {
                        Debug.Log("2: player- " + playerID + " name of object: " + gameObject.name + " isBeingControlled: " + isBeingControlled
                                        + "\n...SERVER? " + playerNetID.isServer + "   CLIENT? " + playerNetID.isClient);
                        if (playerNetID.isLocalPlayer && playerID == 1)
                        {
                            Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                        }
                        if (playerNetID.isLocalPlayer && playerID == 2)
                        {
                            Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                        }
                    }
                    else if (isBeingControlled == true && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                    {
                        Debug.Log("3: player- " + playerID + " name of object: " + gameObject.name + " isBeingControlled: " + isBeingControlled
                                        + "\n...SERVER? " + playerNetID.isServer + "   CLIENT? " + playerNetID.isClient);
                        if (playerNetID.isLocalPlayer && playerID == 1)
                        {
                            Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                        }
                        if (playerNetID.isLocalPlayer && playerID == 2)
                        {
                            Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                        }
                    }
                    else if (isBeingControlled == true && (player.IsGrabingPushable == false || player.IsHoldingPickup == false))
                    {
                        Debug.Log("4: player- " + playerID + " name of object: " + gameObject.name + " isBeingControlled: " + isBeingControlled
                                        + "\n...SERVER? " + playerNetID.isServer + "   CLIENT? " + playerNetID.isClient);
                        if (playerNetID.isLocalPlayer && playerID == 1)
                        {
                            Canvas_Player.gameObject.GetComponent<Canvas>().enabled = false;
                        }
                        if (playerNetID.isLocalPlayer && playerID == 2)
                        {
                            Canvas_Player.gameObject.GetComponent<Canvas>().enabled = false;
                        }
                        //isBeingControlled = false;
                        BeingControlled(false, playerNetID.isServer);
                        Debug.Log("4: IS BEING CONTROLLED IS " + isBeingControlled + "Player: " + playerID);

                        #region TRYING COMMAND & CLIENTRPC ----Comments
                        //if (!playerNetID.isServer)
                        //{
                        //    return;
                        //}

                        //if (playerNetID.isLocalPlayer && playerNetID.isServer)
                        //    RpcUpdatePrompt(isBeingControlled, playerNetID.isServer);
                        //else if (playerNetID.isLocalPlayer && !playerNetID.isServer)
                        //    CmdPrompt(isBeingControlled);

                        //    CmdPrompt(isBeingControlled);
                        #endregion
                    }
                }
                    if (playerNetID.isLocalPlayer && playerNetID.isServer)
                    {
                        RpcUpdatePrompt(isBeingControlled, playerNetID.isServer);
                    }
                    else if (playerNetID.isLocalPlayer && !playerNetID.isServer)
                    {
                        CmdPrompt(isBeingControlled, playerNetID.isServer);
                    }
            }
        }
    }

    void BeingControlled(bool controlled, bool server)
    {
        if (!server)
        {
            return;
        }
        isBeingControlled = controlled;
    }

    [Command] // client to the server. 
    void CmdPrompt(bool m_isBeingControlled, bool server)
    {
        Debug.Log("Command: m_beingControlled--> " + m_isBeingControlled);
        //RpcUpdatePrompt(m_isBeingControlled);
        isBeingControlled = m_isBeingControlled;
        //isBeingControlled = m_isBeingControlled;
    }

    [ClientRpc]
    void RpcUpdatePrompt(bool m_isBeingControlled, bool server)
    {
        if (!playerNetID.isServer) return;
        isBeingControlled = m_isBeingControlled;
        Debug.Log("Here??" + isBeingControlled);
    }


    private void TurnOffPrompt()
    {
        if (buttonprompt == ButtonPromptOn.junk)
        {
            Canvas_Junk.enabled = false;
        }
        if (buttonprompt == ButtonPromptOn.pressureplate)
        {
            Canvas_PresurePlate.enabled = false;
        }
        else if (buttonprompt == ButtonPromptOn.player)
        {
            if (Canvas_Player != null)
            {
                Canvas_Player.enabled = false;
            }
        }
    }


}
