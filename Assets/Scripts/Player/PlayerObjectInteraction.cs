using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

//this allows the player to pick up/throw, and also pull certain objects
//you need to add the tags "Pickup" or "Pushable" to these objects
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMove))]
public class PlayerObjectInteraction : NetworkBehaviour
{
    public GameObject holdPlayerPos;
    public GameObject particlesBoxCollide;
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
    private ResetButton resetButton = null;
    private PlayerMove otherPlayer = null;

    // State attributes
    private float timeOfPickup, timeOfThrow, defRotateSpeed;
    private bool canPushButton = false;

    private float originalMass;

    // Private references
    private PlayerMove playerMove;
    private CharacterMotor characterMotor;
    private AudioSource audioSource;
    private TriggerParent triggerParent;
    private RigidbodyInterpolation objectDefInterpolation;
    private Rigidbody rb;
    public float powCooldown = 0.75f;
    private float currentPowCooldown = 0;
    public float vibrationDuration = 0.5f;
    private float vibrationTime = 0;
    public float vibrationIntensity = 0.1f;

    //setup
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

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
        if (Input.GetButtonDown("Grab") && heldObj)
        {
            if (heldObj.tag == "Pickup" && Time.time > timeOfPickup + throwThrowableCooldownTime)
            {
                ThrowPickup();
            }
            else if (heldObj.tag == "Player" && Time.time > timeOfPickup + throwPlayerCooldownTime)    //NOTE: can combine with above 'if' ---Added for player to pick up another player
            {
                ThrowPickup();
            }
            else if (heldObj.tag == "Pushable")
                DropPickup();
        }

        if (Input.GetButtonDown("Grab") && heldObj == null && canPushButton)
        {
            PushButton();
            // TODO: huh?
            return;
        }

