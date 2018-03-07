using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostObjectInteraction : MonoBehaviour {

    public AudioClip pickUpSound;
    public GameObject dropBox;
    public GameObject grabBox;

    internal GameObject heldObj;


    private const float liftHeight = 0.5f;
    private const float radiusAboveHead= 0.5f;
    public const float weightChange = 0.3f;

    private Vector3 holdPos;
    private RigidbodyInterpolation objectDefInterpolation;
    private FixedJoint joint;
    private AudioSource audioSource;
    private float timeOfPickup;
    private Rigidbody heldObjectRb;
    private Movable movableAI;
    private Color originalHeldObjColor;

    void Awake () {
        audioSource = GetComponent<AudioSource>();
        movableAI = GetComponent<Movable>();
    }
	
	// Update is called once per frame
	void Update () {
        matchVelocities();
	}

    public void LiftPickup(Collider other)
    {
        //get where to move item once its picked up
        Mesh otherMesh = other.GetComponent<MeshFilter>().mesh;
        holdPos = transform.position;
        holdPos.y += (GetComponent<Collider>().bounds.extents.y) + (otherMesh.bounds.extents.y) + liftHeight;

        if (!Physics.CheckSphere(holdPos, radiusAboveHead, 2))
        {
            heldObj = other.gameObject;
            heldObjectRb = heldObj.GetComponent<Rigidbody>();
            objectDefInterpolation = heldObjectRb.interpolation;
            heldObjectRb.interpolation = RigidbodyInterpolation.Interpolate;
            heldObj.transform.position = holdPos;
            heldObj.transform.rotation = transform.rotation;
            AddJoint();
            //here we adjust the mass of the object, so it can seem heavy, but not effect player movement whilst were holding it
            heldObjectRb.mass *= weightChange;
            //make sure we don't immediately throw object after picking it up
            timeOfPickup = Time.time;
        }

        // If the object is a pickup set the boolean that its currently being held
        ResettableObject resettableObject = other.GetComponent<ResettableObject>();
        if (resettableObject != null && resettableObject.CompareTag("Pickup"))
        {
            resettableObject.IsHeld = true;
        }
        heldObjectRb = heldObj.GetComponent<Rigidbody>();

        //reduceHeldObjectVisibility();
    }

    public void GrabObject(Collider other)
    {
        heldObj = other.gameObject;
        heldObj.transform.position = grabBox.transform.position;
        heldObjectRb = heldObj.GetComponent<Rigidbody>();
        heldObjectRb.velocity = Vector3.zero;
        objectDefInterpolation = heldObjectRb.interpolation;
        heldObjectRb.interpolation = RigidbodyInterpolation.Interpolate;
        AddJoint();
        
        //If the object is a pickup set the boolean that its currently being held
        ResettableObject resettableObject = other.GetComponent<ResettableObject>();
        if (resettableObject != null && resettableObject.CompareTag("Pickup"))
        {
            resettableObject.IsHeld = true;
        }

        //reduceHeldObjectVisibility();
    }

    public void DropPickup()
    {
        // Bring back original transparency of the object
        heldObj.GetComponent<MeshRenderer>().material.color = originalHeldObjColor;

        // If the object is a pickup set the boolean that its currently being held
        ResettableObject resettableObject = heldObj.GetComponent<ResettableObject>();
        if (resettableObject != null && resettableObject.CompareTag("Pickup"))
        {
            resettableObject.IsHeld = false;
        }

        if (heldObj.tag == "Pickup")
        {
            heldObj.transform.position = dropBox.transform.position;
        }

        heldObjectRb.interpolation = objectDefInterpolation;
        Destroy(joint);


        heldObj.layer = 0;
        heldObj = null;
        heldObjectRb.velocity = Vector3.zero;
        heldObjectRb = null;
    }

    //connect player and pickup/pushable object via a physics joint
    private void AddJoint()
    {
        if (heldObj)
        {
            if (pickUpSound)
            {
                // TODO: undo hardcoded volume, multiple get etc.
                audioSource.volume = 1;
                audioSource.clip = pickUpSound;
                audioSource.Play();
            }
            joint = heldObj.AddComponent<FixedJoint>();
            joint.connectedBody = GetComponent<Rigidbody>();
            heldObj.layer = gameObject.layer;
        }
    }

    private void matchVelocities()
    {
        if(heldObjectRb != null)
        {
            heldObjectRb.velocity = movableAI.velocity;
        }
    }

    private void reduceHeldObjectVisibility()
    {
        // Reduce transparency of the object
        Renderer heldObjRenderer = heldObj.GetComponent<Renderer>();
        originalHeldObjColor = heldObjRenderer.material.color;
        Color fadedColor = new Color(originalHeldObjColor.r, originalHeldObjColor.g, originalHeldObjColor.b, 0.1f);
        heldObjRenderer.material.color = fadedColor;
    }

}
