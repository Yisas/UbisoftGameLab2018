using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObject : MonoBehaviour {

    public AudioSource audioSource;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer != LayerMask.NameToLayer("Player 1") && collision.gameObject.layer != LayerMask.NameToLayer("Player 2"))
        {
            if(!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
