using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;

public class ButtonPromptsNetworked : NetworkBehaviour
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

    public Image JumpImgP1;
    public Image InteractImgP1;

    PlayerMove player;
    int playerID;

    private bool isBeingControlled;

    void Start()
    {
        JumpImgP1.enabled = false;
        InteractImgP1.enabled = false;

        Canvas_Junk.enabled = false;
        Canvas_PresurePlate.enabled = false;
    }

    /// <summary>
    /// References to the player canvas need to be aquired at runtime
    /// </summary>
    /// <param name="canvasPlayer"></param>
    public void NetworkPlayerPromptReferenceStart(Canvas canvasPlayer)
    {
        Canvas_Player = canvasPlayer;
        Canvas_Player.enabled = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject.GetComponent<PlayerMove>();
            playerID = other.gameObject.GetComponent<PlayerMove>().PlayerID;
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
        if (buttonprompt == ButtonPromptOn.pressureplate)  // If the prompt is appearing above the preasure plate
        {
            if (player.IsHoldingPickup == true) //Checking to see if a player is holding a Pickupable block
            {
                if (playerID == 1)
                {
                    Canvas_PresurePlate.gameObject.layer = LayerMask.NameToLayer("Invisible Player 2");  //"Invisible Player 2 Layer"                    
                }
                if (playerID == 2)
                {
                    Canvas_PresurePlate.gameObject.layer = LayerMask.NameToLayer("Invisible Player 1");   //"Invisible Player 1 Layer"
                }
                Canvas_PresurePlate.enabled = true;
            }
        }
        else if (buttonprompt == ButtonPromptOn.junk)
        {
            if (playerID == 1)
            {
                Canvas_Junk.gameObject.layer = LayerMask.NameToLayer("Invisible Player 2");  //"Invisible Player 2 Layer"                    
            }
            if (playerID == 2)
            {
                Canvas_Junk.gameObject.layer = LayerMask.NameToLayer("Invisible Player 1");   //"Invisible Player 1 Layer"
            }
            Canvas_Junk.enabled = true;
        }
        else if (buttonprompt == ButtonPromptOn.player)
        {
            //Switching the image prompts
            if (gameObject.name == "JumpPromptTrigger")     //(A: Jumpp)
            {
                if (player.jumpPromptConter > 0)
                {
                    JumpImgP1.enabled = true;
                    InteractImgP1.enabled = false;
                    Canvas_Player.enabled = true;
                }
                else
                {
                    JumpImgP1.enabled = false;
                }
            }
            else                                            //(B: Interact)
            {
                JumpImgP1.enabled = false;
                InteractImgP1.enabled = true;
            }

            if (gameObject.name == "PushablePromptTrigger" || gameObject.name == "PickupPromptTrigger" || gameObject.name == "PlayerPromptTrigger")
            {
                if (isBeingControlled == false && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                {
                    Canvas_Player.enabled = true;
                    isBeingControlled = true;
                }
                else if (isBeingControlled == false && (player.IsGrabingPushable == false || !player.IsHoldingPickup == false))
                {
                    Canvas_Player.enabled = true;
                }
                else if (isBeingControlled == true && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                {
                    Canvas_Player.enabled = true;
                }
                else if (isBeingControlled == true && (player.IsGrabingPushable == false || player.IsHoldingPickup == false))
                {
                    Canvas_Player.enabled = false;
                    isBeingControlled = false;
                }
            }
        }
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
            Canvas_Player.enabled = false;
        }
    }
}
