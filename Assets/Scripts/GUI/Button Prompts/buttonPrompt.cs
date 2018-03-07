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

    public enum ButtonPrompt { pressureplate, player}
    public ButtonPrompt buttonprompt;

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
            playerID = other.gameObject.GetComponent<PlayerMove>().PlayerID;
            TurnOnPrompt();            
        }
    }

    private void TurnOnPrompt()
    {
        if(buttonprompt == ButtonPrompt.pressureplate) 
        {
            Canvas_PresurePlate.enabled = true;
        }
        else if(buttonprompt == ButtonPrompt.player)
        {          
            if (playerID == 1)
            {                 
                Canvas_Player_1.enabled = true;
                if (gameObject.name == "PushablePromptTrigger")
                {
                    Debug.Log("here pushable");
                }
                if (gameObject.name == "PickupPromptTrigger")
                {
                    Debug.Log("here pickupable");
                }
            }
            if(playerID == 2)
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

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") 
        {
            playerID = other.gameObject.GetComponent<PlayerMove>().PlayerID;
            TurnOffPrompt(); 
        }
    }

    private void TurnOffPrompt()
    {
        if (buttonprompt == ButtonPrompt.pressureplate)
        {
            Canvas_PresurePlate.enabled = false;
        }
        else if (buttonprompt == ButtonPrompt.player)
        {         
            if (playerID == 1)
                Canvas_Player_1.enabled = false;
            if (playerID == 2)
                Canvas_Player_2.enabled = false;
        }
    }
}
