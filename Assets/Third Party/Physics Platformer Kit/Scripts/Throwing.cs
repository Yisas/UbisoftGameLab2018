using UnityEngine;
using System.Collections;

//this allows the player to pick up/throw, and also pull certain objects
//you need to add the tags "Pickup" or "Pushable" to these objects
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMove))]
public class Throwing : MonoBehaviour
{
    // GGJ additions
    public int playerID;
    private ResetButton resetButton;
    private bool canPushButton = false;
    private ResetButton button = null;

    public AudioClip pickUpSound;                               //sound when you pickup/grab an object
    public AudioClip throwSound;                                //sound when you throw an object
    public AudioClip boxCollideSound;                           //sound when you collide with a box
    public AudioClip appearObjectSound;                         //sound when you make an object apper

    public GameObject particlesBoxCollide;
    public GameObject particlesObjectAppear;

    public GameObject grabBox;                                  //objects inside this trigger box can be picked up by the player (think of this as your reach)
    public GameObject dropBox;                                  //positions where the player's objects will begin dropping
    public float gap = 0.5f;                                    //how high above player to hold objects
    public Vector3 throwForce = new Vector3(0, 5, 7);           //the throw force of the player
    public float rotateToBlockSpeed = 3;                        //how fast to face the "Pushable" object you're holding/pulling
    public float checkRadius = 0.5f;                            //how big a radius to check above the players head, to see if anything is in the way of your pickup
    [Range(0.1f, 1f)]                                           //new weight of a carried object, 1 means no change, 0.1 means 10% of its original weight													
    public float weightChange = 0.3f;                           //this is so you can easily carry objects without effecting movement if you wish to
    [Range(10f, 1000f)]
    public float holdingBreakForce = 45, holdingBreakTorque = 45;//force and angularForce needed to break your grip on a "Pushable" object youre holding onto
    public Animator animator;                                   //object with animation controller on, which you want to animate (usually same as in PlayerMove)
    public int armsAnimationLayer;                              //index of the animation layer for "arms"

    [HideInInspector]
    public GameObject heldObj;
    private Vector3 holdPos;
    private FixedJoint joint;
    private float timeOfPickup, timeOfThrow, defRotateSpeed;
    private Color gizmoColor;


    private PlayerMove playerMove;
    private CharacterMotor characterMotor;
    private TriggerParent triggerParent;
    private RigidbodyInterpolation objectDefInterpolation;


    //setup
    void Awake()
    {
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
        defRotateSpeed = playerMove.rotateSpeed;
        //set arms animation layer to animate with 1 weight (full override)
        if (animator)
            animator.SetLayerWeight(armsAnimationLayer, 1);
    }

