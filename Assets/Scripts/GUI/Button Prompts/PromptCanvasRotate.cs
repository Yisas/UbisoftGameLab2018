using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptCanvasRotate : MonoBehaviour {

    Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        //cameraTransform = playerCamera.transform;
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}
