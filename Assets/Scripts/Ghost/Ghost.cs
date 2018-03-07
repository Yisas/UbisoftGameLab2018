using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// States used in state machine
public enum MovementState { Pursuit, Wander, Return, AvoidingWall}

public class Ghost : Movable
{    // Inspector values
    public float awarenessRadius;
    public float itemDropDistance;
    public Transform floorChecks;

    // Internal values hidden from inspector   
    internal bool isCarryingObject;
    public MovementState movementState;
    public float wallAvoidDistance;

    //Scripts
    GhostObjectInteraction ghostObjectInteraction;


    private List<ResettableObject> pickupableList;
    private ResettableObject closestResettableObject;
    private ResettableObject carriedObject;
    private Coroutine angleChangeCoroutine;
    private bool isWanderCoRunning;
    private bool isHittingWall;
    private Vector3 wallNormal;
    private Vector3 rayHitPosition;
    private float wallAvoidanceTimer;
    private Cloth cloth;
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
                    carriedObject.GetComponent<Rigidbody>().useGravity = true;
                    carriedObject.GetComponent<Collider>().isTrigger = false;
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
     * Casts two rays in front of the ghost to detect if there is wall.
     * If a wall is hit then seek a position in the direction of the normal of the wall.
     */
    private void avoidWalls()
    {
        //isHittingWall = false;
        Vector3 position = transform.position;
        float range = 5f;
        RaycastHit rayHit;
        // Begin shooting the ray ahead of the player otherwise it gets caught on the ghost's mesh
        Vector3 rayOrigin = position + transform.forward * 2.5f;


        // Cast a ray to detect walls
        if (Physics.Raycast(rayOrigin, transform.TransformDirection(Vector3.forward), out rayHit, range))
        {
            if (rayHit.collider.CompareTag("MapBounds"))
            {
                isHittingWall = true;
                wallAvoidanceTimer = 0.0f;
                rayHitPosition = rayHit.point;
                Vector3 direction = (rayHitPosition - transform.position).normalized;
                Vector3 wallNormalDirection = Vector3.Cross(Vector3.up, direction);
                wallNormal = rayHitPosition + wallNormalDirection * wallAvoidDistance;
            }
        }
        avoidWallTimer();
        Debug.DrawRay(rayOrigin, transform.TransformDirection(Vector3.forward) * range, Color.blue);
    }

    /**
     * Timer that makes the ghost commit to moving in the wall avoiding direction for the specified time.
     */
    private void avoidWallTimer()
    {
        wallAvoidanceTimer += Time.deltaTime;
        float wallAvoidanceDuration = 2f;
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
        //get distance to ground, from centre of collider (where floorcheckers should be)
        float dist = 0.5f;

        //check whats at players feet, at each floorcheckers position
        foreach (Transform check in floorCheckers)
        {
            RaycastHit hit;
            // Cast rays down to see if we hit the floor. Also cast rays up to bring us back above ground.
            if (Physics.Raycast(check.position, Vector3.down, out hit, dist) || Physics.Raycast(check.position, Vector3.up, out hit, dist + 20f))
            {               
              return true;
            }
        }

        //no none of the floorchecks hit anything, we must be in the air 
        return false;
    }
}
