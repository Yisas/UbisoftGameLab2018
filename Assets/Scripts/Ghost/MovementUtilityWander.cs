using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementUtilityWander {

    public static  void WanderForward(Movable character)
    {
        Vector3 character3DPosition = character.transform.position;
        character.transform.eulerAngles = Vector3.Slerp(character.transform.eulerAngles, character.targetRotation
            ,Time.deltaTime * character.timeBetweenAngleChange);
        Vector3 forward = character.transform.TransformDirection(Vector3.forward) * character.velocityMax;
        character.transform.position = new Vector3(character3DPosition.x + forward.x * Time.deltaTime
            , character3DPosition.y + character.velocity.y * Time.deltaTime, character3DPosition.z + forward.z * Time.deltaTime);
        character.velocity = new Vector3(forward.x, character.velocity.y, forward.z);
    }

    public static void changeDirection(Movable character)
    {
        // Change angle
        float angle = character.transform.eulerAngles.y;
        float smallAngle = Mathf.Clamp(angle - character.angleChangeLimit, 0, 360);
        float largeAngle = Mathf.Clamp(angle + character.angleChangeLimit, 0, 360);
        angle = Random.Range(smallAngle, largeAngle);
        character.targetRotation = new Vector3(0, angle, 0);

        // Change height
        int random = Random.Range(0, 2);
        if (random == 0 || character.transform.position.y > character.maxHeight)
            character.velocity.y = -character.transform.up.y / 3.0f;
        else
            character.velocity.y = character.transform.up.y / 3.0f;

        if (character.isGrounded)
            character.velocity.y = character.floatSpeed;
    }

}
