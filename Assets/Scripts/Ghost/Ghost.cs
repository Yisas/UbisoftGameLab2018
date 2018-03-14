using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* States used in state machine
   Pursuit means the ghost is moving towards a point.
   Wander: the ghost roams aimlessly.
   Return; the ghost is returning a moved object.
   AvoidingWall: the ghost is prioritizing avoiding a wall it has collided with through raycasting.
*/
public enum MovementState { Pursuit, Wander, Return, AvoidingWall}

public class Ghost : Movable
{   
    // Inspector values
    // This radius begins at the ghost's position. The ghost picks up all moved objects within this radius
    public float awarenessRadius;
    // The minimum distance the ghost has to be from the object's original position before the ghost drops it. 
    public float itemDropDistance;
    // The range of the raycast in front of the ghost which determines if it bumps into walls or not
    public float rayCastRange;
    // The amount of time the ghost will move in the wall avoiding direction before it goes back to wandering
    public float wallAvoidanceDuration;
    // The distance of the ray shot from the ghost's shoes which determines whether or not it is grounded. Ray pointing downward.
    public float groundedRayCastDownDistance;
    // Same as groundedRayCstDownDistance, except it shoots a ray up. The ghost will be considered grounded and begin floating back up to ground level. 
    public float groundedRayCastUpDistance;
    // The distance from the character that the ray originates in the forward direction. This is useful for stopping the ray from pointing at the ghost's own cape.
    public float avoidWallRayOriginOffset;
    // Currently the ghost's shoes. Ray are shot up and down from this object.
    public Transform floorChecks;
    /* The wallAvoidanceRange determines how far and at what angle the ghost will move away from a wall.
     * The ghost casts a ray that hits a wall, the normal of that wall is then taken and the ghost moves along that normal.
     * wallAvoidanceRange determines just how far along the normal the ghost will go.
     */
    public float wallAvoidDistance;
    // The current movement state of the ghost, see enum descriptions. 
    public MovementState movementState;

    // Internal values hidden from inspector   
    internal bool isCarryingObject;  

    //Scripts
    GhostObjectInteraction ghostObjectInteraction;

    // Private variables
    private List<ResettableObject> pickupableList;
    private ResettableObject closestResettableObject;
    private ResettableObject carriedObject;
    // Coroutine that changes the ghosts angle as well as height
    private Coroutine angleChangeCoroutine;
    private bool isWanderCoRunning;
    private bool isHittingWall;
    private Vector3 wallNormal;
    private Vector3 rayHitPosition;
    private float wallAvoidanceTimer;
    // The cloth wrapped around the ghost
    private Cloth cloth;
    // Floor checkers are used to check whether or not the ghost is grounded. Currently set to its shoes.
    private Transform[] floorCheckers;

    #region Unity Functions
    // Used for initialization
    void Start()
    {
        pickupableList = new List<ResettableObject>();
        ghostObjectInteraction = gameObject.GetComponent<GhostObjectInteraction>();
        targetRotation = Vector3.zero;
        cloth = GetComponentInChildren<Cloth>();

        floorCheckers = new Transform[floorChecks.childCount];
        for (int i = 0; i < floorCheckers.Length; i++)
            floorCheckers[i] = floorChecks.GetChild(i);
    }

    // Update is called once per frame
    void Update()
    {
        gatherInfo();
        avoidWalls();
        updateState();
        move();
        AddVelocityToCloth();
        isGrounded = checkIfGrounded();
    }

    /*
     * As the ghost moves towards moved objects, it consistently checks if the object it bumps into is
     * the object it is seeking. If it is then the ghost will carry it, provided it isn't already carrying an object.
     */ 
    void OnTriggerStay(Collider collider)
    {
        if (collider.tag == "Pickup")
        {
            ResettableObject pickupableObject = collider.GetComponent<ResettableObject>();
            if (!isCarryingObject && pickupableObject == closestResettableObject)
            {
                ghostObjectInteraction.GrabObject(collider);
                pickupableObject.GetComponent<Rigidbody>().useGravity = false;
                collider.isTrigger = true;
                carriedObject = pickupableObject;
                isCarryingObject = true;
            }
        }
    }

