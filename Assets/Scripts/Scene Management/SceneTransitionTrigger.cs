using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour {

    public AudioSource audioSource;
    private bool player1In = false;
    private bool player2In = false;

    private void Update()
    {
        if(player1In && player2In)
        {
            GameObject menu = GameObject.FindGameObjectWithTag("MenuUI");
            if(menu != null)
            {
                menu.GetComponent<StartOptions>().NextScene();

                audioSource.Play();
                foreach(Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }

                this.enabled = false;
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(other.GetComponent<PlayerMove>().playerID == 1)
            {
                player1In = true;
            }
            else
            {
                player2In = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.GetComponent<PlayerMove>().playerID == 1)
            {
                player1In = false;
            }
            else
            {
                player2In = false;
            }
        }
    }
}
