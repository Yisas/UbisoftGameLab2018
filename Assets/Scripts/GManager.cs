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
    public float currentLevelFixedTime = 120; //120secs or any set. Delay time. After this time stuff will start appearing
    public float timeToMakeEverythingVisible = 200; //200secs to fade in everything
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

    public GameObject vase;
    public GameObject serverAuthorityCachedVase;
    public GameObject clientAuthorityCachedVase;

    private int localPlayerID;
    private bool clientsConnected = false;

    private List<ResettableObject> resettableObjects = new List<ResettableObject>();
    private List<Vector3> positionsOfResettableObjects = new List<Vector3>();
    private List<Quaternion> rotationsOfResettableObjects = new List<Quaternion>();
    private int lastResettableObjectDestroyed;
    private bool restoringResettableObject = false;

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
        adaptedCurrentLevelTime = currentLevelFixedTime + (currentLevelFixedTime * extraPercentageTime);
        lastLevelFixedTime = currentLevelFixedTime;
    }

    /// <summary>
    /// Should only be called from server
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameObject CacheNewObject(PickupableObject.PickupableType type)
    {
        if (!isServer)
        {
            Debug.LogError("Should only be called from server");
        }

        switch (type)
        {
            case PickupableObject.PickupableType.Vase:
                serverAuthorityCachedVase = Instantiate(vase, new Vector3(0, 1000, 0), vase.transform.rotation);
                serverAuthorityCachedVase.GetComponent<Rigidbody>().useGravity = (false);
                NetworkServer.Spawn(serverAuthorityCachedVase);

                clientAuthorityCachedVase = Instantiate(vase, new Vector3(0, 1010, 0), vase.transform.rotation);
                clientAuthorityCachedVase.GetComponent<Rigidbody>().useGravity = (false);
                NetworkServer.Spawn(clientAuthorityCachedVase);
                SetPlayerAuthorityToHeldObject(GetNonLocalPlayer().GetComponent<NetworkIdentity>(), clientAuthorityCachedVase.GetComponent<NetworkIdentity>());

                if (isServer)
                {
                    RpcCacheNewObject(clientAuthorityCachedVase, type);
                }

                return serverAuthorityCachedVase;
        }

        return null;
    }

    [ClientRpc]
    private void RpcCacheNewObject(GameObject go, PickupableObject.PickupableType type)
    {
        if (isServer)
            return;

        switch (type)
        {
            case PickupableObject.PickupableType.Vase:
                clientAuthorityCachedVase = go;
                clientAuthorityCachedVase.GetComponent<Rigidbody>().useGravity = false;
                break;
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
            serverAuthorityCachedVase = CacheNewObject(PickupableObject.PickupableType.Vase);
        }
    }

    public void RegisterResettableObject(ResettableObject ro)
    {
        if (!restoringResettableObject)
        {
            ro.id = resettableObjects.Count;
            resettableObjects.Add(ro);
            positionsOfResettableObjects.Add(ro.transform.position);
            rotationsOfResettableObjects.Add(ro.transform.rotation);
        }
        else
        {
            ro.id = lastResettableObjectDestroyed;
            resettableObjects[lastResettableObjectDestroyed] = (ro);
        }
    }

    public void RegisterResettableObjectDestroyed(int id)
    {
        lastResettableObjectDestroyed = id;
        restoringResettableObject = true;
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
            if (go.layer == layer) //9: Invisible player 1 or 12: Invisible player 2
            {
                go.AddComponent<InvisibleToVisible>();
                go.GetComponent<InvisibleToVisible>().delayToFadeInTime = currentLevelFixedTime;
                go.GetComponent<InvisibleToVisible>().FadeInTimeout = timeToMakeEverythingVisible;
            }
            else if (go.layer == layerNoSecretToPlayer)
            {
                go.GetComponent<Renderer>().enabled = false;
            }
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
            cameraFollow.target = player1.GetComponent<PlayerObjectInteraction>().fakePlayer.transform;
        else
            cameraFollow.target = player2.GetComponent<PlayerObjectInteraction>().fakePlayer.transform;
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
