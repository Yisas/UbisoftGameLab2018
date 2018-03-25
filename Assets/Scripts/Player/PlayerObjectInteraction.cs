using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

//this allows the player to pick up/throw, and also pull certain objects
//you need to add the tags "Pickup" or "Pushable" to these objects
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMove))]
public class PlayerObjectInteraction : NetworkBehaviour
{
    public enum HoldableType { Pickup, Player, Pushable, None }
    public HoldableType newHeldObj = HoldableType.None;

    public GameObject[] fakeObjects = new GameObject[5];
    private PickupableObject.PickupableType heldObjectType;

    public GameObject holdPlayerPos;
    public Transform airbornePickupDropPosition;
    public GameObject particlesObjectAppear;

    public GameObject grabBox;                                  //objects inside this trigger box can be picked up by the player (think of this as your reach)
    public GameObject dropBox;                                  //positions where the player's objects will begin dropping
    public float gap = 0.5f;                                    //how high above player to hold objects
    public Vector3 throwForce = new Vector3(0, 5, 7);           //the throw force of the player on the ojects
    public Vector3 throwForcePlayer = new Vector3(0, 10, 20);   //Added: the throw force of the player on the player
    [Tooltip("Amount of time it takes before the player can use the 'throw' button again")]
    private float throwThrowableCooldownTime = 0.1f;
    public float throwPlayerCooldownTime = 0.1f;
    public float rotateToBlockSpeed = 3;                        //how fast to face the "Pushable" object you're holding/pulling
    public float checkRadius = 0.5f;                            //how big a radius to check above the players head, to see if anything is in the way of your pickup
    [Range(0.1f, 1f)]                                           //new weight of a carried object, 1 means no change, 0.1 means 10% of its original weight													
    public float weightChange = 0.3f;                           //this is so you can easily carry objects without effecting movement if you wish to
    private bool addChangeMass;
    private bool subChangeMass;
    [Range(10f, 1000f)]
    public float holdingBreakForce = 45f, holdingBreakTorque = 45f;//force and angularForce needed to break your grip on a "Pushable" object youre holding onto
    public Animator animator;                                   //object with animation controller on, which you want to animate (usually same as in PlayerMove)
    public int armsAnimationLayer;                              //index of the animation layer for "arms"
    public float boxHangThreshold;                              // The value the player's y velocity must be bound between before he drops the box which is keeping him attached to a ledge.

    [HideInInspector]
    public GameObject heldObj;
    private Vector3 holdPos;
    private FixedJoint joint;
    private Color gizmoColor;
    private PlayerMove otherPlayer = null;

    // State attributes
    private float timeOfPickup, timeOfThrow, defRotateSpeed;

    private float originalMass;

    // Private references
    private PlayerMove playerMove;
    private CharacterMotor characterMotor;
    private AudioSource audioSource;
    private TriggerParent triggerParent;
    private RigidbodyInterpolation objectDefInterpolation;
    private Rigidbody rb;
    private NetworkIdentity networkIdentity;
    public float bumpVibrationDuration = 0.5f;
    public float invalidDropVibrationDuration = 0.2f;
    private float vibrationTime = 0;
    public float vibrationIntensity = 0.1f;

    //setup
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        networkIdentity = GetComponent<NetworkIdentity>();

        //create grabBox is none has been assigned
        if (!grabBox)
        {
            grabBox = new GameObject();
            grabBox.AddComponent<BoxCollider>();
            grabBox.GetComponent<Collider>().isTrigger = true;
            grabBox.transform.parent = transform;
            grabBox.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            Debug.LogWarning("No grabBox object assigned to 'Throwing' script, one has been created and assigned for you", grabBox);
        }

        playerMove = GetComponent<PlayerMove>();
        characterMotor = GetComponent<CharacterMotor>();
        audioSource = GetComponent<AudioSource>();
        defRotateSpeed = playerMove.rotateSpeed;
        //set arms animation layer to animate with 1 weight (full override)
        if (animator)
            animator.SetLayerWeight(armsAnimationLayer, 1);

