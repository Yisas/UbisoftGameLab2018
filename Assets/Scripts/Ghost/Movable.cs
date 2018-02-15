using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movable : MonoBehaviour {
    public Vector3 velocity;
    public float accelerationMax;
    public float velocityMax;
    public float fov;
    public float rotationSpeed;
    public float angleChangeLimit;
    public float timeBetweenAngleChange;
    public Vector3 targetRotation;
}
