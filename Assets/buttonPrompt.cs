using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonPrompt : MonoBehaviour {

    [SerializeField]
    Canvas buttonPromptCanvas;


    void Start()
    {
        buttonPromptCanvas.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            TurnOnPrompt();
        }
    }

    private void TurnOnPrompt()
    {
        buttonPromptCanvas.enabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            TurnOffPrompt();
        }
    }

    private void TurnOffPrompt()
    {
        buttonPromptCanvas.enabled = false;
    }
}
