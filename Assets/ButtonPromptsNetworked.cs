using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;

public class ButtonPromptsNetworked : MonoBehaviour
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
    int lastPlayerID;
    int playerID;
    NetworkIdentity playerNetID;    //NOTE: Instead we should use the PlayerMove player and get the player.isLocalPlayer??
                                    //ATM: The code uses playerNetID--might be redunant though

    public bool isBeingControlled/* = false*/;


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

    //TRIGGERS ARE HERE
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject.GetComponent<PlayerMove>();

            if (!player.isLocalPlayer)
            {
                return;
            }

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
            if (!player.isLocalPlayer)
            {
                return;
            }

            playerID = other.gameObject.GetComponent<PlayerMove>().PlayerID;
            TurnOffPrompt();
        }
    }

    public void TurnOnPrompt()
    {
        /**        
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
            Debug.Log("HELLLO IM HERE !" + player.IsHoldingPickup);
            //Here we are going to make the prompt on the Pressureplate only visible to the local player. 
            if (player.GetComponent<PlayerObjectInteraction>().newHeldObj == PlayerObjectInteraction.HoldableType.Pickup) //Checking to see if a player is holding a Pickupable block
            {
                Canvas_PresurePlate.gameObject.GetComponent<Canvas>().enabled = true;
            }
        }
        else if (buttonprompt == ButtonPromptOn.junk)   // If the prompt is suppose to appear above the Junk Items
        {
            Canvas_Junk.gameObject.GetComponent<Canvas>().enabled = true;
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

                    Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;

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
                    if (isBeingControlled == false && (player.GetComponent<PlayerObjectInteraction>().newHeldObj == PlayerObjectInteraction.HoldableType.Pushable || player.GetComponent<PlayerObjectInteraction>().newHeldObj == PlayerObjectInteraction.HoldableType.Pickup))
                    {
                        Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;
                        lastPlayerID = player.PlayerID;
                        isBeingControlled = true;                       

                        Debug.Log("1: player- " + player.PlayerID + " name of object: " + gameObject.name + " isBeingControlled: " + isBeingControlled
                                + "\n...SERVER? " + player.isServer + "   CLIENT? " + player.isClient);

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
                    else if (isBeingControlled == false && (player.GetComponent<PlayerObjectInteraction>().newHeldObj == PlayerObjectInteraction.HoldableType.Pushable || player.GetComponent<PlayerObjectInteraction>().newHeldObj == PlayerObjectInteraction.HoldableType.Pickup)) //!player.IsHoldingPickup == false
                    {
                        Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;

                        Debug.Log("2: player- " + player.PlayerID + " name of object: " + gameObject.name + " isBeingControlled: " + isBeingControlled
                                        + "\n...SERVER? " + player.isServer + "   CLIENT? " + player.isClient);
                    }
                    #region old conditions
                    //Might not require this condition...
                    //else if (isBeingControlled == true && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                    //{                        
                    //    Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;

                    //    Debug.Log("3: player- " + player.PlayerID + " name of object: " + gameObject.name + " isBeingControlled: " + isBeingControlled
                    //                    + "\n...SERVER? " + player.isServer + "   CLIENT? " + player.isClient);
                    //}
                    //else if (isBeingControlled == true && (player.IsGrabingPushable == false || player.IsHoldingPickup == false))
                    //{
                    //    Canvas_Player.gameObject.GetComponent<Canvas>().enabled = false;
                    //    isBeingControlled = false;
                    //    //BeingControlled(false, playerNetID.isServer);
                    //    Debug.Log("4: player- " + player.PlayerID + " name of object: " + gameObject.name + " isBeingControlled: " + isBeingControlled
                    //                    + "\n...SERVER? " + player.isServer + "   CLIENT? " + player.isClient);

                    #region TRYING COMMAND & CLIENTRPC ----Comments
                    //    //if (!playerNetID.isServer)
                    //    //{
                    //    //    return;
                    //    //}

                    //    //if (playerNetID.isLocalPlayer && playerNetID.isServer)
                    //    //    RpcUpdatePrompt(isBeingControlled, playerNetID.isServer);
                    //    //else if (playerNetID.isLocalPlayer && !playerNetID.isServer)
                    //    //    CmdPrompt(isBeingControlled);

                    //    //    CmdPrompt(isBeingControlled);
                    #endregion
                    //}
                    #endregion
                }
                //if (!player.isServer)
                //{
                //    Debug.Log("1: COMMAND");
                //    CmdButtonPrompt(isBeingControlled);
                //}
            }
        }
    }


    //[Command] // client to the server. 
    //void CmdButtonPrompt(bool m_isBeingControlled)
    //{
    //    Debug.Log("COMMAND: m_beingControlled--> " + m_isBeingControlled);
    //    //RpcUpdatePrompt(m_isBeingControlled);
    //    isBeingControlled = m_isBeingControlled;
    //}

    //[ClientRpc]
    //void RpcUpdatePrompt(bool m_isBeingControlled)
    //{
    //    isBeingControlled = m_isBeingControlled;
    //    Debug.Log("CLIENTRPC Here??" + isBeingControlled);
    //}


    public void TurnOffPrompt()
    {
        if (buttonprompt == ButtonPromptOn.junk)
        {
            if (Canvas_Junk != null)
            {
                Canvas_Junk.enabled = false;
            }
        }
        if (buttonprompt == ButtonPromptOn.pressureplate)
        {
            if (Canvas_PresurePlate != null)
            {
                Canvas_PresurePlate.enabled = false;
            }
        }
        else if (buttonprompt == ButtonPromptOn.player)
        {
            if (Canvas_Player != null)
            {
                Canvas_Player.enabled = false;
            }

            //if (player.PlayerID != lastPlayerID)    //Check the ID to see if it matches with the previous ID
            //{
            //    //isBeingControlled = true;
            //    return;//didnt work
            //}
            //else
            //{
            //    isBeingControlled = false;               
            //}

            //if (!player.isServer)
            //{
            //    Debug.Log("EXIT: COMMAND");
            //    CmdButtonPrompt(isBeingControlled);
            //}
        }

       
    }

    public PlayerMove Player
    {
        get { return player; }
        set { player = value; }
    }

}
