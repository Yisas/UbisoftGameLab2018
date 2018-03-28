using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;

public class VignettesLoad : NetworkBehaviour
{
   // public Image Vignette;
   // public VideoPlayer Video;

    public float Timer = 30;

    private LoadScene Fade;

    private void Awake()
    {
        StartTimer(); //Lenght of the audio narration
    }
    private void Start()
    {
        Fade = GameObject.FindObjectOfType<LoadScene>();
        //start playing the audio narration?
    }

    private void LAteUpdate()
    {
       
    }

    private void Update()
    {
        if (StartTimer())
        {
            Fade.SetFadeOut(true);
        }
        if (Fade.GetHasFadedOut())
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


    private bool StartTimer()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0)
        {
            return true;
        }
        return false;
    }

}
