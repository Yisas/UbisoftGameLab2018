using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour {

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

                //AkSoundEngine.PostEvent("LevelComplete", gameObject);
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
            if(other.GetComponent<PlayerMove>().PlayerID == 1)
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
            if (other.GetComponent<PlayerMove>().PlayerID == 1)
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
