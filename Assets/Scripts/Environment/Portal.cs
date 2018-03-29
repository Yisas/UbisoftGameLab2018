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
        rays.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        AkSoundEngine.PostEvent("vortex_jump", gameObject);
        if (other.tag == "Player")
        {
            // Do nothing if not local player
            if (!other.GetComponent<UnityEngine.Networking.NetworkIdentity>().isLocalPlayer)
            {
                return;
            }

            if (Camera.main.GetComponent<CameraFollow>())
                Camera.main.GetComponent<CameraFollow>().enabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            // Do nothing if not local player
            if (!other.GetComponent<UnityEngine.Networking.NetworkIdentity>().isLocalPlayer)
            {
                return;
            }

            if (Camera.main.GetComponent<CameraFollow>())
                Camera.main.GetComponent<CameraFollow>().enabled = true;
        }
    }

}
