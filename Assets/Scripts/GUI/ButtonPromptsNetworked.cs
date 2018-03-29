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

    void Update()
    { 
        if(player && player.isLocalPlayer && Canvas_Player == null)
        {
            NetworkPlayerPromptReferenceStart(player.transform.GetComponentInChildren<Canvas>());
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

    #region Triggers
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

    #endregion

    public void TurnOnPrompt()
    {

        if (buttonprompt == ButtonPromptOn.pressureplate)  // If the prompt is suppose to appear above the preasure plate
        {
            //Here we are going to make the prompt on the Pressureplate only visible to the local player. 
            if (player.GetComponent<PlayerObjectInteraction>().newHeldObj == PlayerObjectInteraction.HoldableType.Pickup ) //Checking to see if a player is holding a Pickupable block
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


                    if (player.GetComponent<PlayerObjectInteraction>().newHeldObj == PlayerObjectInteraction.HoldableType.Pickup) //Checking to see if a player is holding a Pickupable block
                    {
                        Canvas_Player.gameObject.GetComponent<Canvas>().enabled = false;
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
                        
            if (buttonprompt == ButtonPromptOn.player)
            {
               
                if (gameObject.name == "PushablePromptTrigger" || gameObject.name == "PickupPromptTrigger" )
                {
                    Canvas_Player.gameObject.GetComponent<Canvas>().enabled = true;                   
                }
            }
        }
    }


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
        }       
    }    

}
