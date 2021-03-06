﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;

public class SkipVignette : /*NetworkBehaviour*/ MonoBehaviour
{
    int counter = 1;
    public bool allowedToSkip = true;

    public void Start()
    {
        XInputDotNetPure.GamePad.SetVibration(XInputDotNetPure.PlayerIndex.One, 0, 0);
    }
    
    private void Update()
    {
        if (Input.GetButtonDown("Skip Vignette"))
        {
            if (allowedToSkip)
            {
                allowedToSkip = false;
                Skip();
            }
            else
                return;
            //if (isServer)
            //{
            //    Debug.Log("Skipping Vignette");
            //    Skip();
            //}
            //else
            //{
            //    cmdSkip();
            //}
        }
    }

    void Skip()
    {
        counter--;
        if (counter == 0)
        {
            GameObject menu = GameObject.FindGameObjectWithTag("MenuUI");
            if (menu != null)
            {
                menu.GetComponent<StartOptions>().NextScene();

                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }

                this.enabled = false;
            }
        }
    }

    //[Command]
    //void cmdSkip()
    //{
    //    Debug.Log("Client!!!!!!!!!! SKIP VIGNETTE");
    //    Skip();
    //}
}
