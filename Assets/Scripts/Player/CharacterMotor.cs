﻿using UnityEngine;
using UnityEngine.Networking;

//this class holds movement functions for a rigidbody character such as player, enemy, npc..
//you can then call these functions from another script, in order to move the character
[RequireComponent(typeof(Rigidbody))]
public class CharacterMotor : NetworkBehaviour 
{
	[HideInInspector]
	public Vector3 currentSpeed;
	
	public float DistanceToTarget;
	
	void Awake()
	{
		GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		
		if(GetComponent<Collider>().material.name == "Default (Instance)")
		{
			PhysicMaterial pMat = new PhysicMaterial();
			pMat.name = "Frictionless";
			pMat.frictionCombine = PhysicMaterialCombine.Multiply;
			pMat.bounceCombine = PhysicMaterialCombine.Multiply;
			pMat.dynamicFriction = 0f;
			pMat.staticFriction = 0f;
			GetComponent<Collider>().material = pMat;
			Debug.LogWarning("No physics material found for CharacterMotor, a frictionless one has been created and assigned", transform);
		}
	}
	//move rigidbody to a target and return the bool "have we arrived?"
	public bool MoveTo(Vector3 destination, float acceleration, float stopDistance, bool ignoreY)
	{
        if (!isLocalPlayer)
        {
            Debug.LogWarning("Move operation being called from non-local player instance. Is this intended behavior?");
            return false;
        }

		Vector3 relativePos = (destination - transform.position);
		if(ignoreY)
			relativePos.y = 0;
		
		DistanceToTarget = relativePos.magnitude;

        if (isLocalPlayer)
        {
            if (isServer)
            {
                RpcSendDistanceToTarget(DistanceToTarget);
            }
            else
            {
                CmdSendDistanceToTarget(DistanceToTarget);
            }
        }

		if (DistanceToTarget <= stopDistance)
			return true;
		else
			GetComponent<Rigidbody>().AddForce(relativePos.normalized * acceleration * Time.deltaTime, ForceMode.VelocityChange);
			return false;
	}
	
	//rotates rigidbody to face its current velocity
	public void RotateToVelocity(float turnSpeed, bool ignoreY)
	{	
		Vector3 dir;
		if(ignoreY)
			dir = new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.z);
		else
			dir = GetComponent<Rigidbody>().velocity;
		
		if (dir.magnitude > 0.1)
		{
			Quaternion dirQ = Quaternion.LookRotation (dir);
			Quaternion slerp = Quaternion.Slerp (transform.rotation, dirQ, dir.magnitude * turnSpeed * Time.deltaTime);
			GetComponent<Rigidbody>().MoveRotation(slerp);
		}
	}
	
	//rotates rigidbody to a specific direction
	public void RotateToDirection(Vector3 lookDir, float turnSpeed, bool ignoreY)
	{
		Vector3 characterPos = transform.position;
		if(ignoreY)
		{
			characterPos.y = 0;
			lookDir.y = 0;
		}
		
		Vector3 newDir = lookDir - characterPos;
		Quaternion dirQ = Quaternion.LookRotation (newDir);
		Quaternion slerp = Quaternion.Slerp (transform.rotation, dirQ, turnSpeed * Time.deltaTime);
		GetComponent<Rigidbody>().MoveRotation (slerp);
	}
	
	// apply friction to rigidbody, and make sure it doesn't exceed its max speed
	public void ManageSpeed(float deceleration, float maxSpeed, bool ignoreY)
	{	
		currentSpeed = GetComponent<Rigidbody>().velocity;
		if (ignoreY)
			currentSpeed.y = 0;
		
		if (currentSpeed.magnitude > 0)
		{
			GetComponent<Rigidbody>().AddForce ((currentSpeed * -1) * deceleration * Time.deltaTime, ForceMode.VelocityChange);
			if (GetComponent<Rigidbody>().velocity.magnitude > maxSpeed)
				GetComponent<Rigidbody>().AddForce ((currentSpeed * -1) * deceleration * Time.deltaTime, ForceMode.VelocityChange);
		}
	}

    [Command]
    public void CmdSendDistanceToTarget(float distanceToTarget)
    {
        DistanceToTarget = distanceToTarget;
    }

    [ClientRpc]
    public void RpcSendDistanceToTarget(float distanceToTarget)
    {
        DistanceToTarget = distanceToTarget;
    }
}

/* NOTE: ManageSpeed does a similar job to simply increasing the friction property of a rigidbodies "physics material"
 * but this is unpredictable and can result in sluggish controls and things like gripping against walls as you walk/falls past them
 * it's not ideal for gameplay, and so we use 0 friction physics materials and control friction ourselves with the ManageSpeed function instead */

/* NOTE: when you use MoveTo, make sure the stopping distance is something like 0.3 and not 0
 * if it is 0, the object is likely to never truly reach the destination, and it will jitter on the spot as it
 * attempts to move toward the destination vector but overshoots it each frame
 */