using System.Collections;
using System.Collections.Generic;
using
UnityEngine.UI;
using UnityEngine;

public class buttonPrompt : MonoBehaviour{

    [SerializeField]
    Canvas Canvas_PresurePlate;
    [SerializeField]
    Canvas Canvas_Player_1;
    [SerializeField]
    Canvas Canvas_Player_2;

    public enum ButtonPromptOn {pressureplate, player}
    public ButtonPromptOn buttonprompt;

    PlayerMove player;
    int playerID;

    void Start()
    {
        Canvas_PresurePlate.enabled = false;
        Canvas_Player_1.enabled = false;
        Canvas_Player_2.enabled = false;
    }

    void OnTriggerEnter(Collider other)
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
                if(playerID == 1)
                {
                    Canvas_PresurePlate.gameObject.layer = 12;  //"Invisible Player 2 Layer"                    
                }
                if (playerID == 2)
                {
                    Canvas_PresurePlate.gameObject.layer = 9;   //"Invisible Player 1 Layer"
                }
                Canvas_PresurePlate.enabled = true;
            }
        }
        else if (buttonprompt == ButtonPromptOn.player)
        {
            if (playerID == 1)
            {
                Canvas_Player_1.enabled = true;
                if ((gameObject.name == "PushablePromptTrigger" || gameObject.name == "PickupPromptTrigger" || gameObject.name == "PlayerPromptTrigger")
                     && (player.IsGrabPushable || player.IsHoldingPickup)) // NOTE : [BUG] Does not work
                                                                                //The other player should not be able to see the prompt if another player 
                                                                                //holding/grabbing a Pickupable or Pushable....
                {
                    Debug.Log("Player 1 and Holding or Grabbing Item. Player 2 should not be able to see prompt");
                    Canvas_Player_2.enabled = false;
                }
            }

            if (playerID == 2)
            {
                Canvas_Player_2.enabled = true;
                if ((gameObject.name == "PushablePromptTrigger" || gameObject.name == "PickupPromptTrigger" || gameObject.name == "PlayerPromptTrigger")
                     && (player.IsGrabPushable || player.IsHoldingPickup)) // NOTE : [BUG] Does not work
                                                                              //The other player should not be able to see the prompt if another player 
                                                                              //holding/grabbing a Pickupable or Pushable....
                {
                    Debug.Log("Player 1 and Holding or Grabbing Item. Player 2 should not be able to see prompt");
                    Canvas_Player_1.enabled = false;
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