        //set animation value for arms layer
        if (animator)
        {
            if (heldObj)
            {
                // --------- Holding animations ---------
                if (heldObj.tag == "Pickup")
                {
                    animator.SetBool("HoldingPickup", true);
                    if (isServer)
                        RpcUpdateClientAnimator("HoldingPickup", true);
                    else
                        CmdUpdateClientAnimator("HoldingPickup", true);
                }
                else if (heldObj.tag.StartsWith("Player"))
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
                if (heldObj && heldObj.tag == "Pushable")
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
        if (Input.GetButtonDown("Drop") && heldObj != null)
        {
            DropPickup();
        }

        checkIfBoxIsHanging();

        if (currentPowCooldown < powCooldown)
            currentPowCooldown += Time.deltaTime;

        if (vibrationTime > 0)
        {
            vibrationTime -= Time.deltaTime;

            if (vibrationTime <= 0)
            {
                XInputDotNetPure.GamePad.SetVibration(playerMove.PlayerID == 1 ? XInputDotNetPure.PlayerIndex.One : XInputDotNetPure.PlayerIndex.Two, 0f, 0f);
            }
        }
    }
    #endregion

    #region Collision
    void OnTriggerEnter(Collider other)
    {
        if (currentPowCooldown > powCooldown && other.gameObject.layer != LayerMask.NameToLayer("Player 1") && other.gameObject.layer != LayerMask.NameToLayer("Player 2")
            && other.gameObject.layer != 2 /*ignore raycast*/ && other.bounds.max.y > gameObject.GetComponent<Collider>().bounds.max.y)
        {
            Instantiate(particlesBoxCollide, transform.position + transform.forward * 0.5f + transform.up, transform.rotation);
            currentPowCooldown = 0;
            XInputDotNetPure.GamePad.SetVibration(playerMove.PlayerID == 1 ? XInputDotNetPure.PlayerIndex.One : XInputDotNetPure.PlayerIndex.Two, vibrationIntensity, vibrationIntensity);
            vibrationTime = vibrationDuration;
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

        if (other.tag == "Button")
        {
            canPushButton = true;
            resetButton = other.GetComponent<ResetButton>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Button")
        {
            canPushButton = false;
            resetButton = null;
        }
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

            // Give the object either host or client authority, depending on which player is picking it up
            if (other.tag != "Player")
            {
                //pickup
                if (other.tag == "Pickup" && heldObj == null && timeOfThrow + 0.2f < Time.time)
                {
                    ResettableObject resettableObject = other.GetComponent<ResettableObject>();
                    if (resettableObject != null && !resettableObject.IsHeld)
                        LiftPickup(other);
                }
                //grab
                else if (other.tag == "Pushable" && (other.gameObject.layer != LayerMask.NameToLayer(("Invisible Player " + playerMove.PlayerID))) && heldObj == null && timeOfThrow + 0.2f < Time.time)
                {
                    if (playerMove.FullyGrounded && playerMove.lastFeetTouched != other.transform)
                    {
                        Vector3 heading = other.transform.position - playerMove.transform.position;
                        float dot = Vector3.Dot(heading, playerMove.transform.forward);

                        if (dot > 0)
                            GrabPushable(other);
                    }
                }

                // Give the object either host or client authority, depending on which player is picking it up
                if (isServer)
                    SetPlayerAuthorityToHeldObject(GetComponent<NetworkIdentity>(), playerMove.PlayerID, other.GetComponent<NetworkIdentity>());
                else
                    CmdSetPlayerAuthorityToHeldObject(GetComponent<NetworkIdentity>(), playerMove.PlayerID, other.GetComponent<NetworkIdentity>());

                return;
            }
            //NOTE: Added to pickup the player:
            if (other.tag == "Player" && heldObj == null && timeOfThrow + 0.2f < Time.time)
            {
                PickupPlayer(other);    //Created new function.
                return;
            }
        }
    }
    #endregion

    public void PushButton()
    {
        if (resetButton)
            resetButton.Push(playerMove.PlayerID);
        else
            Debug.LogError("Button reference is missing dude!");
    }

    /// <summary>
    /// Places the carried player above this players head. Used in networking transform corrections
    /// </summary>
    public void ResetHoldPosition()
    {
        holdPos = transform.position;
        holdPos.y += (GetComponent<Collider>().bounds.extents.y) + (heldObj.GetComponent<Collider>().bounds.extents.y) + gap;
        heldObj.transform.position = holdPos;
        heldObj.transform.rotation = transform.rotation;
    }

    #region Interactions
    private void GrabPushable(Collider other)
    {
        Vector3 touchedPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        if (touchedPoint.y > transform.position.y) return;

        playerMove.transform.LookAt(touchedPoint);

        heldObj = other.gameObject;
        objectDefInterpolation = heldObj.GetComponent<Rigidbody>().interpolation;
        heldObj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        AddJoint();
        //Is grabbing pushable box?
        playerMove.IsGrabingPushable = true;
        //set limits for when player will let go of object
        //joint.breakForce = holdingBreakForce;
        //joint.breakTorque = holdingBreakTorque;
        //stop player rotating in direction of movement, so they can face the block theyre pulling
        playerMove.rotateSpeed = 0;

        //playerMove.SetRestrictToBackCamera(true);

        PushableObject po = other.GetComponent<PushableObject>();
        if (po)
            po.SetIsBeingPushed(true);
        else
            Debug.LogError("Unasignsed PushableObject component");
    }

    private void PickupPlayer(Collider other)
    {
        Collider otherMesh = other.GetComponent<Collider>();
        holdPos = transform.position;
        holdPos.y += (GetComponent<Collider>().bounds.extents.y) + (otherMesh.bounds.extents.y) + gap;

        //if there is space above our head, pick up item (layermask index 2: "Ignore Raycast", anything on this layer will be ignored)
        if (!Physics.CheckSphere(holdPos, checkRadius, 2))
        {
            heldObj = other.gameObject;

            heldObj.layer = LayerMask.NameToLayer("Player " + (playerMove.PlayerID == 1 ? 2 : 1) + " While Carried");

            Rigidbody heldObjectRigidbody = heldObj.GetComponent<Rigidbody>();
            heldObj.transform.position = holdPos;
            heldObj.transform.rotation = transform.rotation;

            heldObj.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            heldObj.GetComponent<Rigidbody>().isKinematic = true;
            heldObj.GetComponent<PlayerMove>().LockMovementToOtherPlayer(holdPlayerPos.transform);


            playerMove.CanJump = false;   //Bottom player cannot jump
            heldObj.GetComponent<PlayerMove>().SetIsBeingHeld(true);

            timeOfPickup = Time.time;
        }
        //if not print to console (look in scene view for sphere gizmo to see whats stopping the pickup)
        else
        {
            gizmoColor = Color.red;
            print("Can't lift object here. If nothing is above the player, make sure triggers are set to layer index 2 (ignore raycast by default)");
        }

        // Networking logic: this function now needs to be executed by the opposite version of this player instance
        if (isLocalPlayer && isServer)
            RpcPickupPlayer(playerMove.PlayerID);
        else if (isLocalPlayer && !isServer)
            CmdPickupPlayer(playerMove.PlayerID);
    }

    [ClientRpc]
    private void RpcPickupPlayer(int targetPlayerID)
    {
        CommonPickupPlayerCommand(targetPlayerID);
    }

    [Command]
    private void CmdPickupPlayer(int targetPlayerID)
    {
        CommonPickupPlayerCommand(targetPlayerID);
    }

    /// <summary>
    /// To be called by the networking commands to resolve the same logic from different network origins (client/server)
    /// </summary>
    /// <param name="targetPlayerID"></param>
    private void CommonPickupPlayerCommand(int targetPlayerID)
    {
        if (otherPlayer == null)
        {
            FindOtherPlayer();
        }

        // Execution already happened in local player at this point, so we avoid circular referencing
        if (!isLocalPlayer && playerMove.PlayerID == targetPlayerID)
            PickupPlayer(otherPlayer.GetComponent<Collider>());
    }

    private void LiftPickup(Collider other)
    {
        //get where to move item once its picked up
        Mesh otherMesh = other.GetComponent<MeshFilter>().mesh;
        holdPos = transform.position;
        holdPos.y += (GetComponent<Collider>().bounds.extents.y) + (otherMesh.bounds.extents.y) + gap;

        //if there is space above our head, pick up item (layermask index 2: "Ignore Raycast", anything on this layer will be ignored)
        if (!Physics.CheckSphere(holdPos, checkRadius, 2))
        {
            gizmoColor = Color.green;
            heldObj = other.gameObject;
            Rigidbody heldObjectRigidbody = heldObj.GetComponent<Rigidbody>();
            objectDefInterpolation = heldObjectRigidbody.interpolation;
            heldObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            heldObj.transform.position = holdPos;
            heldObj.transform.rotation = transform.rotation;
            AddJoint();
            //NEW: Is holding pickup box
            playerMove.IsHoldingPickup = true;

            //here we adjust the mass of the object, so it can seem heavy, but not effect player movement whilst were holding it
            heldObjectRigidbody.mass *= weightChange;
            //make sure we don't immediately throw object after picking it up
            timeOfPickup = Time.time;
        }
        //if not print to console (look in scene view for sphere gizmo to see whats stopping the pickup)
        else
        {
            gizmoColor = Color.red;
            print("Can't lift object here. If nothing is above the player, make sure triggers are set to layer index 2 (ignore raycast by default)");
        }

        // If the object is a pickup set the boolean that its currently being held
        ResettableObject resettableObject = heldObj.GetComponent<ResettableObject>();
        if (resettableObject != null && heldObj.CompareTag("Pickup"))
        {
            resettableObject.IsHeld = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetNetworkIdentity">Client to receive authority of the object</param>
    /// <param name="playerID"></param>
    /// <param name="netIdentityOfObj">Network identity of the gameObject that will have its player auth modified</param>
    public void SetPlayerAuthorityToHeldObject(NetworkIdentity targetNetworkIdentity, int playerID, NetworkIdentity netIdentityOfObj)
    {
        if (otherPlayer == null)
        {
            FindOtherPlayer();
        }

        // Remove prior ownership if necessary
        // TODO: consider removing authority (back to server) after letting go of heldObj
        if (netIdentityOfObj.clientAuthorityOwner != null)
            if (netIdentityOfObj.clientAuthorityOwner != targetNetworkIdentity.connectionToClient)
            {
                netIdentityOfObj.RemoveClientAuthority(otherPlayer.GetComponent<NetworkIdentity>().connectionToClient);
            }

        netIdentityOfObj.AssignClientAuthority((targetNetworkIdentity.connectionToClient));

    }

    [Command]
    public void CmdSetPlayerAuthorityToHeldObject(NetworkIdentity networkIdentity, int playerID, NetworkIdentity objToChangeAuthNetIdentity)
    {
        SetPlayerAuthorityToHeldObject(networkIdentity, playerID, objToChangeAuthNetIdentity);
    }

    public void DropPickup()
    {
        Rigidbody heldObjectRigidbody = heldObj.GetComponent<Rigidbody>();

        if (heldObj.tag == "Pickup")
        {
            heldObj.layer = 0; //Default layer

            heldObj.transform.position = dropBox.transform.position;
            heldObjectRigidbody.mass /= weightChange;

            heldObj.GetComponent<FixedJoint>().connectedBody = null;

            // If the object is a pickup set the boolean that its currently being held                
            ResettableObject resettableObject = heldObj.GetComponent<ResettableObject>();
            if (resettableObject != null)
                resettableObject.IsHeld = false;

            //Is holding pickup box?
            playerMove.IsHoldingPickup = false;
        }

        //NOTE: Added the bottom player allow and drop the top player
        if (heldObj.tag == "Player")
        {
            heldObj.transform.position = dropBox.transform.position;
            heldObj.GetComponent<Rigidbody>().isKinematic = false;
            heldObj.GetComponent<PlayerMove>().UnlockMovementToOtherPlayer();
            heldObj.GetComponent<PlayerMove>().SetIsBeingHeld(false);
            playerMove.CanJump = true;

            heldObj.layer = LayerMask.NameToLayer("Player " + (playerMove.PlayerID == 1 ? 2 : 1));

            // Networking logic: this function now needs to be executed by the opposite version of this player instance
            if (isLocalPlayer && isServer)
                RpcDropPickup(playerMove.PlayerID);
            else if (isLocalPlayer && !isServer)
                CmdDropPickup(playerMove.PlayerID);
        }

        heldObjectRigidbody.interpolation = objectDefInterpolation;
        heldObjectRigidbody.useGravity = true;
        heldObj.GetComponent<Collider>().isTrigger = false;
        Destroy(joint);
        playerMove.rotateSpeed = defRotateSpeed;
        //playerMove.SetRestrictToBackCamera(false);

        if (heldObj.tag == "Pushable")
        {
            heldObj.layer = 0; //Default layer

            PushableObject po = heldObj.GetComponent<PushableObject>();
            if (po)
                po.SetIsBeingPushed(false);
            else
                Debug.LogError("Unasignsed PushableObject component");

            //Is grabbing pushable box?
            playerMove.IsGrabingPushable = false;
        }

        heldObj = null;

        timeOfThrow = Time.time;
    }

    [ClientRpc]
    private void RpcDropPickup(int targetPlayerID)
    {
        CommonDropPickup(targetPlayerID);
    }

    [Command]
    private void CmdDropPickup(int targetPlayerID)
    {
        CommonDropPickup(targetPlayerID);
    }

    /// <summary>
    /// To be called by the networking commands to resolve the same logic from different network origins (client/server)
    /// </summary>
    /// <param name="targetPlayerID"></param>
    private void CommonDropPickup(int targetPlayerID)
    {
        // Execution already happened in local player at this point, so we avoid circular referencing
        if (!isLocalPlayer && playerMove.PlayerID == targetPlayerID)
            DropPickup();
    }

    public void ThrowPickup()
    {
        // If the object is a pickup set the boolean that its currently being held
        ResettableObject resettableObject = heldObj.GetComponent<ResettableObject>();
        Rigidbody heldObjectRigidbody = heldObj.GetComponent<Rigidbody>();
        if (resettableObject != null && heldObj.CompareTag("Pickup"))
        {
            resettableObject.IsHeld = false;
        }

        if (heldObj.CompareTag("Pickup") || heldObj.CompareTag("Pushable"))
        {
            // And modify mass
            heldObj.GetComponent<Collider>().isTrigger = false;
            heldObjectRigidbody.useGravity = true;
            heldObjectRigidbody.interpolation = objectDefInterpolation;
            heldObjectRigidbody.mass /= weightChange;
        }

        AkSoundEngine.PostEvent("Throw", gameObject);

        Destroy(joint);

        //Note Added:
        if (heldObj.tag == "Player")
        {
            PlayerMove heldPlayerMove = heldObj.GetComponent<PlayerMove>();
            heldObj.GetComponent<Rigidbody>().isKinematic = false;
            heldPlayerMove.UnlockMovementToOtherPlayer();
            heldPlayerMove.SetIsBeingHeld(false);

            if (heldPlayerMove.isLocalPlayer)
                heldObjectRigidbody.AddRelativeForce(throwForcePlayer, ForceMode.Impulse);

            // Networking logic: this function now needs to be executed by the opposite version of this player instance
            if (isLocalPlayer && isServer)
                RpcThrowPickup(playerMove.PlayerID, transform.position, transform.rotation, rb.velocity);
            else if (isLocalPlayer && !isServer)
                CmdThrowPickup(playerMove.PlayerID, transform.position, transform.rotation, rb.velocity);

            heldObj.layer = LayerMask.NameToLayer("Player " + (playerMove.PlayerID == 1 ? 2 : 1));
        }
        else
        {
            heldObjectRigidbody.AddRelativeForce(throwForce, ForceMode.VelocityChange);
            //Is holding pickup box
            playerMove.IsHoldingPickup = false;
        }
        heldObj = null;
        playerMove.CanJump = true;    //Added: lets the bottom player jump again
        timeOfThrow = Time.time;
    }

    [ClientRpc]
    private void RpcThrowPickup(int targetPlayerID, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        CommonThrowPickup(targetPlayerID, position, rotation, velocity);
    }

    [Command]
    private void CmdThrowPickup(int targetPlayerID, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        CommonThrowPickup(targetPlayerID, position, rotation, velocity);
    }

    /// <summary>
    /// To be called by the networking commands to resolve the same logic from different network origins (client/server)
    /// </summary>
    /// <param name="targetPlayerID"></param>
    private void CommonThrowPickup(int targetPlayerID, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        // Ensure that the non-local player has been updated before attempting throw
        transform.position = position;
        transform.rotation = rotation;
        rb.velocity = velocity;

        // Execution already happened in local player at this point, so we avoid circular referencing
        if (!isLocalPlayer && playerMove.PlayerID == targetPlayerID)
            ThrowPickup();
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
            timeOfThrow = Time.time;

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
