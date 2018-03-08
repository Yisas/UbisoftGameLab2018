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

    public enum ButtonPromptOn { pressureplate, player}
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
        if (buttonprompt == ButtonPromptOn.pressureplate)
        {
            if (player.IsHoldingPickup == true)
            { 
                if(playerID == 1)
                {
                    Canvas_PresurePlate.gameObject.layer = 12;  //"Invisible Player 2 Layer"
                    Canvas_PresurePlate.enabled = true;
                }
                if (playerID == 2)
                {
                    Canvas_PresurePlate.gameObject.layer = 9;   //"Invisible Player 1 Layer"
                    Canvas_PresurePlate.enabled = true;
                }
            }
        }
        else if (buttonprompt == ButtonPromptOn.player)
        {
            if (playerID == 1)
            {
                Canvas_Player_1.enabled = true;
                if (gameObject.name == "PushablePromptTrigger")
                {
                    //Debug.Log("here pushable");
                }
                if (gameObject.name == "PickupPromptTrigger")
                {
                    //Debug.Log("here pickupable");
                }
            }
            if (playerID == 2)
            {
                Canvas_Player_2.enabled = true;
                if (gameObject.name == "PushablePromptTrigger")
                {
                    //Add
                }
                if (gameObject.name == "PickupPromptTrigger")
                {
                    //Add
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
