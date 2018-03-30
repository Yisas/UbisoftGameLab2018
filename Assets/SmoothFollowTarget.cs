using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollowTarget : MonoBehaviour {

    public Transform followTarget;
    public float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void Update () {
        transform.position = Vector3.SmoothDamp(transform.position, followTarget.position, ref velocity, smoothTime);
	}
}