    //throwing/dropping
    void Update()
    {
        //when we press grab button, throw object if we're holding one
        if (Input.GetButtonDown("Grab " + playerID) && heldObj)
        {
            if (heldObj.tag == "Pickup" && Time.time > timeOfPickup + 0.1f)
                ThrowPickup();
            else if (heldObj.tag == "Pushable")
                DropPickup();

        }

        if (Input.GetButtonDown("Grab " + playerID) && heldObj == null && canPushButton)
        {
            animator.SetTrigger("PushSingleMotion");

            if (button)
                button.Push();
            else
                Debug.LogError("Button reference is missing dude!");

            return;
        }

        //set animation value for arms layer
        if (animator)
            if (heldObj && heldObj.tag == "Pickup")
                animator.SetBool("HoldingPickup", true);
            else
                animator.SetBool("HoldingPickup", false);

        if (heldObj && heldObj.tag == "Pushable")
            animator.SetBool("HoldingPushable", true);
        else
            animator.SetBool("HoldingPushable", false);

        //when grab is released, let go of any pushable objects were holding
        if (Input.GetButtonDown("Drop " + playerID) && heldObj != null)
        {
            DropPickup();
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Pushable" || LayerMask.LayerToName(other.gameObject.layer).Contains("Invisible") || LayerMask.LayerToName(other.gameObject.layer).Contains("Appearing"))
        {
            AudioSource aus = GetComponent<AudioSource>();

            if (boxCollideSound)
            {
                if (other.GetComponent<AppearingObject>() && other.gameObject.layer != LayerMask.NameToLayer("Default"))
                {
                    if (appearObjectSound)
                    {
                        Instantiate(particlesObjectAppear, transform.position + transform.forward + transform.up, transform.rotation);
                        GetComponent<AudioSource>().volume = 1f;
                        aus.clip = appearObjectSound;
                        aus.Play();
                    }
                }
                else
                {
                    GetComponent<AudioSource>().volume = 0.5f;
                    aus.clip = boxCollideSound;
                    aus.Play();
                }
            }
        }

        if (other.tag == "Button")
        {
            canPushButton = true;
            button = other.GetComponent<ResetButton>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Button")
        {
            canPushButton = false;
            button = null;
        }
    }

    //pickup/grab
    void OnTriggerStay(Collider other)
    {
        //if grab is pressed and an object is inside the players "grabBox" trigger
        if (Input.GetButton("Grab " + playerID))
        {
            //pickup
            if (other.tag == "Pickup" && heldObj == null && timeOfThrow + 0.2f < Time.time)
            {
                LiftPickup(other);
                return;
            }
            //grab
            if (other.tag == "Pushable" && (other.gameObject.layer != LayerMask.NameToLayer(("Invisible Player " + playerID))) && heldObj == null && timeOfThrow + 0.2f < Time.time)
            {
                GrabPushable(other);
                return;
            }
        }
    }

    public void PushButton()
    {
        if (resetButton)
            resetButton.Push();

        resetButton = null;
    }

    private void GrabPushable(Collider other)
    {
        heldObj = other.gameObject;
        objectDefInterpolation = heldObj.GetComponent<Rigidbody>().interpolation;
        heldObj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        AddJoint();
        //set limits for when player will let go of object
        //joint.breakForce = holdingBreakForce;
        //joint.breakTorque = holdingBreakTorque;
        //stop player rotating in direction of movement, so they can face the block theyre pulling
        playerMove.rotateSpeed = 0;

        playerMove.SetRestrictMovementToOneAxis(true);

        PushableObject po = other.GetComponent<PushableObject>();
        if (po)
            po.SetIsBeingPushed(true);
        else
            Debug.LogError("Unasignsed PushableObject component");
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
            objectDefInterpolation = heldObj.GetComponent<Rigidbody>().interpolation;
            heldObj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            heldObj.transform.position = holdPos;
            heldObj.transform.rotation = transform.rotation;
            AddJoint();
            //here we adjust the mass of the object, so it can seem heavy, but not effect player movement whilst were holding it
            heldObj.GetComponent<Rigidbody>().mass *= weightChange;
            //make sure we don't immediately throw object after picking it up
            timeOfPickup = Time.time;
        }
        //if not print to console (look in scene view for sphere gizmo to see whats stopping the pickup)
        else
        {
            gizmoColor = Color.red;
            print("Can't lift object here. If nothing is above the player, make sure triggers are set to layer index 2 (ignore raycast by default)");
        }
    }

    private void DropPickup()
    {
        if (heldObj.tag == "Pickup")
        {
            heldObj.transform.position = dropBox.transform.position;
            heldObj.GetComponent<Rigidbody>().mass /= weightChange;
        }

        heldObj.GetComponent<Rigidbody>().interpolation = objectDefInterpolation;
        Destroy(joint);
        playerMove.rotateSpeed = defRotateSpeed;
        playerMove.SetRestrictMovementToOneAxis(false);

        if (heldObj.tag == "Pushable")
        {
            PushableObject po = heldObj.GetComponent<PushableObject>();
            if (po)
                po.SetIsBeingPushed(false);
            else
                Debug.LogError("Unasignsed PushableObject component");
        }

        heldObj = null;
        timeOfThrow = Time.time;


    }

    public void ThrowPickup()
    {
        if (throwSound)
        {
            GetComponent<AudioSource>().volume = 1;
            GetComponent<AudioSource>().clip = throwSound;
            GetComponent<AudioSource>().Play();
        }
        Destroy(joint);
        heldObj.GetComponent<Rigidbody>().interpolation = objectDefInterpolation;
        heldObj.GetComponent<Rigidbody>().mass /= weightChange;
        heldObj.GetComponent<Rigidbody>().AddRelativeForce(throwForce, ForceMode.VelocityChange);
        heldObj = null;
        timeOfThrow = Time.time;
    }

    //connect player and pickup/pushable object via a physics joint
    private void AddJoint()
    {
        if (heldObj)
        {
            if (pickUpSound)
            {
                GetComponent<AudioSource>().volume = 1;
                GetComponent<AudioSource>().clip = pickUpSound;
                GetComponent<AudioSource>().Play();
            }
            joint = heldObj.AddComponent<FixedJoint>();
            joint.connectedBody = GetComponent<Rigidbody>();
        }
    }

    //draws red sphere if something is in way of pickup (select player in scene view to see)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(holdPos, checkRadius);
    }
}

/* NOTE: to check if the player is able to lift an object, and that nothing is above their head, a sphereCheck is used. (line 100)
 * this has a layermask set to layer index 2, by default: "Ignore Raycast", so to lift objects properly, any triggers need to be set to this layer
 * else the sphereCheck will collide with the trigger, and the script will think we cannot lift an object here, as there is something above our head */
