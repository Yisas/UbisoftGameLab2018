﻿using UnityEngine;
using System.Collections;

//handles player movement, utilising the CharacterMotor class
[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField]
    private int playerID;

    // Camera interactions
    public float cameraDelayTimerBeforeRespawn;
    private bool restrictMovementToOneAxis = false;
    private bool restrictMovementToTwoAxis = false;
    private bool restrictToBackCamera = false;

    //setup
    public Transform mainCam, floorChecks;      //main camera, and floorChecks object. FloorChecks are raycasted down from to check the player is grounded.
    public Animator animator;                   //object with animation controller on, which you want to animate
    public AudioClip jumpSound;                 //play when jumping
    public AudioClip landSound;                 //play when landing on ground
    public AudioClip runSound;                  //play when running
    public Transform lastFeetTouched;           //what the floorchecks last touched

    //movement
    public float accel = 70f;                   //acceleration/deceleration in air or on the ground
    public float airAccel = 18f;
    public float decel = 7.6f;
    public float airDecel = 1.1f;
    [Range(0f, 5f)]
    public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f; //how fast to rotate on the ground, how fast to rotate in the air
    public float maxSpeed = 9;                              //maximum speed of movement in X/Z axis
    public float slopeLimit = 40, slideAmount = 35;         //maximum angle of slopes you can walk on, how fast to slide down slopes you can't
    public float movingPlatformFriction = 7.7f;             //you'll need to tweak this to get the player to stay on moving platforms properly
    public float maxVerticalVel = 12;

    //jumping
    public Vector3 jumpForce = new Vector3(0, 13, 0);       //normal jump force
    [Tooltip("Jump force while being held by another player, in order to jump off")]
    public Vector3 jumpForceWhileCarried; //= new Vector3(0, 2, 4);
    public float jumpDelay = 0.1f;                          //how fast you need to jump after hitting the ground, to do the next type of jump
    public float jumpLeniancy = 0.17f;                      //how early before hitting the ground you can press jump, and still have it work
    public int jumpPromptConter = 10;

    // States
    private bool grounded;
    private bool fullyGrounded;
    private bool canJump = true;
    private bool isBeingHeld = false;
    private bool isHoldingPickup = false;
    private bool isGrabingPushable = false;

    // Movement data
    private float airPressTime, groundedCount, curAccel, curDecel, curRotateSpeed, slope;
    private Vector3 direction, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;

    // Private references
    private Transform[] floorCheckers;
    private Quaternion screenMovementSpace;
    private CharacterMotor characterMotor;

    //setup
    void Awake()
    {
        //create single floorcheck in centre of object, if none are assigned
        if (!floorChecks)
        {
            floorChecks = new GameObject().transform;
            floorChecks.name = "FloorChecks";
            floorChecks.parent = transform;
            floorChecks.position = transform.position;
            GameObject check = new GameObject();
            check.name = "Check1";
            check.transform.parent = floorChecks;
            check.transform.position = transform.position;
            Debug.LogWarning("No 'floorChecks' assigned to PlayerMove script, so a single floorcheck has been created", floorChecks);
        }
        //assign player tag if not already
        if (tag != "Player")
        {
            tag = "Player";
            Debug.LogWarning("PlayerMove script assigned to object without the tag 'Player', tag has been assigned automatically", transform);
        }

        //usual setup
        characterMotor = GetComponent<CharacterMotor>();
        //gets child objects of floorcheckers, and puts them in an array
        //later these are used to raycast downward and see if we are on the ground
        floorCheckers = new Transform[floorChecks.childCount];
        for (int i = 0; i < floorCheckers.Length; i++)
            floorCheckers[i] = floorChecks.GetChild(i);
    }

    //get state of player, values and input
    void Update()
    {
        //handle jumping
        JumpCalculations();
        //adjust movement values if we're in the air or on the ground
        curAccel = (grounded) ? accel : airAccel;
        curDecel = (grounded) ? decel : airDecel;
        curRotateSpeed = (grounded) ? rotateSpeed : airRotateSpeed;

        //get movement axis relative to camera
        screenMovementSpace = Quaternion.Euler(0, mainCam.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;

        //get movement input, set direction to move in. Movement inputs stay at zero if you're being held, otherwise they get processed
        float horizontalInput = 0;
        float verticalInput = 0;
        if (!isBeingHeld)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal " + playerID);
            verticalInput = Input.GetAxisRaw("Vertical " + playerID);
        }

        direction = (screenMovementForward * verticalInput) + (screenMovementRight * horizontalInput);

        moveDirection = transform.position + direction;

        if(GetComponent<Rigidbody>().velocity.y > maxVerticalVel) //Preventing superman jump
        {
            Vector3 currVel = GetComponent<Rigidbody>().velocity;
            GetComponent<Rigidbody>().velocity = new Vector3(currVel.x, maxVerticalVel, currVel.y);
        }
    }

    //apply correct player movement (fixedUpdate for physics calculations)
    void FixedUpdate()
    {
        //are we grounded
        grounded = IsGrounded();
        fullyGrounded = IsFullyGrounded();

        //move, rotate, manage speed
        characterMotor.MoveTo(moveDirection, curAccel, 0.7f, true);

        if (!restrictMovementToOneAxis)
            if (rotateSpeed != 0 && direction.magnitude != 0)
            {
                characterMotor.RotateToDirection(moveDirection, curRotateSpeed * 5, true);
            }

        characterMotor.ManageSpeed(curDecel, maxSpeed + movingObjSpeed.magnitude, true);
        //set animation values
        if (animator)
        {
            animator.SetFloat("DistanceToTarget", characterMotor.DistanceToTarget);
            animator.SetBool("Grounded", grounded);
            animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
        }

        // Adding footsteps audio GGJ2018:
        if (grounded && runSound && !GetComponent<AudioSource>().isPlaying && GetComponent<Rigidbody>().velocity.magnitude > 0)
        {
            // TODO: add clip volume change as attribute, not hardcoded
            GetComponent<AudioSource>().volume = 1;
            GetComponent<AudioSource>().clip = runSound;
            GetComponent<AudioSource>().Play();
        }

    }

    //prevents rigidbody from sliding down slight slopes (read notes in characterMotor class for more info on friction)
    void OnCollisionStay(Collision other)
    {
        //only stop movement on slight slopes if we aren't being touched by anything else
        if (other.collider.tag != "Untagged" || grounded == false)
            return;
        //if no movement should be happening, stop player moving in Z/X axis
        if (direction.magnitude == 0 && slope < slopeLimit && GetComponent<Rigidbody>().velocity.magnitude < 2)
        {
            //it's usually not a good idea to alter a rigidbodies velocity every frame
            //but this is the cleanest way i could think of, and we have a lot of checks beforehand, so it shou
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    private bool IsFullyGrounded()
    {
        //get distance to ground, from centre of collider (where floorcheckers should be)
        float dist = GetComponent<Collider>().bounds.extents.y;
        //check whats at players feet, at each floorcheckers position
        int groundedCount = 0;

        foreach (Transform check in floorCheckers)
        {
            RaycastHit hit;
            if (Physics.Raycast(check.position, Vector3.down, out hit, dist + 0.05f))
            {
                if (!hit.transform.GetComponent<Collider>().isTrigger)
                {
                    //slope control
                    slope = Vector3.Angle(hit.normal, Vector3.up);
                    //slide down slopes
                    if (slope > slopeLimit && hit.transform.tag != "Pushable")
                    {
                        Vector3 slide = new Vector3(0f, -slideAmount, 0f);
                        GetComponent<Rigidbody>().AddForce(slide, ForceMode.Force);
                    }

                    //moving platforms
                    // TODO: double check this implementation
                    if (hit.transform.tag == "MovingPlatform" || hit.transform.tag == "Pushable")
                    {
                        movingObjSpeed = hit.transform.GetComponent<Rigidbody>().velocity;
                        movingObjSpeed.y = 0f;
                        //9.5f is a magic number, if youre not moving properly on platforms, experiment with this number
                        GetComponent<Rigidbody>().AddForce(movingObjSpeed * movingPlatformFriction * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    }
                    else
                    {
                        movingObjSpeed = Vector3.zero;
                    }
                    //yes our feet are on something
                    groundedCount++;
                    lastFeetTouched = hit.transform;
                    continue;
                }
            }
        }

        return groundedCount == floorCheckers.Length;
    }


    //returns whether we are on the ground or not
    //also: bouncing on enemies, keeping player on moving platforms and slope checking
    private bool IsGrounded()
    {
        //get distance to ground, from centre of collider (where floorcheckers should be)
        float dist = GetComponent<Collider>().bounds.extents.y;
        //check whats at players feet, at each floorcheckers position
        foreach (Transform check in floorCheckers)
        {
            RaycastHit hit;
            if (Physics.Raycast(check.position, Vector3.down, out hit, dist + 0.05f))
            {
                if (!hit.transform.GetComponent<Collider>().isTrigger)
                {
                    //slope control
                    slope = Vector3.Angle(hit.normal, Vector3.up);
                    //slide down slopes
                    if (slope > slopeLimit && hit.transform.tag != "Pushable")
                    {
                        Vector3 slide = new Vector3(0f, -slideAmount, 0f);
                        GetComponent<Rigidbody>().AddForce(slide, ForceMode.Force);
                    }

                    //moving platforms
                    // TODO: double check this implementation
                    if (hit.transform.tag == "MovingPlatform" || hit.transform.tag == "Pushable")
                    {
                        movingObjSpeed = hit.transform.GetComponent<Rigidbody>().velocity;
                        movingObjSpeed.y = 0f;
                        //9.5f is a magic number, if youre not moving properly on platforms, experiment with this number
                        GetComponent<Rigidbody>().AddForce(movingObjSpeed * movingPlatformFriction * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    }
                    else
                    {
                        movingObjSpeed = Vector3.zero;
                    }
                    //yes our feet are on something
                    return true;
                }
            }
        }
        movingObjSpeed = Vector3.zero;
        //no none of the floorchecks hit anything, we must be in the air (or water)
        return false;
    }

    //jumping
    private void JumpCalculations()
    {
        //keep how long we have been on the ground
        groundedCount = (grounded) ? groundedCount += Time.deltaTime : 0f;

        //play landing sound
        if (groundedCount < 0.25 && groundedCount != 0 && !GetComponent<AudioSource>().isPlaying && landSound && GetComponent<Rigidbody>().velocity.y < 1)
        {
            GetComponent<AudioSource>().volume = Mathf.Abs(GetComponent<Rigidbody>().velocity.y) / 40;
            GetComponent<AudioSource>().clip = landSound;
            GetComponent<AudioSource>().Play();
        }
        //if we press jump in the air, save the time
        if (Input.GetButtonDown("Jump " + playerID) && !grounded)
            airPressTime = Time.time;

        //if were on ground within slope limit
        if (grounded && slope < slopeLimit)
        {
            //and we press jump, or we pressed jump justt before hitting the ground
            if (Input.GetButtonDown("Jump " + playerID) || airPressTime + jumpLeniancy > Time.time)
            {
                if (!isBeingHeld)
                    Jump(jumpForce);
                else
                {
                    Debug.Log("[PlayerMove Class] Player ID: " + playerID + "\n -----JumpForceWhileCarried: " + jumpForceWhileCarried + "(mag= " + jumpForceWhileCarried.magnitude + " )"
                                + " Mass of Player: " + GetComponent<Rigidbody>().mass);
                    Jump(jumpForceWhileCarried);
                }
            }
        }
    }

    //push player at jump force
    public void Jump(Vector3 jumpVelocity)
    {
        if (!canJump)  //Added.
        {
            return;
        }

        if (jumpSound)
        {
            // TODO: add clip volume change as attribute, not hardcoded. Single get
            GetComponent<AudioSource>().volume = 1;
            GetComponent<AudioSource>().clip = jumpSound;
            GetComponent<AudioSource>().Play();
        }

        GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.z);
        GetComponent<Rigidbody>().AddRelativeForce(jumpVelocity, ForceMode.Impulse);
        airPressTime = 0f;

        //Jump Prompt Counter
        if (jumpPromptConter > 0)
            jumpPromptConter--;

        // Stop being held after jumping
        IsBeingHeld = false;
    }

    public void ToogleRestrictMovementToTwoAxis()
    {
        restrictMovementToTwoAxis = !restrictMovementToTwoAxis;
    }

    public void SetRestrictMovementToTwoAxis(bool value)
    {
        restrictMovementToTwoAxis = value;
    }

    public bool isRestrictedMovementToTwoAxis()
    {
        return restrictMovementToTwoAxis;
    }

    public void SetRestrictToBackCamera(bool value)
    {
        restrictToBackCamera = value;
    }

    public bool isRestrictToBackCamera()
    {
        return restrictToBackCamera;
    }

    public void ToogleRestrictMovementToOneAxis()
    {
        restrictMovementToOneAxis = !restrictMovementToOneAxis;
    }

    public void SetRestrictMovementToOneAxis(bool value)
    {
        restrictMovementToOneAxis = value;
    }

    public bool isRestrictedMovementToOneAxis()
    {
        return restrictMovementToOneAxis;
    }

    // Public attribute visibility methods

    public int PlayerID
    {
        get
        {
            return playerID;
        }
    }

    public bool IsBeingHeld
    {
        get
        {
            return isBeingHeld;
        }
        set
        {
            isBeingHeld = value;

            Rigidbody rb = GetComponent<Rigidbody>();
            // If you are now being held...
            if (isBeingHeld)
            {
                //... unrestrict rotation freezes
                //... by wiping constraints set by CharacterMotor...
                rb.constraints = RigidbodyConstraints.None;
                //... and adding the ones necessary when being carried through a FixedJoint logic
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                //... put back rotation constraints
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
    }

    public bool CanJump
    {
        get
        {
            return canJump;
        }
        set
        {
            canJump = value;
        }
    }

    public bool IsHoldingPickup
    {
        get
        {
            return isHoldingPickup;
        }
        set
        {
            isHoldingPickup = value;
        }
    }

    public bool IsGrabingPushable
    {
        get
        {
            return isGrabingPushable;
        }
        set
        {
            canJump = !value;
            isGrabingPushable = value;
        }
    }

    public bool Grounded
    {
        get
        {
            return grounded;
        }
        set
        {
            grounded = value;
        }
    }

    public bool FullyGrounded
    {
        get
        {
            return fullyGrounded;
        }
        set
        {
            fullyGrounded = value;
        }
    }
}