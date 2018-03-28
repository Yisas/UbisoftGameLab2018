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
            && other.gameObject.layer != 2 /*ignore raycast*/ && !isBeingHeld && other.collider.bounds.max.y > gameObject.GetComponent<Collider>().bounds.max.y)
        {
            Instantiate(bamParticleEffect, transform.position + transform.forward * 0.3f + transform.up, transform.rotation);
            currentPowCooldown = 0;
            
        }
        if(other.gameObject.layer != LayerMask.NameToLayer("Player 1") && other.gameObject.layer != LayerMask.NameToLayer("Player 2")
            && other.gameObject.layer != 2 /*ignore raycast*/ && !isBeingHeld && !gameObject.CompareTag("Player"))
            AkSoundEngine.PostEvent("drop", gameObject);
    }

    public void Reset(bool preventRespawnEffect = false)
    {
        Vector3 ogPosition = GManager.Instance.GetPositionOfResettableObject(idInGameManager);
        Quaternion ogRotation = GManager.Instance.GetRotationOfResettableObject(idInGameManager);

        if (transform.tag == "Pickup" && !preventRespawnEffect)
        {
            MeshRenderer meshRenderer;
            if (GetComponent<MeshRenderer>() != null)
                meshRenderer = GetComponent<MeshRenderer>();
            else
                meshRenderer = GetComponentInChildren<MeshRenderer>();

            Vector3 positionToSpawnAt = new Vector3(ogPosition.x, ogPosition.y - meshRenderer.bounds.extents.y, ogPosition.z);

            GManager.Instance.TriggerRespawnThrowableEffect(positionToSpawnAt);
            AkSoundEngine.PostEvent("item_respawn", gameObject);
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

    private void OnDestroy()
    {
        GManager.Instance.RegisterResettableObjectDestroyed(idInGameManager, GetComponent<PickupableObject>().Type);
    }

    public bool IsMoved
    {
        get
        {
            if (Vector3.Distance(GManager.Instance.GetPositionOfResettableObject(idInGameManager), transform.position) > distanceMovedThreshold)
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
        get { return GManager.Instance.GetPositionOfResettableObject(idInGameManager); }
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