        originalMass = GetComponent<Rigidbody>().mass;

    }

    #region Updates
    void LateUpdate()
    {
        if (heldObj != null)
        {
            //if (addChangeMass)
            //{
            //    heldObj.GetComponent<Rigidbody>().mass *= weightChange;
            //    addChangeMass = false;
            //}
            //if (subChangeMass)
            //{
            //    //heldObj.GetComponent<Rigidbody>().mass /= weightChange;0
            //    heldObj.GetComponent<Rigidbody>().mass = originalMass;
            //    heldObj = null;
            //    subChangeMass = false;
            //}
        }
    }

    //throwing/dropping
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Defensive programing: if the carried player ever ends up inside the carrying player due to networking issues, reset
        // the position to where it should be above the carrying players head
        if (playerMove.IsBeingHeld)
        {
            Collider col = GetComponent<Collider>();
            FindOtherPlayer();

            if (col.bounds.Intersects(otherPlayer.GetComponent<Collider>().bounds))
            {
                otherPlayer.GetComponent<PlayerObjectInteraction>().ResetHoldPosition();
            }
        }

        //when we press grab button, throw object if we're holding one
        if (Input.GetButtonDown("Grab") && newHeldObj != HoldableType.None)
        {
            if (newHeldObj == HoldableType.Pickup && Time.time > timeOfPickup + throwThrowableCooldownTime)
            {
                Debug.Log("Throwing pickup from player " + playerMove.PlayerID + ", isServer? " + isServer);
                ThrowPickup();
            }
            else if (newHeldObj == HoldableType.Player && Time.time > timeOfPickup + throwPlayerCooldownTime)
            {
                ThrowPlayer();
            }
            else if (newHeldObj == HoldableType.Pushable && Time.time > timeOfPickup + throwPlayerCooldownTime)
                LetGoOfPushable();
        }

        //set animation value for arms layer
        if (animator)
        {
            if (newHeldObj != HoldableType.None)
            {
                // --------- Holding animations ---------
                if (newHeldObj == HoldableType.Pickup)
                {
                    animator.SetBool("HoldingPickup", true);
                    if (isServer)
                        RpcUpdateClientAnimator("HoldingPickup", true);
                    else
                        CmdUpdateClientAnimator("HoldingPickup", true);
                }
                else if (newHeldObj == HoldableType.Player)
                //**TODO NOTE: Add Animation for picking up the player. 
                {
                    animator.SetBool("HoldingPickup", true);
                    if (isServer)
                        RpcUpdateClientAnimator("HoldingPickup", true);
                    else
                        CmdUpdateClientAnimator("HoldingPickup", true);
                }
                else
                {
                    animator.SetBool("HoldingPickup", false);
                    if (isServer)
                        RpcUpdateClientAnimator("HoldingPickup", false);
                    else
                        CmdUpdateClientAnimator("HoldingPickup", false);
                }

                // --------- Pushing animations ---------
                if (newHeldObj == HoldableType.Pushable)
                {
                    animator.SetBool("HoldingPushable", true);
                    if (isServer)
                        RpcUpdateClientAnimator("HoldingPushable", true);
                    else
                        CmdUpdateClientAnimator("HoldingPushable", true);
                }
                else
                {
                    animator.SetBool("HoldingPushable", false);
                    if (isServer)
                        RpcUpdateClientAnimator("HoldingPushable", false);
                    else
                        CmdUpdateClientAnimator("HoldingPushable", false);
                }
            }
            else
            {
                animator.SetBool("HoldingPickup", false);
                animator.SetBool("HoldingPushable", false);
                if (isServer)
                {
                    RpcUpdateClientAnimator("HoldingPickup", false);
                    RpcUpdateClientAnimator("HoldingPushable", false);
                }
                else
                {
                    CmdUpdateClientAnimator("HoldingPickup", false);
                    CmdUpdateClientAnimator("HoldingPushable", false);
                }
            }
        }
        //when grab is released, let go of any pushable objects were holding
        if (Input.GetButtonDown("Drop") && newHeldObj != HoldableType.None)
        {
            if (newHeldObj == HoldableType.Player)
            {
                DropPlayer();
            }
            else if (newHeldObj == HoldableType.Pushable)
            {
                LetGoOfPushable();
            }
            else
            {
                if (!Physics.CheckSphere(dropBox.transform.position, checkRadius, LayerMask.NameToLayer("Player 2")))
                    DropPickup();
                else
                {
                    if (isLocalPlayer)
                    {
                        Debug.Log("Trying to drop inside something");
                        gizmoColor = Color.red;
                        XInputDotNetPure.GamePad.SetVibration(playerMove.PlayerID == 1 ? XInputDotNetPure.PlayerIndex.One : XInputDotNetPure.PlayerIndex.Two, vibrationIntensity, vibrationIntensity);
                        vibrationTime = invalidDropVibrationDuration;
                    }
                }
            }
        }

        checkIfBoxIsHanging();

        if (isLocalPlayer)
        {
            if (vibrationTime > 0)
            {
                vibrationTime -= Time.deltaTime;

                if (vibrationTime <= 0)
                {
                    XInputDotNetPure.GamePad.SetVibration(playerMove.PlayerID == 1 ? XInputDotNetPure.PlayerIndex.One : XInputDotNetPure.PlayerIndex.Two, 0f, 0f);
                }
            }
        }
    }
    #endregion

    #region Collision
    void OnTriggerEnter(Collider other)
    {
        if (isLocalPlayer)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player 1") && other.gameObject.layer != LayerMask.NameToLayer("Player 2")
                && other.gameObject.layer != 2 /*ignore raycast*/ && other.bounds.max.y > gameObject.GetComponent<Collider>().bounds.max.y)
            {
                XInputDotNetPure.GamePad.SetVibration(playerMove.PlayerID == 1 ? XInputDotNetPure.PlayerIndex.One : XInputDotNetPure.PlayerIndex.Two, vibrationIntensity, vibrationIntensity);
                vibrationTime = bumpVibrationDuration;
            }
        }

        if (other.tag == "Pushable" || LayerMask.LayerToName(other.gameObject.layer).Contains("Invisible") || LayerMask.LayerToName(other.gameObject.layer).Contains("Appearing"))
        {
            AppearingObject ao = other.GetComponent<AppearingObject>();

            if (ao && other.gameObject.layer != LayerMask.NameToLayer("Default"))
            {
                if (ao.playerToAppearTo == playerMove.PlayerID)
                {
                    AkSoundEngine.PostEvent("AppearObject", gameObject);
                }
            }
            else if (other.gameObject.layer != LayerMask.NameToLayer("Player 1") && other.gameObject.layer != LayerMask.NameToLayer("Player 2"))
            {
                AkSoundEngine.PostEvent("BoxCollide", gameObject);
            }
        }


        if (other.tag == "Pickup")
        {
            if (isLocalPlayer)
            {
                NetworkIdentity pickupableNetID = other.GetComponent<NetworkIdentity>();

                if (!pickupableNetID.hasAuthority)
                    if (isServer)
                        GManager.Instance.SetPlayerAuthorityToHeldObject(networkIdentity, other.GetComponent<NetworkIdentity>());
                    else
                        CmdSetPlayerAuthorityToHeldObject(networkIdentity, other.GetComponent<NetworkIdentity>());
            }
        }
    }

    [Command]
    private void CmdSetPlayerAuthorityToHeldObject(NetworkIdentity clientToReceiveAuthority, NetworkIdentity netIdentityOfObj)
    {
        GManager.Instance.SetPlayerAuthorityToHeldObject(clientToReceiveAuthority, netIdentityOfObj);
    }

    //pickup/grab
    void OnTriggerStay(Collider other)
    {
        // non-local players shouldn't try to grab. Local version will call appropriate functions when grabbing
        if (!isLocalPlayer)
            return;

        //if grab is pressed and an object is inside the players "grabBox" trigger
        if (Input.GetButton("Grab"))
        {
            // Save computational time by not attempting to interact with non-valid objects
            if (other.tag != "Pickup" && other.tag != "Pushable" && other.tag != "Player")
                return;

            ResettableObject resettableObject = other.GetComponent<ResettableObject>();

            // Give the object either host or client authority, depending on which player is picking it up
            if (other.tag != "Player")
            {
                //pickup
                if (other.tag == "Pickup" && other.GetComponent<PickupableObject>() && newHeldObj == HoldableType.None && timeOfThrow + 0.2f < Time.time)
                {
                    Debug.Log("Lifting pickup from player " + playerMove.PlayerID + ", isServer? " + isServer);
                    LiftPickup(other.transform, other.GetComponent<PickupableObject>().Type);
                }
                //grab
                else if (other.tag == "Pushable" && (other.gameObject.layer != LayerMask.NameToLayer(("Invisible Player " + playerMove.PlayerID))) && newHeldObj == HoldableType.None && timeOfThrow + 0.2f < Time.time)
                {
                    // Never grab off another player's hands
                    if (playerMove.FullyGrounded && playerMove.lastFeetTouched != other.transform && !resettableObject.IsBeingHeld)
                    {
                        Vector3 heading = other.transform.position - playerMove.transform.position;
                        float dot = Vector3.Dot(heading, playerMove.transform.forward);

                        if (dot > 0)
                            GrabPushable(other);
                    }
                }

                return;
            }
            if (other.tag == "Player" && newHeldObj == HoldableType.None && timeOfThrow + 0.2f < Time.time)
            {
                PickupPlayer(other);
                return;
            }
        }
    }
    #endregion

    /// <summary>
    /// Places the carried player above this players head. Used in networking transform corrections
    /// </summary>
    public void ResetHoldPosition()
    {
        if (!heldObj)
        {
            Debug.LogWarning("Held object was missing when resetting. Did this break something in the game?");
            return;
        }

        holdPos = transform.position;
        holdPos.y += (GetComponent<Collider>().bounds.extents.y) + (heldObj.GetComponent<Collider>().bounds.extents.y) + gap;
        heldObj.transform.position = holdPos;
        heldObj.transform.rotation = transform.rotation;
    }

    #region Interactions
    private void GrabPushable(Collider other)
    {
        Debug.Log("Grabbing pushable from player " + playerMove.PlayerID + " islocal? " + isLocalPlayer + " isServer?" + isServer);

        // Avoid any physics while in the process of grabbing
        other.GetComponent<Collider>().isTrigger = true;

        Vector3 touchedPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        if (touchedPoint.y > transform.position.y) return;

        playerMove.transform.LookAt(touchedPoint);

        fakeObjects[(int)PickupableObject.PickupableType.BigBox].transform.position = other.transform.position;
        fakeObjects[(int)PickupableObject.PickupableType.BigBox].transform.rotation = other.transform.rotation;

        // also modify client side
        if (!isServer)
        {
            CmdChangeFakePushableBoxOrientation(fakeObjects[(int)PickupableObject.PickupableType.BigBox].transform.localPosition,
                fakeObjects[(int)PickupableObject.PickupableType.BigBox].transform.localRotation);
        }

        playerMove.IsGrabingPushable = true;
        playerMove.rotateSpeed = 0;

        // Only destroy objects on the server
        if (isServer)
        {
            NetworkServer.Destroy(other.gameObject);
        }
        else
        {
            CmdServerDestroy(other.gameObject);
        }

        timeOfPickup = Time.time;
        newHeldObj = HoldableType.Pushable;
        ShowFakeObject(PickupableObject.PickupableType.BigBox);
    }

    [Command]
    private void CmdChangeFakePushableBoxOrientation(Vector3 position, Quaternion rotation)
    {
        fakeObjects[(int)PickupableObject.PickupableType.BigBox].transform.localPosition = position;
        fakeObjects[(int)PickupableObject.PickupableType.BigBox].transform.localRotation = rotation;
    }

    private void PickupPlayer(Collider other)
    {
        Debug.Log("Player " + playerMove.PlayerID + " is picking up the other. islocal? " + isLocalPlayer + " isServer?" + isServer);

        if (isLocalPlayer)
        {
            //if there is space above our head, pick up item (layermask index 2: "Ignore Raycast", anything on this layer will be ignored)
            if (!Physics.CheckSphere(fakeObjects[(int)PickupableObject.PickupableType.Player].transform.position, checkRadius, 2))
            {
                CommonPickupPlayer();
            }

            // Networking logic: this function now needs to be executed by the opposite version of this player instance
            if (isServer)
                RpcPickupPlayer();
            else
                CmdPickupPlayer();
        }
    }

    [ClientRpc]
    private void RpcPickupPlayer()
    {
        if (!isLocalPlayer)
            CommonPickupPlayer();
    }

    [Command]
    private void CmdPickupPlayer()
    {
        CommonPickupPlayer();
    }

    private void CommonPickupPlayer()
    {
        ShowFakeObject(PickupableObject.PickupableType.Player);
        newHeldObj = HoldableType.Player;

        //heldObj.GetComponent<PlayerMove>().LockMovementToOtherPlayer(holdPlayerPos.transform);

        //playerMove.CanJump = false;   //Bottom player cannot jump

        timeOfPickup = Time.time;

        OverrideOtherPlayer();
    }

    /// <summary>
    /// The other player is now faked in this screen, so pretend the other game is this instance's fake player
    /// </summary>
    private void OverrideOtherPlayer()
    {
        CommonOverrideOtherPlayer();
    }

    private void CommonOverrideOtherPlayer()
    {
        int otherPlayerID = playerMove.PlayerID == 1 ? 2 : 1;
        GManager.Instance.OverridePlayer(otherPlayerID);

        // Local player needs a new camera target, so if you're not local player but carrying, you're overriding camera
        if (!isLocalPlayer)
        {
            GManager.Instance.OverrideCameraFollow(playerMove.PlayerID);
        }
    }

    [Command]
    private void CmdOverrideOtherPlayer()
    {
        CommonOverrideOtherPlayer();
    }

    [ClientRpc]
    private void RpcOverrideOtherPlayer()
    {
        if (!isLocalPlayer)
            CommonOverrideOtherPlayer();
    }

    private void LiftPickup(Transform other, PickupableObject.PickupableType type)
    {
        if (!Physics.CheckSphere(other.position, checkRadius, LayerMask.NameToLayer("Ignore Raycast")))
        {
            // Only destroy objects on the server
            if (isServer)
            {
                NetworkServer.Destroy(other.gameObject);
            }
            else
            {
                CmdServerDestroy(other.gameObject);
            }

            CommonLiftPickup(type);

            // Local player only
            timeOfPickup = Time.time;

            if (isLocalPlayer)
            {
                if (isServer)
                {
                    RpcLiftPickup(type);
                }
                else
                {
                    CmdLiftPickup(type);
                }
            }

        }
        else
        {
            // TODO: handle not being able to pickup if necessary
            Debug.LogWarning("The player tried to lift something over it's head and something was in the way. Did it look/feel super bad?");
        }
    }

    [Command]
    private void CmdLiftPickup(PickupableObject.PickupableType type)
    {
        CommonLiftPickup(type);
    }

    [ClientRpc]
    private void RpcLiftPickup(PickupableObject.PickupableType type)
    {
        CommonLiftPickup(type);
    }

    private void CommonLiftPickup(PickupableObject.PickupableType type)
    {
        newHeldObj = HoldableType.Pickup;
        ShowFakeObject(type);
    }

    [Command]
    private void CmdServerDestroy(GameObject gameObjectToDestroy)
    {
        NetworkServer.Destroy(gameObjectToDestroy);
    }

    /// <summary>
    /// Side effect: will set heldObjectType to type
    /// </summary>
    /// <param name="type"></param>
    private void ShowFakeObject(PickupableObject.PickupableType type)
    {
        CommonShowFakeObject(type);

        if (isLocalPlayer)
        {
            if (isServer)
                RpcShowFakeObject(type);
            else
                CmdShowFakeObject(type);
        }
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

        if (isLocalPlayer)
        {
            if (isServer)
                RpcHideFakeObject();
            else
                CmdHideFakeObject();
        }
    }

    private void CommonHideFakeObject()
    {
        fakeObjects[(int)heldObjectType].SetActive(false);
        newHeldObj = HoldableType.None;
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

    [Command]
    private void CmdSetInteractableIsBeingHeld(bool value, Vector3 positionOfHeldObject, string type)
    {
        FindLiftedPickupInNonLocalInstance(positionOfHeldObject, type);
        SetInteractableIsBeingHeld(value, type);
    }

    private void SetInteractableIsBeingHeld(bool value, string type)
    {
        // If the object is a pickup set the boolean that its currently being held
        ResettableObject resettableObject = heldObj.GetComponent<ResettableObject>();
        if (resettableObject != null && heldObj.CompareTag(type))
        {
            resettableObject.IsBeingHeld = value;
        }
    }

    /// <summary>
    /// The non-local game instance does not lift the pickup per-se, so we gotta find it at runtime for other purposes
    /// </summary>
    private void FindLiftedPickupInNonLocalInstance(Vector3 positionOfHeldObject, string type)
    {
        Collider[] colliders;
        if ((colliders = Physics.OverlapSphere(positionOfHeldObject, 1f)).Length > 1)
        {
            foreach (Collider collider in colliders)
            {
                GameObject go = collider.gameObject; //This is the game object you collided with
                if (go.tag == type)
                {
                    heldObj = go;
                    return;
                }
            }
        }
    }

    public void LetGoOfPushable()
    {
        Debug.Log("Letting go pushable from player " + playerMove.PlayerID + " islocal? " + isLocalPlayer + " isServer?" + isServer);

        CommonLetGoOfPushable();

        if (isLocalPlayer)
        {
            timeOfThrow = Time.time;

            if (isServer)
            {
                RpcLetGoOfPushable();
            }
            else
            {
                CmdLetGoOfPushable();
            }
        }
    }

    private void CommonLetGoOfPushable()
    {
        HideFakeObject();

        GameObject pushableToSpawn = null;
        if (isLocalPlayer)
        {
            if (heldObjectType == PickupableObject.PickupableType.BigBox)
            {
                pushableToSpawn = GManager.Instance.GetCachedObject(heldObjectType);
                pushableToSpawn.transform.position = fakeObjects[(int)PickupableObject.PickupableType.BigBox].transform.position;
                pushableToSpawn.transform.rotation = fakeObjects[(int)PickupableObject.PickupableType.BigBox].transform.rotation;
                pushableToSpawn.GetComponent<Rigidbody>().useGravity = true;
                pushableToSpawn.GetComponent<Rigidbody>().isKinematic = false;
                pushableToSpawn.GetComponent<Collider>().isTrigger = false;
            }
            else
            {
                Debug.LogWarning("Letting go of pushable you don't have?");
            }

        }

        if (isServer)
            GManager.Instance.CachedObjectWasUsed(heldObjectType, playerMove.PlayerID == 1);

        playerMove.IsGrabingPushable = false;
        playerMove.rotateSpeed = defRotateSpeed;
    }

    [ClientRpc]
    private void RpcLetGoOfPushable()
    {
        if (!isLocalPlayer)
            CommonLetGoOfPushable();
    }

    [Command]
    private void CmdLetGoOfPushable()
    {
        CommonLetGoOfPushable();
    }

    public void DropPickup()
    {
        CommonDropPickup();

        if (isLocalPlayer)
        {
            if (isServer)
                RpcDropPickup();
            else
                CmdDropPickup();
        }
    }

    [ClientRpc]
    private void RpcDropPickup()
    {
        if (!isLocalPlayer)
            CommonDropPickup();
    }

    [Command]
    private void CmdDropPickup()
    {
        CommonDropPickup();
    }

    private void CommonDropPickup()
    {
        //AkSoundEngine.PostEvent("Throw", gameObject);
        HideFakeObject();
        newHeldObj = HoldableType.None;

        GameObject throwableToSpawn = null;
        if (isLocalPlayer)
        {
            throwableToSpawn = GManager.Instance.GetCachedObject(heldObjectType);
            Transform positionToSpawnAt = null;

            if (playerMove.Grounded)
                positionToSpawnAt = dropBox.transform;
            else
                positionToSpawnAt = airbornePickupDropPosition.transform;

            if (heldObjectType == PickupableObject.PickupableType.Torch)
                // use syncvared value to turn on physics in client after spawn
                throwableToSpawn.GetComponent<InteractableObjectSpawnCorrections>().turnOnPhysicsAtStart = true;

            throwableToSpawn.transform.position = positionToSpawnAt.position;
            throwableToSpawn.transform.rotation = positionToSpawnAt.rotation;
            throwableToSpawn.GetComponent<Rigidbody>().useGravity = true;
            throwableToSpawn.GetComponent<Rigidbody>().isKinematic = false;
            throwableToSpawn.GetComponent<Collider>().isTrigger = false;
        }

        if (isServer)
            GManager.Instance.CachedObjectWasUsed(heldObjectType, playerMove.PlayerID == 1);
    }

    private void DropPlayer()
    {
        CommonDropPlayer();

        if (isLocalPlayer)
        {
            if (isServer)
                RpcDropPlayer();
            else
                CmdDropPlayer();
        }
    }

    private void CommonDropPlayer()
    {
        int otherPlayerID = playerMove.PlayerID == 1 ? 2 : 1;
        //timeOfThrow = Time.time;
        HideFakeObject();
        newHeldObj = HoldableType.None;
        GManager.Instance.RestorePlayerOverride(dropBox.transform.position, dropBox.transform.rotation, otherPlayerID);

        // If you're not the local player, apply force to the one that is
        if (!isLocalPlayer)
        {
            if (!otherPlayer)
                FindOtherPlayer();

            GManager.Instance.RestoreCameraFollow(otherPlayerID);
        }
    }

    [ClientRpc]
    private void RpcDropPlayer()
    {
        if (!isLocalPlayer)
            CommonDropPlayer();
    }

    [Command]
    private void CmdDropPlayer()
    {
        CommonDropPlayer();
    }


    [Command]
    private void CmdServerSpawnObject(GameObject objectToSpawn)
    {
        NetworkServer.Spawn(objectToSpawn);
    }

    public void ThrowPickup()
    {
        CommonThrowPickup();

        if (isLocalPlayer)
        {
            timeOfThrow = Time.time;

            if (isServer)
            {
                RpcThrowPickup();
            }
            else
            {
                CmdThrowPickup();
            }
        }
    }

    private void CommonThrowPickup()
    {
        AkSoundEngine.PostEvent("Throw", gameObject);

        HideFakeObject();

        GameObject throwableToSpawn = null;
        if (isLocalPlayer)
        {
            throwableToSpawn = GManager.Instance.GetCachedObject(heldObjectType);

            throwableToSpawn.transform.position = fakeObjects[(int)heldObjectType].transform.position;
            throwableToSpawn.transform.rotation = fakeObjects[(int)heldObjectType].transform.rotation;
            throwableToSpawn.GetComponent<Rigidbody>().useGravity = true;
            throwableToSpawn.GetComponent<Rigidbody>().isKinematic = false;
            throwableToSpawn.GetComponent<Collider>().isTrigger = false;

            if (heldObjectType == PickupableObject.PickupableType.Torch)
                // use syncvared value to turn on physics in client after spawn
                throwableToSpawn.GetComponent<InteractableObjectSpawnCorrections>().turnOnPhysicsAtStart = true;

            throwableToSpawn.GetComponent<Rigidbody>().velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            throwableToSpawn.GetComponent<Rigidbody>().AddForce(throwableToSpawn.transform.forward * throwForce.z, ForceMode.VelocityChange);
            throwableToSpawn.GetComponent<Rigidbody>().AddForce(throwableToSpawn.transform.up * throwForce.y, ForceMode.VelocityChange);
        }

        if (isServer)
            GManager.Instance.CachedObjectWasUsed(heldObjectType, playerMove.PlayerID == 1);

    }

    [ClientRpc]
    private void RpcThrowPickup()
    {
        if (!isLocalPlayer)
            CommonThrowPickup();
    }

    [Command]
    private void CmdThrowPickup()
    {
        CommonThrowPickup();
    }

    private void ThrowPlayer()
    {
        Debug.Log("Throwing player from player " + playerMove.PlayerID + " isLocalPlayer? " + isLocalPlayer + " isServer? " + isServer);

        if (isLocalPlayer)
        {
            CommonThrowPlayer();

            if (isServer)
            {
                RpcThrowPlayer();
            }
            else
            {
                CmdThrowPlayer();
            }
        }
    }

    private void CommonThrowPlayer()
    {
        int otherPlayerID = playerMove.PlayerID == 1 ? 2 : 1;
        timeOfThrow = Time.time;
        HideFakeObject();
        newHeldObj = HoldableType.None;
        GManager.Instance.RestorePlayerOverride(fakeObjects[(int)PickupableObject.PickupableType.Player].transform.position,
            fakeObjects[(int)PickupableObject.PickupableType.Player].transform.rotation, otherPlayerID);

        // If you're not the local player, apply force to the one that is
        if (!isLocalPlayer)
        {
            if (!otherPlayer)
                FindOtherPlayer();

            GManager.Instance.RestoreCameraFollow(otherPlayerID);
            //Match velocity before applying change
            Rigidbody otherPlayerRb = otherPlayer.GetComponent<Rigidbody>();
            otherPlayerRb.velocity = GetComponent<Rigidbody>().velocity;
            otherPlayerRb.AddRelativeForce(throwForcePlayer, ForceMode.VelocityChange);
        }
    }

    [Command]
    private void CmdThrowPlayer()
    {
        CommonThrowPlayer();
    }

    [ClientRpc]
    private void RpcThrowPlayer()
    {
        if (!isLocalPlayer)
            CommonThrowPlayer();
    }

    //Adding:
    //If the top player jumps while being held it will break the joint and reset both the players.
    public void PlayerDrop()
    {
        if (heldObj != null && heldObj.tag.StartsWith("Player"))
        {
            Destroy(joint);
            heldObj.GetComponent<Rigidbody>().interpolation = objectDefInterpolation;
            Destroy(joint);
            heldObj.GetComponent<Rigidbody>().interpolation = objectDefInterpolation;

            heldObj.layer = LayerMask.NameToLayer("Player " + (playerMove.PlayerID == 1 ? 2 : 1));

            heldObj = null;
            playerMove.CanJump = true;

            // Networking logic: this function now needs to be executed by the opposite version of this player instance
            if (isLocalPlayer && isServer)
                RpcPlayerDrop(playerMove.PlayerID);
            else if (isLocalPlayer && !isServer)
                CmdPlayerDrop(playerMove.PlayerID);
        }
    }

    [ClientRpc]
    public void RpcPlayerDrop(int targetPlayerID)
    {
        CommonPlayerDrop(targetPlayerID);
    }

    [Command]
    public void CmdPlayerDrop(int targetPlayerID)
    {
        CommonPlayerDrop(targetPlayerID);
    }

    private void CommonPlayerDrop(int targetPlayerID)
    {
        // Execution already happened in local player at this point, so we avoid circular referencing
        if (!isLocalPlayer && playerMove.PlayerID == targetPlayerID)
            PlayerDrop();
    }

    #endregion

    //connect player and pickup/pushable object via a physics joint
    private void AddJoint()
    {
        if (heldObj)
        {
            AkSoundEngine.PostEvent("Pickup", gameObject);

            if (joint)
            {
                Destroy(joint);
            }

            joint = heldObj.AddComponent<FixedJoint>();
            joint.connectedBody = GetComponent<Rigidbody>();
            heldObj.layer = gameObject.layer;
        }
    }

    private void FindOtherPlayer()
    {
        if (otherPlayer)
            return;

        PlayerMove[] players = GameObject.FindObjectsOfType<PlayerMove>();
        foreach (PlayerMove player in players)
        {
            if (player != playerMove)
            {
                otherPlayer = player;
                break;
            }
        }
    }

    /// <summary>
    /// Send message to modify animator of player on client-side
    /// </summary>
    /// <param name="animatorAttributeName"></param>
    /// <param name="value"></param>
    [ClientRpc]
    private void RpcUpdateClientAnimator(string animatorAttributeName, bool value)
    {
        if (animator)
        {
            animator.SetBool(animatorAttributeName, value);
        }
    }

    /// <summary>
    /// Send message to modify animator of player on server-side
    /// </summary>
    /// <param name="animatorAttributeName"></param>
    /// <param name="value"></param>
    [Command]
    private void CmdUpdateClientAnimator(string animatorAttributeName, bool value)
    {
        if (animator)
        {
            animator.SetBool(animatorAttributeName, value);
        }
    }

    //draws red sphere if something is in way of pickup (select player in scene view to see)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(holdPos, checkRadius);
    }

    // Checks if the box is hanging off a ledge and removes the joint if it is.
    private void checkIfBoxIsHanging()
    {
        // If we've jumped and our velocity is 0 it means the box is keeping us afloat and we should drop it.
        if ((rb.velocity.y < boxHangThreshold && rb.velocity.y > -boxHangThreshold && !playerMove.Grounded)
            && (heldObj != null && heldObj.CompareTag("Pickup")))
            DropPickup();
    }
}

/* NOTE: to check if the player is able to lift an object, and that nothing is above their head, a sphereCheck is used. (line 100)
 * this has a layermask set to layer index 2, by default: "Ignore Raycast", so to lift objects properly, any triggers need to be set to this layer
 * else the sphereCheck will collide with the trigger, and the script will think we cannot lift an object here, as there is something above our head */
