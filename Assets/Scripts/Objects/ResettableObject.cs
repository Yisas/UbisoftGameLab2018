using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class ResettableObject : NetworkBehaviour
{
    // Inspector
    // Particle effect that happens hen bumping into objects
    public GameObject bamParticleEffect;
    public float powCooldown;

    //Properties
    private Vector3 ogPosition;
    private Quaternion ogRotation;
    private bool isMoved;
    private bool isOnPressurePlate;
    [SyncVar]
    private bool isBeingHeld;
    private bool usesGravity = true;
    private bool isTrigger;

    private float currentPowCooldown;

    // Distance from the original position for the object to be considered moved
    private const float distanceMovedThreshold = 5.0f;

    // Use this for initialization
    void Start()
    {
        ogPosition = transform.position;
        ogRotation = transform.rotation;
        usesGravity = GetComponent<Rigidbody>().useGravity;

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
            if (gameObject.CompareTag("Player"))
            {
                PlayerObjectInteraction playerInteraction = gameObject.GetComponent<PlayerObjectInteraction>();
                if (playerInteraction != null && playerInteraction.heldObj != null)
                    playerInteraction.DropPickup();
            }
            Reset();
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        // If a resettable object bumps into something then make a 'pow' particle effect
        if (currentPowCooldown > powCooldown && other.gameObject.layer != LayerMask.NameToLayer("Player 1") && other.gameObject.layer != LayerMask.NameToLayer("Player 2")
            && other.gameObject.layer != 2 /*ignore raycast*/ && !isBeingHeld)
        {
            Instantiate(bamParticleEffect, transform.position + transform.forward * 0.3f + transform.up, transform.rotation);
            currentPowCooldown = 0;
        }
    }

    public void Reset(bool preventRespawnEffect = false)
    {
        if(transform.tag == "Pickup" && !preventRespawnEffect)
        {
            MeshRenderer meshRenderer;
            if (GetComponent<MeshRenderer>() != null)
                meshRenderer = GetComponent<MeshRenderer>();
            else 
                meshRenderer = GetComponentInChildren<MeshRenderer>();

            Vector3 positionToSpawnAt = new Vector3(ogPosition.x, ogPosition.y - meshRenderer.bounds.extents.y, ogPosition.z);

            GManager.Instance.TriggerRespawnThrowableEffect(positionToSpawnAt);
        }

        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().useGravity = usesGravity;

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
            if (Vector3.Distance(ogPosition, transform.position) > distanceMovedThreshold)
                isMoved = true;
            else
                isMoved = false;
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
        get { return ogPosition; }
    }


}
