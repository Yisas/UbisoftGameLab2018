using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // GGJ addition
    public Transform backCameraPosition;
    public int playerID;
    private PlayerMove playerMove;

    public Transform target;                                    //object camera will focus on and follow
    public Vector3 targetOffset = new Vector3(0f, 3.5f, 7); //how far back should camera be from the lookTarget
    public float followSpeed = 6;                               //how fast the camera moves to its intended position
    public float inputRotationSpeed = 100;                      //how fast the camera rotates around lookTarget when you press the camera adjust buttons
    public float rotateDamping = 100;                           //how fast camera rotates to look at target
    public GameObject waterFilter;                              //object to render in front of camera when it is underwater
    public float minDistance = 5;                               //how close camera can move to player, when avoiding clipping with walls

    private Transform followTarget;
    private Vector3 defTargetOffset;
    private bool camColliding;

    //setup objects
    void Awake()
    {
        playerMove = target.GetComponent<PlayerMove>();
        followTarget = new GameObject().transform;  //create empty gameObject as camera target, this will follow and rotate around the player
        followTarget.name = "Camera Target";
        if (waterFilter)
            waterFilter.GetComponent<Renderer>().enabled = false;
        defTargetOffset = targetOffset;
        if (!target)
            Debug.LogError("'CameraFollow script' has no target assigned to it", transform);
    }

    //run our camera functions each frame
    void Update()
    {
        AdjustCamera();
        if (target)
        {
            SmoothLookAt();
            SmoothFollow();
        }
    }

    //toggle waterfilter, is camera clipping walls?
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water" && waterFilter)
            waterFilter.GetComponent<Renderer>().enabled = true;

        if (other.tag != "Water" && !other.isTrigger)
            camColliding = true;
    }

    //toggle waterfilter, is camera clipping walls?
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water" && waterFilter)
            waterFilter.GetComponent<Renderer>().enabled = false;

        if (other.tag != "Water" && !other.isTrigger)
            camColliding = false;
    }

    //rotate smoothly toward the target
    void SmoothLookAt()
    {
        Quaternion rotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotateDamping * Time.deltaTime);
    }

    //move camera when it clips walls
    void AdjustCamera()
    {
        //move cam in/out
        if (camColliding == true)
        {
            if (targetOffset.magnitude > minDistance)
                targetOffset *= 0.99f;
        }
        else
            targetOffset *= 1.01f;

        if (targetOffset.magnitude > defTargetOffset.magnitude)
            targetOffset = defTargetOffset;
    }

    //move camera smoothly toward its target
    void SmoothFollow()
    {
        if (playerMove.isRestrictedMovementToOneAxis())
        {
            transform.position = backCameraPosition.position;
            return;
        }

        //move the followTarget (empty gameobject created in awake) to correct position each frame 
        followTarget.position = target.position;

        followTarget.Translate(targetOffset, Space.Self);


        //rotate the followTarget around the target with input
        float axis = Input.GetAxis("CamHorizontal " + playerID) * inputRotationSpeed * Time.deltaTime;
        followTarget.RotateAround(target.position, Vector3.up, axis);

        //camera moves to the followTargets position
        transform.position = Vector3.Lerp(transform.position, followTarget.position, followSpeed * Time.deltaTime);
    }
}