using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementUtilityWander {

    public static  void WanderForward(Movable character)
    {
        Vector3 character3DPosition = character.transform.position;
        character.transform.eulerAngles = Vector3.Slerp(character.transform.eulerAngles, character.targetRotation
            ,Time.deltaTime * character.timeBetweenAngleChange);
        Vector3 forward = character.transform.TransformDirection(Vector3.forward);
        character.transform.position = new Vector3(character3DPosition.x + forward.x * Time.deltaTime
            , character3DPosition.y + forward.y * Time.deltaTime, character3DPosition.z + forward.z * Time.deltaTime);
        character.velocity = new Vector3(forward.x, forward.y, forward.z);
    }

    public static void changeAngle(Movable player)
    {
        float angle = player.transform.eulerAngles.y;
        float smallAngle = Mathf.Clamp(angle - player.angleChangeLimit, 0, 360);
        float largeAngle = Mathf.Clamp(angle + player.angleChangeLimit, 0, 360);
        angle = Random.Range(smallAngle, largeAngle);
        player.transform.eulerAngles = new Vector3(player.transform.eulerAngles.x, angle, player.transform.eulerAngles.z);
        player.targetRotation = new Vector3(0, angle, 0);
    }

}
