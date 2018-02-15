using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementUtilitySeek{

    public static void SteerSeek(Movable character, Transform target)
    {
        Vector3 characterPosition = character.transform.position;
        Vector3 targetPosition = target.transform.position;

        // Check if enemy is within our field of view
        Vector3 directionTowardsTarget = target.transform.position - character.transform.position;
        // Get angle between the target and our forward
        float angle = Vector3.Angle(directionTowardsTarget, character.transform.forward);

        // If the enemy is within our field of view then move towards it while rotating towards it
        if (angle < character.fov)
        {
            // Move towards the target
            Vector3 velocityDirection = (targetPosition - characterPosition).normalized;
            Vector3 seekAcceleration = velocityDirection * character.accelerationMax;
            Vector3 seekVelocity = character.velocity + seekAcceleration * Time.deltaTime;

            // If velocity exceeds max velocity then we normalize and then multiply by max velocity
            if (seekVelocity.magnitude > character.velocityMax)
            {
                seekVelocity = seekVelocity.normalized * character.velocityMax;
            }
            character.transform.position = new Vector3(characterPosition.x + seekVelocity.x * Time.deltaTime
                , characterPosition.y + seekVelocity.y * Time.deltaTime, characterPosition.z + seekVelocity.z * Time.deltaTime);
            character.velocity = seekVelocity;

            // Rotate towards the target
            Quaternion characterRotation = character.transform.rotation;
            Vector3 targetOrientation = target.transform.position - character.transform.position;
            Quaternion rotation = Quaternion.LookRotation(targetOrientation);
            rotation = new Quaternion(0, rotation.y, 0, rotation.w);
            character.transform.rotation = Quaternion.Slerp(characterRotation, rotation, Time.deltaTime * character.rotationSpeed);
            float characterYRotation = characterRotation.eulerAngles.y;
        }
    }
}
