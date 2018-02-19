using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementUtilityArrive {

    /**
         * Moves and rotates the character towards the target position.
         */
    public static void SteerArrive(Movable character, Vector3 targetPosition)
    {
        Vector3 characterPosition = character.transform.position;

        // Get direction towards target
        Vector3 directionTowardsTarget = targetPosition - character.transform.position;
        float distance = directionTowardsTarget.magnitude;

        // Stop moving if we've reached the target
        if(distance < character.targetRadius)
        {
            return;
        }

        float targetSpeed;
        // Move at max speed if we're outside the slow down radius
        if(distance > character.slowRadius)
        {
            targetSpeed = character.velocityMax;
        }
        else
        {
            // If inside the slow down radius, then gradually slow down as we approach the target
            targetSpeed = character.velocityMax * distance / character.slowRadius;
        }

        Vector3 targetVelocity = directionTowardsTarget.normalized;
        targetVelocity *= targetSpeed;
        character.linearVelocity = targetVelocity - character.velocity;
        character.linearVelocity /= character.timeToTarget;
        if(character.linearVelocity.magnitude > character.accelerationMax)
        {
            character.linearVelocity = character.linearVelocity.normalized;
            character.linearVelocity *= character.accelerationMax;
        }


        // Move towards the target
        //Vector3 velocityDirection = (targetPosition - characterPosition).normalized;
        //Vector3 seekAcceleration = velocityDirection * character.accelerationMax;
        //Vector3 seekVelocity = character.velocity + seekAcceleration * Time.deltaTime;

        // If velocity exceeds max velocity then we normalize and then multiply by max velocity
        //if (seekVelocity.magnitude > character.velocityMax)
        //{
        //    seekVelocity = seekVelocity.normalized * character.velocityMax;
        //}
        Vector3 velocity = targetVelocity;
        if (velocity.magnitude > character.velocityMax)
        {
            velocity = velocity.normalized * character.velocityMax;
        }
        character.transform.position = new Vector3(characterPosition.x + velocity.x * Time.deltaTime
            , characterPosition.y + velocity.y * Time.deltaTime, characterPosition.z + velocity.z * Time.deltaTime);
        character.velocity = velocity;

        // Rotate towards the target
        Quaternion characterRotation = character.transform.rotation;
        Vector3 targetOrientation = targetPosition - character.transform.position;
        Quaternion rotation = Quaternion.LookRotation(targetOrientation);
        rotation = new Quaternion(0, rotation.y, 0, rotation.w);
        character.transform.rotation = Quaternion.Slerp(characterRotation, rotation, Time.deltaTime * character.rotationSpeed);
        float characterYRotation = characterRotation.eulerAngles.y;

    }
}