    // Code used to visualize the wall avoidance
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallNormal, 1.0f);
        if (rayHitPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rayHitPosition, 1.0f);
        }
    }

    #endregion

    private void gatherInfo()
    {
        // Reset values
        pickupableList.Clear();

        // Gather info on nearby pickupable objects
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, awarenessRadius);
        foreach (Collider collider in hitColliders)
        {
            // Only add pickup object to list if it has been moved, is not being held and is not on pressure plate
            if (collider.tag == "Pickup")
            {
                ResettableObject pickupableObject = collider.GetComponent<ResettableObject>();                
                if (pickupableObject != null && pickupableObject.IsMoved && !pickupableObject.IsOnPressurePlate 
                    && !pickupableObject.IsHeld)
                    pickupableList.Add(pickupableObject);
            }
        }

        closestResettableObject = findClosestObject(pickupableList);
    }

    // Changes the enum state of the ghost based on its environment
    private void updateState()
    {
        if (isCarryingObject)
        {
            // Return the object to its original location
            movementState = MovementState.Return;
        }
        else if (closestResettableObject != null && !isCarryingObject)
        {
            // Pursue the closest object
            movementState = MovementState.Pursuit;
        }
        else if (isHittingWall)
        {
            movementState = MovementState.AvoidingWall;
        }
        else
        {
            movementState = MovementState.Wander;
        }
    }

    // Moves the ghost according to its state
    private void move()
    {
        if (angleChangeCoroutine != null && movementState != MovementState.Wander)
        {
            StopCoroutine(angleChangeCoroutine);
            isWanderCoRunning = false;
        }
        switch (movementState)
        {
            case MovementState.Pursuit:
                // Pursue the closest object
                MovementUtilityArrive.SteerArrive(this, closestResettableObject.transform.position);
                break;
            case MovementState.Return:
                // Return the carried object
                MovementUtilityArrive.SteerArrive(this, carriedObject.OriginalPosition);
                // Drop the object if we're close enough to its original position
                if(Vector3.Distance(transform.position, carriedObject.OriginalPosition) < itemDropDistance)
                {
                    ghostObjectInteraction.DropPickup();
                    // These are now handled by the reseter
                    //carriedObject.GetComponent<Rigidbody>().useGravity = true;
                    //carriedObject.GetComponent<Collider>().isTrigger = false;
                    carriedObject = null;
                    isCarryingObject = false;
                }
                break;
            case MovementState.AvoidingWall:
                // Pursue the closest object
                MovementUtilitySeek.SteerSeek(this, wallNormal);
                targetRotation = transform.rotation.eulerAngles;
                break;
            case MovementState.Wander:
                MovementUtilityWander.WanderForward(this);
                if (!isWanderCoRunning)
                    angleChangeCoroutine = StartCoroutine(ChangeAngle());
                isWanderCoRunning = true;
                break;
        }
    }

    /**
     * Casts a ray in front of the ghost to detect if there is wall.
     * If a wall is hit then seek a position in the direction of the normal of the wall.
     */
    private void avoidWalls()
    {
        Vector3 position = transform.position;
        RaycastHit rayHit;
        // Begin shooting the ray ahead of the player otherwise it gets caught on the ghost's mesh
        Vector3 rayOrigin = position + transform.forward * avoidWallRayOriginOffset;


        // Cast a ray to detect walls
        if (Physics.Raycast(rayOrigin, transform.TransformDirection(Vector3.forward), out rayHit, rayCastRange))
        {
            if (rayHit.collider.CompareTag("MapBounds"))
            {
                isHittingWall = true;
                wallAvoidanceTimer = 0.0f;
                rayHitPosition = rayHit.point;
                Vector3 direction = (rayHitPosition - transform.position).normalized;
                // Get normal of the wall
                Vector3 wallNormalDirection = Vector3.Cross(Vector3.up, direction);
                // Multiply the wall normal by wall avoid distance to get a point farther along the normal. We seek that point.
                wallNormal = rayHitPosition + wallNormalDirection * wallAvoidDistance;
            }
        }
        avoidWallTimer();
        Debug.DrawRay(rayOrigin, transform.TransformDirection(Vector3.forward) * rayCastRange, Color.blue);
    }

    /**
     * Timer that makes the ghost commit to moving in the wall avoiding direction for the specified time.
     */
    private void avoidWallTimer()
    {
        wallAvoidanceTimer += Time.deltaTime;
        if (wallAvoidanceTimer > wallAvoidanceDuration)
        {
            wallAvoidanceTimer = 0.0f;
            isHittingWall = false;
        }
    }

    // Used by wander to change the ghost's angle
    IEnumerator ChangeAngle()
    {
        while (movementState == MovementState.Wander)
        {
            MovementUtilityWander.changeDirection(this);
            yield return new WaitForSeconds(timeBetweenAngleChange);
        }
    }

    // Finds the object nearest to the ghost
    private ResettableObject findClosestObject(List<ResettableObject> objects)
    {
        ResettableObject closestObject = null;
        float shortestDistance = float.MaxValue;
        Vector3 currentPosition = transform.position;
        float distance; 
        foreach (ResettableObject resettableObject in objects)
        {
            distance = Vector3.Distance(currentPosition, resettableObject.transform.position);
            if (distance < shortestDistance)
            {
                closestObject = resettableObject;
                shortestDistance = distance;
            }
        }
        return closestObject;
    }

    // Add ghost's velocity as an external force on the cloth since the ghost doesn't use rigid body physics.
    private void AddVelocityToCloth()
    {
        cloth.externalAcceleration = velocity;
    }

    // Checks whether or not the ghost is touching the floor
    private bool checkIfGrounded()
    {
        //check whats at players feet, at each floorcheckers position
        foreach (Transform check in floorCheckers)
        {
            RaycastHit hit;
            // Cast rays down to see if we hit the floor. Also cast rays up to bring us back above ground.
            if (Physics.Raycast(check.position, Vector3.down, out hit, groundedRayCastDownDistance) || Physics.Raycast(check.position, Vector3.up, out hit, groundedRayCastUpDistance))
            {               
              return true;
            }
        }

        //no none of the floorchecks hit anything, we must be in the air 
        return false;
    }
}
