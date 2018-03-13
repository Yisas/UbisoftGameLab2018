using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movable : MonoBehaviour {
    public Vector3 velocity;
    public float accelerationMax;
    public float velocityMax;
    // Currently unimplemented
    public float fov;
    public float rotationSpeed;
    // The amount the character can rotate while wandering
    public float angleChangeLimit;
    public float timeBetweenAngleChange;
    // When seeking the target radius is where the ghost will drop items
    public float targetRadius;
    // The character will slow down when entering this radius
    public float slowRadius;
    public float timeToTarget;
    public bool isGrounded;
    public int maxHeight;
    // The speed at which the ghost will float back up after being grounded.
    public float floatSpeed;

    internal Vector3 targetRotation;
    internal Vector3 linearVelocity;
}
