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
    public float minDistance = 5;                               //how close camera can move to player, when avoiding clipping with walls
    public float yAxisPeakTilt = 4;                             //how high can get the tilt regarding the player (ex: looking down)
    public float yAxisBottomTilt = 2;                           //how close to the ground the camera can go, this distance is regarding player position
    public float closeUpMultiplier = 0.4f;                      //how fast or strongth are the close-ups
    public float verticalOffsetMultiplier = 10f;                //modifier so that the camera is not too low

    private Transform followTarget;
    private Vector3 defTargetOffset;
    private bool camColliding;

    private Transform lastCollided;

    //setup objects
    void Awake()
    {
        playerMove = target.GetComponent<PlayerMove>();
        followTarget = new GameObject().transform;  //create empty gameObject as camera target, this will follow and rotate around the player
        followTarget.name = "Camera Target";
        defTargetOffset = targetOffset;

        if (!target)
            Debug.LogError("'CameraFollow script' has no target assigned to it", transform);
    }

    private void Update()
    {
    }

    //run our camera functions each frame
    void LateUpdate()
    {
        AdjustCamera();
        if (target)
        {
            SmoothLookAt();
            SmoothFollow();
        }
    }

    //is camera clipping walls?
    void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            camColliding = true;
            lastCollided = other.transform;
        }
    }

    //is camera clipping walls?
    void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
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
            //If it's colliding against something and the character is not static get camera closer
            if (targetOffset.magnitude > minDistance)
            {
                //Fixing most occurences of bouncing, don't do a harsh close-up unless the character is moving
                //Also treat differently collision against floor than against all other type of collision
                if (target.GetComponent<Rigidbody>().velocity.magnitude > Vector3.zero.magnitude)
                {
                    if (lastCollided.position.y < target.position.y)
                    {
                        targetOffset *= closeUpMultiplier;
                    }

                    else
                    {
                        targetOffset *= 0.99f;
                    }
                }
            }
        }
        else
        {
            targetOffset *= 1.01f;
        }

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

        float axis2 = Input.GetAxis("CamVertical " + playerID) * inputRotationSpeed * Time.deltaTime;

        //If it haven't reached the peak
        if (transform.position.y < target.position.y + yAxisPeakTilt)
        {
            //If the camera is over the shoulders of the character
            if (transform.position.y > target.position.y + yAxisBottomTilt)
            {
                followTarget.RotateAround(target.position + transform.up * verticalOffsetMultiplier, transform.right, -axis2);
            }
            else //Camera under the shoulders threshold
            {
                if (axis2 < 0)
                {
                    followTarget.RotateAround(target.position + transform.up * verticalOffsetMultiplier, transform.right, -axis2);
                }
            }
        }
        else if (-axis2 <= 0) //It can only go down when reaches peak
        {
            followTarget.RotateAround(target.position + transform.up * verticalOffsetMultiplier, transform.right, -axis2);
        }

        transform.position = Vector3.Lerp(transform.position, followTarget.position, followSpeed * Time.deltaTime);
    }
}