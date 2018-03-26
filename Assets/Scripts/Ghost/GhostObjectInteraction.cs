using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GhostObjectInteraction : NetworkBehaviour
{
    public PlayerObjectInteraction.HoldableType newHeldObj = PlayerObjectInteraction.HoldableType.None;
    private PickupableObject.PickupableType heldObjectType;

    public GameObject dropBox;
    public GameObject grabBox;

    internal GameObject heldObj;


    public float liftHeight;
    public float radiusAboveHead;
    public float weightChange;

    public GameObject[] fakeObjects = new GameObject[3];

    private Vector3 holdPos;
    private RigidbodyInterpolation objectDefInterpolation;
    private FixedJoint joint;
    private float timeOfPickup;
    private Rigidbody heldObjectRb;
    private Movable movableAI;
    private Color originalHeldObjColor;

    void Awake()
    {
        movableAI = GetComponent<Movable>();
    }

    // Update is called once per frame
    void Update()
    {
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
            resettableObject.IsBeingHeld = true;
        }
        heldObjectRb = heldObj.GetComponent<Rigidbody>();

        //reduceHeldObjectVisibility();
    }

    public void GrabObject(Collider other)
    {
        GManager.Instance.NetworkedObjectDestroy(other.gameObject);
        newHeldObj = PlayerObjectInteraction.HoldableType.Pickup;
        ShowFakeObject(other.GetComponent<PickupableObject>().Type);

        //heldObj = other.gameObject;
        //Vector3 grabBoxPosition = grabBox.transform.position;
        //grabBoxPosition.y += liftHeight;
        //heldObj.transform.position = grabBoxPosition;
        //heldObjectRb = heldObj.GetComponent<Rigidbody>();
        //heldObjectRb.velocity = Vector3.zero;
        //objectDefInterpolation = heldObjectRb.interpolation;
        //heldObjectRb.interpolation = RigidbodyInterpolation.Interpolate;
        //AddJoint();

        ////If the object is a pickup set the boolean that its currently being held
        //ResettableObject resettableObject = other.GetComponent<ResettableObject>();
        //if (resettableObject != null && resettableObject.CompareTag("Pickup"))
        //{
        //    resettableObject.IsBeingHeld = true;
        //}

        ////reduceHeldObjectVisibility();
    }

    public void DropPickup()
    {
        // Bring back original transparency of the object
        //heldObj.GetComponent<MeshRenderer>().material.color = originalHeldObjColor;
        // If the object is a pickup set the boolean that its currently being held
        ResettableObject resettableObject = heldObj.GetComponent<ResettableObject>();
        if (resettableObject != null && resettableObject.CompareTag("Pickup"))
        {
            resettableObject.IsBeingHeld = false;

            if (heldObj.tag == "Pickup")
            {
                heldObj.transform.position = dropBox.transform.position;
                PickupableObject pickup = heldObj.GetComponent<PickupableObject>();
                if (pickup)
                {
                    heldObj.GetComponent<Rigidbody>().useGravity = (pickup.Type == PickupableObject.PickupableType.Torch) ? false : true;
                    heldObj.GetComponent<Collider>().isTrigger = (pickup.Type == PickupableObject.PickupableType.Torch) ? true : false;
                    heldObj.GetComponent<Rigidbody>().isKinematic = false;
                }
            }
        }
        else
        {
            Debug.LogWarning("Object " + heldObj.name + " dropped by ghost was missing ResettableObject script. Was that intentional?");
            heldObj.transform.position = dropBox.transform.position;
        }

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
            AkSoundEngine.PostEvent("GhostLaugh", gameObject);
            joint = heldObj.AddComponent<FixedJoint>();
            joint.connectedBody = GetComponent<Rigidbody>();
            heldObj.layer = gameObject.layer;
        }
    }

    private void matchVelocities()
    {
        if (heldObjectRb != null)
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


    /// <summary>
    /// Side effect: will set heldObjectType to type
    /// </summary>
    /// <param name="type"></param>
    private void ShowFakeObject(PickupableObject.PickupableType type)
    {
        CommonShowFakeObject(type);

        if (isServer)
            RpcShowFakeObject(type);
        else
            CmdShowFakeObject(type);
    }

    private void CommonShowFakeObject(PickupableObject.PickupableType type)
    {
        fakeObjects[(int)type].SetActive(true);
        heldObjectType = type;
    }

    [Command]
    private void CmdShowFakeObject(PickupableObject.PickupableType type)
    {
        CommonShowFakeObject(type);
    }

    [ClientRpc]
    private void RpcShowFakeObject(PickupableObject.PickupableType type)
    {
        if (!isLocalPlayer)
            CommonShowFakeObject(type);
    }
}
