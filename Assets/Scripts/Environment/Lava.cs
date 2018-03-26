using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    public GameObject lavaSinkParticles;
    public float lavaParticleHeight;
    private CameraFollow cameraFollow;
    private bool cameraDeactivated = false;
    private float cameraFollowTime = 0;
    private GameObject menu;

    private void Start()
    {
        menu = GameObject.FindGameObjectWithTag("MenuUI");
    }

    private void OnTriggerStay(Collider other)
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

            cameraFollow = Camera.main.GetComponent<CameraFollow>();
            cameraFollow.enabled = false;
            cameraDeactivated = true;
            cameraFollowTime = other.GetComponent<PlayerMove>().cameraDelayTimerBeforeRespawn;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // Start particle effect when player sinks into lava
            Vector3 particlePosition = other.transform.position;
            particlePosition.y = transform.position.y + lavaParticleHeight;
            Instantiate(lavaSinkParticles, particlePosition, transform.rotation);

            // Fade out camera
            if (menu != null)
            {
                // Player can't move while camera is black
                PlayerMove player = other.GetComponent<PlayerMove>();
                menu.GetComponent<StartOptions>().FadeOutThenIn(player);
            }
        }
    }

    private void Update()
    {
        if (cameraDeactivated)
        {
            cameraFollowTime -= Time.deltaTime;

            if (cameraFollowTime <= 0)
            {
                cameraDeactivated = false;
                cameraFollow.enabled = true;
                cameraFollow = null;
            }
        }
    }
}