﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;

public class SkipVignette : /*NetworkBehaviour*/ MonoBehaviour
{
    private void Update()
    {
        if (Input.GetButtonDown("Skip Vignette"))
        {
            Skip();
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

    //[Command]
    //void cmdSkip()
    //{
    //    Debug.Log("Client!!!!!!!!!! SKIP VIGNETTE");
    //    Skip();
    //}
}
