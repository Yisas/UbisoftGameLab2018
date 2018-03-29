using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class ResettableObject : NetworkBehaviour
{
    // Inspector
    // Particle effect that happens hen bumping into objects
    public GameObject bamParticleEffect;
    public float powCooldown;

    [SyncVar]
    [SerializeField]
    private int idInGameManager;

    //Properties
    private bool isMoved;
    private bool isOnPressurePlate;
    [SyncVar]
    private bool isBeingHeld;
    private bool isTrigger;

    private float currentPowCooldown;

    [SyncVar]
    public bool wasSpawnedByGameManager = false;

    [SyncVar]
    private Vector3 originalPosition;
    [SyncVar]
    private Quaternion originalRotation;

    [SyncVar]
    public bool hasOriginalPosition = false;
    [SyncVar]
    public bool hasOriginalRotation = false;

    // Distance from the original position for the object to be considered moved
    private const float distanceMovedThreshold = 5.0f;

    // Use this for initialization
    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col)
            isTrigger = GetComponent<Collider>().isTrigger;
    }

    private void Update()
    {
        if (currentPowCooldown < powCooldown)
            currentPowCooldown += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If a resettable object touches the deadzone then it will be reset to its original position.
        if (other.gameObject.layer == LayerMask.NameToLayer("Deadzone"))
        {
            Reset();
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        // If a resettable object bumps into something then make a 'pow' particle effect
        if (currentPowCooldown > powCooldown && other.gameObject.layer != LayerMask.NameToLayer("Player 1") && other.gameObject.layer != LayerMask.NameToLayer("Player 2")
            && other.gameObject.layer != 2 /*ignore raycast*/ && !isBeingHeld && other.collider.bounds.max.y > gameObject.GetComponent<Collider>().bounds.max.y)
        {
            Instantiate(bamParticleEffect, transform.position + transform.forward * 0.3f + transform.up, transform.rotation);
            currentPowCooldown = 0;
        }
        if (other.gameObject.layer != LayerMask.NameToLayer("Player 1") && other.gameObject.layer != LayerMask.NameToLayer("Player 2")
            && other.gameObject.layer != 2 /*ignore raycast*/ && !isBeingHeld && !gameObject.CompareTag("Player"))
            AkSoundEngine.PostEvent("drop", gameObject);
    }

    public void Reset(bool preventRespawnEffect = false)
    {
        Vector3 ogPosition;
        Quaternion ogRotation;

        if (tag != "Player")
        {
            ogPosition = originalPosition;
            ogRotation = originalRotation;
        }
        else
        {
            ogPosition = GManager.Instance.GetOriginalPositionOfPlayer();
            ogRotation = GManager.Instance.GetOriginalRotationOfPlayer();
        }

        if (transform.tag == "Pickup" && !preventRespawnEffect)
        {
            MeshRenderer meshRenderer;
            if (GetComponent<MeshRenderer>() != null)
                meshRenderer = GetComponent<MeshRenderer>();
            else
                meshRenderer = GetComponentInChildren<MeshRenderer>();

            Vector3 positionToSpawnAt = new Vector3(ogPosition.x, ogPosition.y - meshRenderer.bounds.extents.y, ogPosition.z);

            GManager.Instance.TriggerRespawnThrowableEffect(positionToSpawnAt);
            AkSoundEngine.PostEvent("vase_generate", gameObject);
        }

        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        // If a torch, turn off gravity
        PickupableObject pickup = GetComponent<PickupableObject>();
        if (pickup)
            GetComponent<Rigidbody>().useGravity = (pickup.Type == PickupableObject.PickupableType.Torch) ? false : true;

        Collider col = GetComponent<Collider>();
        if (col)
            col.isTrigger = isTrigger;

        transform.position = ogPosition;
        transform.rotation = ogRotation;
    }

    public bool IsMoved
    {
        get
        {
            if (hasOriginalPosition)
            {
                if (Vector3.Distance(originalPosition, transform.position) > distanceMovedThreshold)
                    isMoved = true;
                else
                    isMoved = false;
            }
            else
            {
                return false;
            }
            return isMoved;
        }
    }

    public bool IsOnPressurePlate
    {
        get { return isOnPressurePlate; }
        set { isOnPressurePlate = value; }
    }

    public bool IsBeingHeld
    {
        get { return isBeingHeld; }
        set { isBeingHeld = value; }
    }

    public Vector3 OriginalPosition
    {
        get { return originalPosition; }

        set
        {
            if (!hasOriginalPosition)
            {
                hasOriginalPosition = true;
                originalPosition = value;

                if (isServer)
                {
                    RpcSetPosition(value);
                }
                else
                {
                    CmdSetPosition(value);
                }
            }
        }
    }

    [Command]
    private void CmdSetPosition(Vector3 value)
    {
        hasOriginalPosition = true;
        originalPosition = value;
    }

    private void RpcSetPosition(Vector3 value)
    {
        if (!isServer)
        {
            hasOriginalPosition = true;
            originalPosition = value;
        }
    }

    public Quaternion OriginalRotation
    {
        get { return originalRotation; }

        set
        {
            if (!hasOriginalRotation)
            {
                hasOriginalRotation = true;
                originalRotation = value;

                if (isServer)
                {
                    RpcSetRotation(value);
                }
                else
                {
                    CmdSetRotation(value);
                }
            }
        }
    }

    [Command]
    private void CmdSetRotation(Quaternion value)
    {
        hasOriginalRotation = true;
        originalRotation = value;
    }

    private void RpcSetRotation(Quaternion value)
    {
        if (!isServer)
        {
            hasOriginalRotation = true;
            originalRotation = value;
        }
    }

    public int ID
    {
        get { return idInGameManager; }

        set
        {
            idInGameManager = value;

            if (!isServer)
                CmdSetIdInGameManager(value);
        }
    }

    [Command]
    private void CmdSetIdInGameManager(int value)
    {
        idInGameManager = value;
    }
}
