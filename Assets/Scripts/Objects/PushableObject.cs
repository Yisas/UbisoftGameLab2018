using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour
{
    [Header("Space delta in world space that has to happen to play scraping sound")]
    public float dragNoisePlayThreshold;

    public AudioSource audioSource;

    private float originalMass;
    private bool isBeingPushed = false;
    private Vector3 lastPosition;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalMass = rb.mass;
        rb.mass = 100;
    }

    private void Update()
    {
        if (isBeingPushed)
        {
            if (Mathf.Abs((transform.position - lastPosition).magnitude) >= dragNoisePlayThreshold)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }

            lastPosition = transform.position;
        }
    }

    public void ToggleIsBeingPushed()
    {
        LockUnlockRigidBody(isBeingPushed);
        isBeingPushed = !isBeingPushed;
    }

    public void SetIsBeingPushed(bool beingPushed)
    {
        isBeingPushed = beingPushed;
        LockUnlockRigidBody(!isBeingPushed);
    }

    private void LockUnlockRigidBody(bool locking)
    {
        if (locking)
        {
            //rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.mass = 100;
            audioSource.Stop();
        }
        else
        {
            //rb.constraints = RigidbodyConstraints.None;
            rb.mass = originalMass;
        }
    }
}
