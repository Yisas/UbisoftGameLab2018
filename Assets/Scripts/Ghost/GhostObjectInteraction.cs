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

    public void GrabObject(Collider other)
    {
        AkSoundEngine.PostEvent("ghost_laugh", gameObject);
        GManager.Instance.NetworkedObjectDestroy(other.gameObject);
        newHeldObj = PlayerObjectInteraction.HoldableType.Pickup;
        ShowFakeObject(other.GetComponent<PickupableObject>().Type);
    }

    public void DropPickup()
    {
        HideFakeObject();
        newHeldObj = PlayerObjectInteraction.HoldableType.None;

        GameObject throwableToSpawn = null;
        throwableToSpawn = GManager.Instance.GetCachedObject(heldObjectType);
        Transform positionToSpawnAt = fakeObjects[(int)heldObjectType].transform;

        throwableToSpawn.transform.position = positionToSpawnAt.position;
        throwableToSpawn.transform.rotation = positionToSpawnAt.rotation;

        if (heldObjectType != PickupableObject.PickupableType.Torch)
        {
            throwableToSpawn.GetComponent<Rigidbody>().useGravity = true;
            throwableToSpawn.GetComponent<Rigidbody>().isKinematic = false;
            throwableToSpawn.GetComponent<Collider>().isTrigger = false;
        }
        else
        {
            AkSoundEngine.PostEvent("torch_place", gameObject);
            throwableToSpawn.GetComponent<Rigidbody>().useGravity = false;
            throwableToSpawn.GetComponent<Collider>().isTrigger = true;
        }

        if (isServer)
            GManager.Instance.CachedObjectWasUsed(heldObjectType, true);
    }

    //connect player and pickup/pushable object via a physics joint
    private void AddJoint()
    {
        if (heldObj)
        {
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

    public void HideFakeObject()
    {
        CommonHideFakeObject();

        if (isServer)
            RpcHideFakeObject();
        else
            CmdHideFakeObject();
    }

    private void CommonHideFakeObject()
    {
        fakeObjects[(int)heldObjectType].SetActive(false);
        newHeldObj = PlayerObjectInteraction.HoldableType.None;
    }

    [Command]
    private void CmdHideFakeObject()
    {
        CommonHideFakeObject();
    }

    [ClientRpc]
    private void RpcHideFakeObject()
    {
        if (!isLocalPlayer)
            CommonHideFakeObject();
    }
}
