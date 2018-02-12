using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour {

    private CameraFollow cameraFollow;
    private bool cameraDeactivated = false;
    private float cameraFollowTime= 0;

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            // Deactivate camera follow
            int playerID = other.GetComponent<PlayerMove>().playerID;

            cameraFollow = GameObject.Find("Player " + playerID + " Camera").GetComponent<CameraFollow>();

            cameraFollow.enabled = false;

            cameraDeactivated = true;

            cameraFollowTime = other.GetComponent<PlayerMove>().cameraDelayTimerBeforeRespawn;
        }
    }

    private void Update()
    {
        if (cameraDeactivated)
        {
            cameraFollowTime -= Time.deltaTime;

            if(cameraFollowTime <= 0)
            {
                cameraDeactivated = false;

                cameraFollow.enabled = true;

                cameraFollow = null;
            }
        }
    }
}
