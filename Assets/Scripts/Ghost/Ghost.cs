using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// States used in state machine
enum MovementState { Pursuit, Wander, Return }

public class Ghost : Movable {

    // Inspector values
    public float awarenessRadius;

    // Internal values hidden from inspector   
    internal bool isCarryingObject;
    internal MovementState movementState;


    private List<ResettableObject> pickupableList;
    private ResettableObject closestResettableObject;
    private Coroutine angleChangeCoroutine;
    private bool isWanderCoRunning;

	// Use this for initialization
	void Start () {
        pickupableList = new List<ResettableObject>();

        // Set Movable values
        velocity = Vector3.zero;
        velocityMax = 5.0f;
        accelerationMax = 15.0f;
        fov = 90.0f;
        rotationSpeed = 3f;
        angleChangeLimit = 30.0f;
        timeBetweenAngleChange = 1.0f;
        targetRotation = Vector3.zero;
    }
	
	// Update is called once per frame
	void Update () {
        gatherInfo();
        updateState();
        move();

	}

    private void gatherInfo()
    {
        // Reset values
        pickupableList.Clear();

        // Gather info on nearby pickupable objects
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, awarenessRadius);
        foreach (Collider collider in hitColliders)
        {
            // TODO only add pickup object to list if it has been moved
            if (collider.tag == "Pickup")
            {
                ResettableObject pickupableObject = collider.GetComponent<ResettableObject>();
                pickupableList.Add(pickupableObject);
            }
        }

        closestResettableObject = findClosestObject(pickupableList);
    }

    // Changes the enum state of the ghost based on its environment
    private void updateState()
    {
        if(closestResettableObject != null && !isCarryingObject)
        {
            // Pursue the closest object
            movementState = MovementState.Pursuit;
        }
        else if(isCarryingObject)
        {
            // Return the object to its original location
            movementState = MovementState.Return;
        }
        else
        {
            movementState = MovementState.Wander;
        }
    }

    // Moves the ghost according to its state
    private void move()
    {
        switch(movementState)
        {
            case MovementState.Pursuit:
                // Pursue the closest object
                if(isWanderCoRunning)
                    StopCoroutine(angleChangeCoroutine);
                MovementUtilitySeek.SteerSeek(this, closestResettableObject.transform);
                break;
            case MovementState.Return:
                if (isWanderCoRunning)
                    StopCoroutine(angleChangeCoroutine);
                // Return the carried object
                break;
            case MovementState.Wander:                
                MovementUtilityWander.WanderForward(this);
                if(!isWanderCoRunning)
                    angleChangeCoroutine = StartCoroutine(ChangeAngle());
                isWanderCoRunning = true;
                break;
        }
    }

    // Used by wander to change the ghost's angle
    IEnumerator ChangeAngle()
    {
        while (movementState == MovementState.Wander)
        {
            MovementUtilityWander.changeAngle(this);
            yield return new WaitForSeconds(timeBetweenAngleChange);
        }
    }


    // Finds the object nearest to the ghost
    private ResettableObject findClosestObject(List<ResettableObject> objects)
    {
        ResettableObject closestObject = null;
        float shortestDistance = float.MaxValue;
        Vector3 currentPosition = transform.position;
        foreach (ResettableObject resettableObject in objects)
        {
            float distance = Vector3.Distance(currentPosition, resettableObject.transform.position);
            if (distance < shortestDistance)
                closestObject = resettableObject;
        }
        return closestObject;
    }
}
