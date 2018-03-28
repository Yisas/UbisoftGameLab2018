using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {
    public GameObject rays;
    public static Portal Instance;

	// Use this for initialization
	void Awake () {
        Instance = this;
        rays.SetActive(false);
	}

    public void enableRays()
    {
        // play portal sound      
        rays.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // Do nothing if not local player
            if (!other.GetComponent<UnityEngine.Networking.NetworkIdentity>().isLocalPlayer)
            {
                return;
            }

            // Deactivate camera follow
            int playerID = other.GetComponent<PlayerMove>().PlayerID;

            if (Camera.main.GetComponent<CameraFollow>())
                Camera.main.GetComponent<CameraFollow>().enabled = false;
        }
    }

}
