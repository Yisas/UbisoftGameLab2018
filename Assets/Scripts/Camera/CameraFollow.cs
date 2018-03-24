using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // GGJ addition
    public int playerID;
    private PlayerMove playerMove;

    private Transform target;                                   //object camera will focus on and follow
    public Vector3 targetOffset = new Vector3(0f, 3.5f, 7);     //how far back should camera be from the lookTarget
    public float followSpeed = 6;                               //how fast the camera moves to its intended position
    public float inputRotationSpeed = 100;                      //how fast the camera rotates around lookTarget when you press the camera adjust buttons
    public float rotateDamping = 100;                           //how fast camera rotates to look at target
    public float minDistance = 5;                               //how close camera can move to player, when avoiding clipping with walls
    public float yAxisPeakTilt = 4;                             //how high can get the tilt regarding the player (ex: looking down)
    private float defyAxisPeakTilt;
    public float yAxisBottomTilt = 2;                           //how close to the ground the camera can go, this distance is regarding player position
    public float closeUpMultiplier = 0.4f;                      //how fast or strongth are the close-ups
    public float verticalOffsetMultiplier = 10f;                //modifier so that the camera is not too low
    private Transform followTarget;
    private Vector3 defTargetOffset;

    private float startingTargetY;
    private Transform backCameraPosition;                       // Over the shoulder position of the camera for when the player is push/pulling blocks
    public string[] layersToSeeThrough;
    private int layerMaskSeeThrough;

    private bool isZoomingIn;
    private bool isZoomingOut;
    private Vector3 residualVectorY;
    public bool reboundBack;
    public Vector3 zoomingOutTarget = new Vector3(0, -1, 0);
    public Vector3 zoomingInTarget = new Vector3(0, 1, 0);
    public float verticalSensitivity = 0.95f;

    public float zoomInMultiplier = 0.99f;
    public float zoomOutMultiplier = 1.1f;

    // State variables
    private bool camColliding;

    public void StartFollowingPlayer(PlayerMove playerMove, Transform backCameraPosition)
    {
        if (target)
            return;

        target = playerMove.transform;
        this.backCameraPosition = backCameraPosition;
        this.playerMove = playerMove;
        followTarget = new GameObject().transform;  //create empty gameObject as camera target, this will follow and rotate around the player
        followTarget.name = "Camera Target";
        defTargetOffset = targetOffset;
        startingTargetY = target.position.y;

        foreach (string layer in layersToSeeThrough)
            layerMaskSeeThrough |= 1 << LayerMask.NameToLayer(layer);

        defyAxisPeakTilt = yAxisPeakTilt;

        if (!target)
            Debug.LogError("'CameraFollow script' has no target assigned to it", transform);

    }

    void Update()
    {
        if (target)
        {
            SmoothLookAt();
            SmoothFollow();

            RaycastHit[] hits;

            //Calculate distance to player with small offset
            float distanceToPLayer = Vector3.Distance(transform.position, target.position) * 1.1f;

            // you can also use CapsuleCastAll()
            // TODO: setup your layermask it improve performance and filter your hits.
            hits = Physics.RaycastAll(transform.position - transform.forward, transform.forward, distanceToPLayer, layerMaskSeeThrough);
            foreach (RaycastHit hit in hits)
            {
                if (startingTargetY > hit.point.y
                    && target.position.y > hit.point.y) continue;

                Renderer R = hit.collider.GetComponent<Renderer>();

                if (R != null)
                    if (R.tag != GManager.pickupLayer)
                        setAutoTransparentOnObject(R.gameObject);

                Renderer[] Rchilds = hit.collider.GetComponentsInChildren<Renderer>();

                if (Rchilds != null)
                {
                    foreach (Renderer child in Rchilds)
                    {
                        if (child.tag != GManager.pickupLayer)
                        {
                            setAutoTransparentOnObject(child.gameObject);
                        }
                    }
                }
                else continue; // no renderer attached? go to next hit
                               // TODO: maybe implement here a check for GOs that should not be affected like the player


                if (Rchilds != null)
                {
                    foreach (Renderer child in Rchilds)
                        setAutoTransparentOnObject(child.gameObject);
                }
                else continue; // no renderer attached? go to next hit
                               // TODO: maybe implement here a check for GOs that should not be affected like the player
            }
        }
    }

    void setAutoTransparentOnObject(GameObject alphaObject)
    {
        if (alphaObject.GetComponent<InvisibleToVisible>()) return;
        if (alphaObject.GetComponent<InvisibleToVisible2>()) return;
        if (alphaObject.GetComponent<VisibleToInvisible>()) return;
        
        AutoTransparent AT = alphaObject.GetComponent<AutoTransparent>();
 

        if (AT == null) // if no script is attached, attach one
        {
            AT = alphaObject.gameObject.AddComponent<AutoTransparent>();
        }
        AT.BeTransparent(); // get called every frame to reset the falloff
    }

    //run our camera functions each frame
    void LateUpdate()
    {
        if (target)
        {
            AdjustCamera();
            SmoothLookAt();
            SmoothFollow();
        }
    }

    //rotate smoothly toward the target
    void SmoothLookAt()
    {
        if (playerMove.isRestrictToBackCamera())
        {
            transform.LookAt(target);
            return;
        }

        Quaternion rotation = Quaternion.LookRotation(target.position + residualVectorY - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotateDamping * Time.deltaTime);


        if (!isZoomingIn)
        {
            if (reboundBack)
                residualVectorY = Vector3.Slerp(residualVectorY, Vector3.zero, Time.deltaTime);
        }
        else if (isZoomingOut)
        {
            residualVectorY = Vector3.Slerp(residualVectorY, zoomingOutTarget, Time.deltaTime);
        }
        else
        {
            residualVectorY = Vector3.Slerp(residualVectorY, zoomingInTarget, Time.deltaTime);
        }
    }

    //move camera when it clips walls
    void AdjustCamera()
    {
        if ((!isZoomingIn && reboundBack) || isZoomingOut)
        {
            targetOffset *= zoomOutMultiplier;
            yAxisPeakTilt *= zoomOutMultiplier;
        }

        if (targetOffset.magnitude > defTargetOffset.magnitude)
        {
            targetOffset = defTargetOffset;
        }

        if (yAxisPeakTilt > defyAxisPeakTilt)
            yAxisPeakTilt = defyAxisPeakTilt;
    }

    //move camera smoothly toward its target
    void SmoothFollow()
    {
        if (playerMove.isRestrictToBackCamera())
        {
            transform.position = backCameraPosition.position;
            return;
        }

        //move the followTarget (empty gameobject created in awake) to correct position each frame 
        followTarget.position = target.position;

        followTarget.Translate(targetOffset, Space.Self);

        //rotate the followTarget around the target with input
        float axis = Input.GetAxis("CamHorizontal") * inputRotationSpeed * Time.deltaTime;
        followTarget.RotateAround(target.position, Vector3.up, axis);

        float axis2 = Input.GetAxis("CamVertical") * inputRotationSpeed * Time.deltaTime * verticalSensitivity;

        //If it haven't reached the peak
        if (transform.position.y < target.position.y + yAxisPeakTilt)
        {
            //If the camera is over the shoulders of the character
            if (transform.position.y > target.position.y + yAxisBottomTilt)
            {
                followTarget.RotateAround(target.position + transform.up * verticalOffsetMultiplier + residualVectorY, transform.right, -axis2);
                isZoomingOut = true;
            }
            else //Camera under the shoulders! threshold
            {
                if (axis2 < 0) //only if it's going up
                {
                    followTarget.RotateAround(target.position + transform.up * verticalOffsetMultiplier + residualVectorY, transform.right, -axis2);
                    isZoomingOut = true;
                }
            }

            if (axis2 > 0)
            {
                if (targetOffset.magnitude > defTargetOffset.magnitude * 0.20)
                {
                    targetOffset *= zoomInMultiplier;
                    yAxisPeakTilt *= zoomInMultiplier;
                    Quaternion rotation = Quaternion.LookRotation(target.position + residualVectorY - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotateDamping * Time.deltaTime);
                    isZoomingIn = true;
                    isZoomingOut = false;
                }
            }
            else isZoomingIn = false;
        }
        else if (-axis2 <= 0) //It can only go down when reaches peak
        {
            followTarget.RotateAround(target.position + transform.up * verticalOffsetMultiplier, transform.right, -axis2);
        }

        Vector3 nextFramePosition = Vector3.Lerp(transform.position, followTarget.position, followSpeed * Time.deltaTime);
        //transform.position = futurePosition;
        transform.position = nextFramePosition;
    }
}