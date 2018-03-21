using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ResettableObject : MonoBehaviour
{

    private Vector3 ogPosition;
    private Quaternion ogRotation;

    //Properties
    private bool isMoved;
    private bool isOnPressurePlate;
    private bool isHeld;
    private bool usesGravity = true;
    private bool isTrigger;
    // Distance from the original position for the object to be considered moved
    private const float distanceMovedThreshold = 5.0f;

    // Use this for initialization
    void Start()
    {
        ogPosition = transform.position;
        ogRotation = transform.rotation;
        usesGravity = GetComponent<Rigidbody>().useGravity;

        Collider col = GetComponent<Collider>();
        if (col)
            isTrigger = GetComponent<Collider>().isTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Deadzone"))
        {
            if (gameObject.CompareTag("Player"))
            {
                PlayerObjectInteraction playerInteraction = gameObject.GetComponent<PlayerObjectInteraction>();
                if (playerInteraction != null && playerInteraction.heldObj != null)
                    playerInteraction.DropPickup();
            }
            Reset();
        }
    }

    public void Reset(bool preventRespawnEffect = false)
    {
        if(transform.tag == "Pickup" && !preventRespawnEffect)
        {
            Vector3 positionToSpawnAt = new Vector3(ogPosition.x, ogPosition.y - GetComponent<MeshRenderer>().bounds.extents.y, ogPosition.z);

            GManager.Instance.TriggerRespawnThrowableEffect(positionToSpawnAt);
        }

        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().useGravity = usesGravity;

        Collider col = GetComponent<Collider>();
        if (col)
            col.isTrigger = isTrigger;

        transform.position = ogPosition;
        transform.rotation = ogRotation;
    }

    public bool IsMoved
    {
        get
        {
            if (Vector3.Distance(ogPosition, transform.position) > distanceMovedThreshold)
                isMoved = true;
            else
                isMoved = false;
            return isMoved;
        }
    }

    public bool IsOnPressurePlate
    {
        get { return isOnPressurePlate; }
        set { isOnPressurePlate = value; }
    }

    public bool IsHeld
    {
        get { return isHeld; }
        set { isHeld = value; }
    }

    public Vector3 OriginalPosition
    {
        get { return ogPosition; }
    }


}
