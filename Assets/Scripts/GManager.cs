using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GManager : NetworkBehaviour
{
    public static float lastLevelFinishedTime;
    public static float currentLevelTime;
    public static float adaptedCurrentLevelTime;
    public static bool isPaused = false;
    public float currentLevelInvisibleTime = 120; //120secs or any set. Delay time. After this time stuff will start appearing
    public float blinkTime = 5; //200secs*numberOfBlinks to fade in everything
    public float blinkAlphaTresholdTop = 0.5f;
    public float blinkAlphaTresholdBottom = 0.05f;
    public int numberOfBlinks = 5; //flickering
    public static float lastLevelFixedTime;
    public static GManager Instance;
    public bool resetPlayers = false;
    public static string pickupLayer = "Pickup";
    public GameObject respawnPickupEffect;

    public static int invisiblePlayer1Layer = 9;
    public static int invisiblePlayer2Layer = 12;

    public static int SeeTP1NonCollidable = 20;
    public static int SeeTP2NonCollidable = 21;

    private CameraFollow cameraFollow;
    private GameObject player1;
    private GameObject player2;

    public GameObject[] spawnableInteractableObjects;
    public GameObject[] serverAuthorityCachedObjects = new GameObject[4];
    public GameObject[] clientAuthorityCachedObjects = new GameObject[4];

    private int localPlayerID;
    private bool clientsConnected = false;

    private List<ResettableObject> resettableObjects = new List<ResettableObject>();
    private List<Vector3> positionsOfResettableObjects = new List<Vector3>();
    private List<Quaternion> rotationsOfResettableObjects = new List<Quaternion>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        lastLevelFinishedTime = currentLevelTime;
        currentLevelTime = 0;

        if (lastLevelFinishedTime == 0) return;

        float extraPercentageTime = (lastLevelFinishedTime - lastLevelFixedTime) / lastLevelFinishedTime;
        if (extraPercentageTime < 0)
        {
            extraPercentageTime = 0;
        }
        adaptedCurrentLevelTime = currentLevelInvisibleTime + (currentLevelInvisibleTime * extraPercentageTime);
        lastLevelFixedTime = currentLevelInvisibleTime;
    }

    /// <summary>
    /// Should only be called from server. Will also make RpcCommand to cache client-side
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameObject CacheNewObject(PickupableObject.PickupableType type, bool isArespawn = false, bool respawnTriggerdByServer = true)
    {
        if (!isServer)
        {
            Debug.LogError("Should only be called from server");
        }

        GameObject modelToSpawn = spawnableInteractableObjects[(int)type];

        if (!isArespawn || (isArespawn && respawnTriggerdByServer))
        {
            serverAuthorityCachedObjects[(int)type] = Instantiate(modelToSpawn, new Vector3(0, 200 * ((int)type + 1), 0), modelToSpawn.transform.rotation);
            serverAuthorityCachedObjects[(int)type].GetComponent<Rigidbody>().useGravity = (false);
            serverAuthorityCachedObjects[(int)type].GetComponent<Rigidbody>().isKinematic = (false);
            serverAuthorityCachedObjects[(int)type] = serverAuthorityCachedObjects[(int)type];
            NetworkServer.Spawn(serverAuthorityCachedObjects[(int)type]);
        }

        if (!isArespawn || (isArespawn && !respawnTriggerdByServer))
        {
            clientAuthorityCachedObjects[(int)type] = Instantiate(modelToSpawn, new Vector3(0, 210 * ((int)type + 1), 0), modelToSpawn.transform.rotation);
            clientAuthorityCachedObjects[(int)type].GetComponent<Rigidbody>().useGravity = (false);
            clientAuthorityCachedObjects[(int)type].GetComponent<Rigidbody>().isKinematic = (false);
            NetworkServer.Spawn(clientAuthorityCachedObjects[(int)type]);
            SetPlayerAuthorityToHeldObject(GetNonLocalPlayer().GetComponent<NetworkIdentity>(), clientAuthorityCachedObjects[(int)type].GetComponent<NetworkIdentity>());
            clientAuthorityCachedObjects[(int)type] = clientAuthorityCachedObjects[(int)type];

            RpcCacheNewObject(clientAuthorityCachedObjects[(int)type], type);
        }

        return serverAuthorityCachedObjects[(int)type];
    }

    [ClientRpc]
    private void RpcCacheNewObject(GameObject go, PickupableObject.PickupableType type)
    {
        if (isServer)
            return;

        clientAuthorityCachedObjects[(int)type] = go;
        clientAuthorityCachedObjects[(int)type].GetComponent<Rigidbody>().useGravity = false;
    }

    public GameObject GetCachedObject(PickupableObject.PickupableType type)
    {
        if (isServer)
            return serverAuthorityCachedObjects[(int)type];
        else
            return clientAuthorityCachedObjects[(int)type];
    }

    /// <summary>
    /// Should only be called from server
    /// </summary>
    /// <param name="type"></param>
    public void CachedObjectWasUsed(PickupableObject.PickupableType type, bool usedByServer)
    {
        if (isServer)
        {
            serverAuthorityCachedObjects[(int)type] = CacheNewObject(type, true, usedByServer);
        }
        else
        {
            Debug.LogError("CacheObjectWas used called from server");
        }
    }

    private void Update()
    {
        if (!isPaused)
        {
            currentLevelTime += Time.deltaTime;
        }

        if (isServer && !clientsConnected && GetComponent<NetworkIdentity>().observers.Count == 2)
        {
            clientsConnected = true;
            FindPlayers();

            // Register resettable objects positions
            foreach (ResettableObject ro in GameObject.FindObjectsOfType<ResettableObject>())
            {
                RegisterResettableObject(ro);
            }

            RpcRegisterAllResettableObjects();

            // Spawn cached objects
            for (int i = 0; i < spawnableInteractableObjects.Length; i++)
                serverAuthorityCachedObjects[i] = CacheNewObject((PickupableObject.PickupableType)i);
        }
    }

    [ClientRpc]
    private void RpcRegisterAllResettableObjects()
    {
        if (!isServer)
        {
            // Register resettable objects positions
            foreach (ResettableObject ro in GameObject.FindObjectsOfType<ResettableObject>())
            {
                RegisterResettableObject(ro);
            }
        }
    }

    public void RegisterResettableObject(ResettableObject ro)
    {
        ro.id = resettableObjects.Count;
        resettableObjects.Add(ro);
        positionsOfResettableObjects.Add(ro.transform.position);
        rotationsOfResettableObjects.Add(ro.transform.rotation);
    }

    public void RegisterResettableObjectDestroyed(int id, PickupableObject.PickupableType type)
    {
        // Cached object should become the resettable object reference
        if (isServer)
        {
            serverAuthorityCachedObjects[(int)type].GetComponent<ResettableObject>().id = id;
            resettableObjects[id] = serverAuthorityCachedObjects[(int)type].GetComponent<ResettableObject>();
        }
        else
        {
            clientAuthorityCachedObjects[(int)type].GetComponent<ResettableObject>().id = id;
            resettableObjects[id] = clientAuthorityCachedObjects[(int)type].GetComponent<ResettableObject>();
        }
    }

    public void DeRegisterResettableObject(ResettableObject ro)
    {
        resettableObjects.Remove(ro);
    }

    // When the player falls into lava with a carried object, the cached one should be reset and a new one should be cached
    public void ResetCachedObject(PickupableObject.PickupableType type)
    {
        if (isServer)
        {
            serverAuthorityCachedObjects[(int)type].GetComponent<ResettableObject>().Reset();
            serverAuthorityCachedObjects[(int)type] = CacheNewObject(type);
        }
    }

    public Vector3 GetPositionOfResettableObject(int id)
    {
        return positionsOfResettableObjects[id];
    }

    public Quaternion GetRotationOfResettableObject(int id)
    {
        return rotationsOfResettableObjects[id];
    }

    public void ResetAllResetableObjects(bool resetPlayers)
    {
        foreach (ResettableObject ro in GameObject.FindObjectsOfType<ResettableObject>())
        {
            if (!resetPlayers && ro.gameObject.tag == "Player")
            {
                continue;
            }
            ro.Reset();
        }
    }

    public void setInvisibleToVisibleWorld(int layer, int layerNoSecretToPlayer)
    {
        //Call this in a method in camera thing where filtering
        GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //will return an array of all GameObjects in the scene
        foreach (GameObject go in gos)
        {
            if (go.layer == layer && go.GetComponent<Renderer>()) //9: Invisible player 1 or 12: Invisible player 2
            {
                go.AddComponent<InvisibleToVisible2>();
                go.GetComponent<InvisibleToVisible2>().regressionTresholdBottom = blinkAlphaTresholdBottom;
                go.GetComponent<InvisibleToVisible2>().regressionTresholdTop = blinkAlphaTresholdTop;
                go.GetComponent<InvisibleToVisible2>().numberOfRegressions = numberOfBlinks;
                go.GetComponent<InvisibleToVisible2>().delayToFadeInTime = currentLevelInvisibleTime;
                go.GetComponent<InvisibleToVisible2>().FadeInTimeout = blinkTime;
            }
            else if (go.layer == layerNoSecretToPlayer)
            {
                go.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    /// <summary>
    /// After player has been spawned, setup references on environmental objects
    /// </summary>
    public void NetworkingLevelReferencesSetup(PlayerMove playerSpawned)
    {
        /*if (playerSpawned.isLocalPlayer)
        {
            Debug.Log("HELLO?");
            Destroy(playerSpawned.GetComponentInChildren<PromptCanvasRotate>().gameObject);
            Destroy(playerSpawned.GetComponentInChildren<ButtonPromptsNetworked>());
            return;
        }*/
        ButtonPromptsNetworked[] buttonPrompts = GameObject.FindObjectsOfType<ButtonPromptsNetworked>();
        foreach (ButtonPromptsNetworked bp in buttonPrompts)
        {
            bp.NetworkPlayerPromptReferenceStart(playerSpawned.transform.GetComponentInChildren<Canvas>());
        }
    }

    public void TriggerRespawnThrowableEffect(Vector3 position)
    {
        GameObject.Instantiate(respawnPickupEffect, position, Quaternion.Euler(90, 0, 0));
    }

    private void FindPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerMove playerMove = player.GetComponent<PlayerMove>();
            if (playerMove.PlayerID == 1)
            {
                player1 = player;

                if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                    localPlayerID = 1;
            }
            else if (playerMove.PlayerID == 2)
            {
                player2 = player;

                if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
                    localPlayerID = 2;
            }
        }
    }

    //--------------------------- NETWORKING HACKS ------------------------------------
    public void OverridePlayer(int playerID)
    {
        if (!player1 || !player2)
            FindPlayers();

        Debug.Log("Overriding player " + playerID);

        if (playerID == 1)
        {
            player1.SetActive(false);
        }
        else if (playerID == 2)
        {
            player2.SetActive(false);
        }
    }

    public void OverrideCameraFollow(int idOfPlayerOverriding)
    {
        if (!cameraFollow)
            cameraFollow = Camera.main.GetComponent<CameraFollow>();

        if (idOfPlayerOverriding == 1)
            cameraFollow.target = player1.GetComponent<PlayerObjectInteraction>().fakeObjects[(int)PickupableObject.PickupableType.Player].transform;
        else
            cameraFollow.target = player2.GetComponent<PlayerObjectInteraction>().fakeObjects[(int)PickupableObject.PickupableType.Player].transform;
    }

    public void RestorePlayerOverride(Vector3 positionToRestoreTo, Quaternion rotationToRestoreTo, int idOfPlayerToRestore)
    {
        if (!player1 || !player2)
            FindPlayers();

        Debug.Log("Restoring player " + idOfPlayerToRestore);

        if (idOfPlayerToRestore == 1)
        {
            player1.transform.position = positionToRestoreTo;
            player1.transform.rotation = rotationToRestoreTo;
            player1.SetActive(true);
        }
        else if (idOfPlayerToRestore == 2)
        {
            player2.transform.position = positionToRestoreTo;
            player2.transform.rotation = rotationToRestoreTo;
            player2.SetActive(true);
        }
    }

    public void RestoreCameraFollow(int idOfPlayerToRestore)
    {
        if (!player1 || !player2)
            FindPlayers();

        if (!cameraFollow)
            cameraFollow = Camera.main.GetComponent<CameraFollow>();

        if (idOfPlayerToRestore == 1)
        {
            cameraFollow.target = player1.transform;
        }
        else if (idOfPlayerToRestore == 2)
        {
            cameraFollow.target = player2.transform;
        }
    }

    public void ForceHideFakeObject(int playerID)
    {
        if (!player1 || !player2)
            FindPlayers();

        if (playerID == 1)
        {
            player1.GetComponent<PlayerObjectInteraction>().HideFakeObject();
        }
        else
        {
            player2.GetComponent<PlayerObjectInteraction>().HideFakeObject();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientToReceiveAuthority">Client to receive authority of the object</param>
    /// <param name="netIdentityOfObj">Network identity of the gameObject that will have its player auth modified</param>
    public void SetPlayerAuthorityToHeldObject(NetworkIdentity clientToReceiveAuthority, NetworkIdentity netIdentityOfObj)
    {
        Debug.Log("Changing authority of " + netIdentityOfObj.gameObject.name + " to " + clientToReceiveAuthority.GetComponent<PlayerMove>().PlayerID);

        // Remove prior ownership if necessary
        // TODO: consider removing authority (back to server) after letting go of heldObj
        if (netIdentityOfObj.clientAuthorityOwner != null)
            if (netIdentityOfObj.clientAuthorityOwner != clientToReceiveAuthority.connectionToClient)
            {
                NetworkIdentity netIdentityToRemove = null;

                if (clientToReceiveAuthority.isLocalPlayer)
                {
                    netIdentityToRemove = GetNonLocalPlayer().GetComponent<NetworkIdentity>();
                }
                else
                {
                    netIdentityToRemove = GetLocalPlayer().GetComponent<NetworkIdentity>();
                }

                netIdentityOfObj.RemoveClientAuthority(netIdentityToRemove.GetComponent<NetworkIdentity>().connectionToClient);
            }

        netIdentityOfObj.AssignClientAuthority((clientToReceiveAuthority.connectionToClient));

    }

    [Command]
    public void CmdSetPlayerAuthorityToHeldObject(NetworkIdentity clientToReceiveAuthority, NetworkIdentity netIdentityOfObj)
    {
        SetPlayerAuthorityToHeldObject(clientToReceiveAuthority, netIdentityOfObj);
    }

    /// <summary>
    /// Server will receive authority of the netidentity passed to it
    /// </summary>
    /// <param name="netIdentityOfObj"></param>
    public void AssignPlayerAuthorityToServer(NetworkIdentity netIdentityOfObj)
    {
        if (!isServer)
        {
            CmdAssignPlayerAuthorityToServer(netIdentityOfObj);
        }

        SetPlayerAuthorityToHeldObject(GetLocalPlayer().GetComponent<NetworkIdentity>(), netIdentityOfObj);
    }

    [Command]
    public void CmdAssignPlayerAuthorityToServer(NetworkIdentity netIdentityOfObj)
    {
        AssignPlayerAuthorityToServer(netIdentityOfObj);
    }

    public GameObject GetLocalPlayer()
    {
        if (!player1 || !player2)
            FindPlayers();

        return (localPlayerID == 1 ? player1 : player2);
    }

    public GameObject GetNonLocalPlayer()
    {
        if (!player1 || !player2)
            FindPlayers();

        return (localPlayerID == 1 ? player2 : player1);
    }

    public GameObject GetPlayer(int playerID)
    {
        if (!player1 || !player2)
            FindPlayers();

        return (playerID == 1 ? player1 : player2);
    }

    public int LocalPlayerID
    {
        get
        {
            if (!player1 || !player2)
                FindPlayers();

            return localPlayerID;
        }
    }
}
