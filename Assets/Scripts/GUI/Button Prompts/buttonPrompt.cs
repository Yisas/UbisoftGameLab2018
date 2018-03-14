using System.Collections;
using System.Collections.Generic;
using
UnityEngine.UI;
using UnityEngine;

public class buttonPrompt : MonoBehaviour
{

    [SerializeField]
    Canvas Canvas_PresurePlate;
    [SerializeField]
    Canvas Canvas_Player_1;
    [SerializeField]
    Canvas Canvas_Player_2;

    public enum ButtonPromptOn { pressureplate, player }
    public ButtonPromptOn buttonprompt;

    public Image JumpImgP1;
    public Image InteractImgP1;

    public Image JumpImgP2;
    public Image InteractImgP2;

    PlayerMove player;
    int playerID;

    private bool isBeingControlled;

    void Start()
    {
        JumpImgP1.enabled = false;
        InteractImgP1.enabled = false;

        JumpImgP2.enabled = false;
        InteractImgP2.enabled = false;

        Canvas_PresurePlate.enabled = false;
        Canvas_Player_1.enabled = false;
        Canvas_Player_2.enabled = false;
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
            /**
            if (Player1.GetComponent<PlayerMove>().IsHoldingPickup)
            {
                Canvas_PresurePlate.gameObject.layer = 12;  //"Invisible Player 2 Layer
            }

            if (Player2.GetComponent<PlayerMove>().IsHoldingPickup)
            {
                Canvas_PresurePlate.gameObject.layer = 9;   //"Invisible Player 1 Layer"
            }

            Canvas_PresurePlate.enabled = true;
        }*/

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
        else if (buttonprompt == ButtonPromptOn.player)
        {
            //Switching the image prompts
            if (gameObject.name == "JumpPromptTrigger")     //(A: Jumpp)
            {
                if (playerID == 1)
                {
                    if(player.jumpPromptConter > 0)
                    {
                        JumpImgP1.enabled = true;
                        InteractImgP1.enabled = false;
                        Canvas_Player_1.enabled = true;
                    }
                    else
                    {
                        JumpImgP1.enabled = false;
                    }
                }

                if (playerID == 2)
                {
                    if(player.jumpPromptConter > 0)
                    {
                        JumpImgP2.enabled = true;
                        InteractImgP2.enabled = false;
                        Canvas_Player_2.enabled = true;
                    }
                    else
                    {
                        JumpImgP2.enabled = false;
                    }
                }
            }
            else                                            //(B: Interact)
            {
                if (playerID == 1)
                {
                    JumpImgP1.enabled = false;
                    InteractImgP1.enabled = true;
                }

                if (playerID == 2)
                {
                    JumpImgP2.enabled = false;
                    InteractImgP2.enabled = true;
                }
            }

            if (gameObject.name == "PushablePromptTrigger" || gameObject.name == "PickupPromptTrigger" || gameObject.name == "PlayerPromptTrigger")
            {
                if (playerID == 1)
                {
                    if (isBeingControlled == false && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                    {
                        Canvas_Player_1.enabled = true;
                        isBeingControlled = true;
                    }
                    else if (isBeingControlled == false && (player.IsGrabingPushable == false || !player.IsHoldingPickup == false))
                    {
                        Canvas_Player_1.enabled = true;
                    }
                    else if (isBeingControlled == true && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                    {
                        Canvas_Player_1.enabled = true;
                    }
                    else if (isBeingControlled == true && (player.IsGrabingPushable == false || player.IsHoldingPickup == false))
                    {
                        Canvas_Player_1.enabled = false;
                        isBeingControlled = false;
                    }
                    //dont think we need this else
                    /*else 
                    {
                        Debug.Log("P1: 5");
                        Canvas_Player_1.enabled = true;
                    }*/
                }

                if (playerID == 2)
                {
                    if (isBeingControlled == false && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                    {
                        isBeingControlled = true;
                        Canvas_Player_2.enabled = true;
                    }
                    else if (isBeingControlled == false && (player.IsGrabingPushable == false || player.IsHoldingPickup == false))
                    {
                        Canvas_Player_2.enabled = true;
                    }
                    else if (isBeingControlled == true && (player.IsGrabingPushable == true || player.IsHoldingPickup == true))
                    {
                        Canvas_Player_2.enabled = true;
                    }
                    else if (isBeingControlled == true && (player.IsGrabingPushable == false || !player.IsHoldingPickup) == false)
                    {
                        Canvas_Player_2.enabled = false;
                        isBeingControlled = false;
                    }
                    //dont think we need this else
                    /*else
                    {
                        Debug.Log("P2: 5");
                        Canvas_Player_2.enabled = true;
                    }*/
                }
            }
        }
    }
    private void TurnOffPrompt()
    {
        if (buttonprompt == ButtonPromptOn.pressureplate)
        {
            Canvas_PresurePlate.enabled = false;
        }
        else if (buttonprompt == ButtonPromptOn.player)
        {
            if (playerID == 1)
                Canvas_Player_1.enabled = false;
            if (playerID == 2)
                Canvas_Player_2.enabled = false;
        }
    }

}
