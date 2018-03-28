using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour
{
    [Header("Space delta in world space that has to happen to play scraping sound")]
    public float dragNoisePlayThreshold;

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
            // TODO if heldObj type is pushable and character motor direction is > 1 but tweek the value
            //if (Mathf.Abs((transform.position - lastPosition).magnitude) >= dragNoisePlayThreshold)
            //{
                AkSoundEngine.PostEvent("slide_start", gameObject);
            //}

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
            AkSoundEngine.PostEvent("slide_stop", gameObject);
        }
        else
        {
            //rb.constraints = RigidbodyConstraints.None;
            rb.mass = originalMass;
        }
    }

}
