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
    public GameObject cachedVase;

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

        if (isServer)
        {
            //cachedVase = CacheNewObject(PickupableObject.PickupableType.Vase);
        }
        else
        {

        }

        if (lastLevelFinishedTime == 0) return;

        float extraPercentageTime = (lastLevelFinishedTime - lastLevelFixedTime) / lastLevelFinishedTime;
        if (extraPercentageTime < 0)
        {
            extraPercentageTime = 0;
        }
        adaptedCurrentLevelTime = currentLevelFixedTime + (currentLevelFixedTime * extraPercentageTime);
        lastLevelFixedTime = currentLevelFixedTime;
    }

    public GameObject CacheNewObject(PickupableObject.PickupableType type)
    {
        switch (type)
        {
            case PickupableObject.PickupableType.Vase:
                cachedVase = Instantiate(vase, new Vector3(0, 1000, 0), vase.transform.rotation);
                cachedVase.SetActive(false);
                NetworkServer.Spawn(cachedVase);
                if (isServer)
                {
                    RpcCacheNewObject(cachedVase, type);
                }
                return cachedVase;
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
                cachedVase = go;
                break;
        }
    }

    private void Update()
    {
        if (!isPaused)
        {
            currentLevelTime += Time.deltaTime;
        }

        if(!clientsConnected && GetComponent<NetworkIdentity>().observers.Count == 2)
        {
            clientsConnected = true;
            FindPlayers();
            cachedVase = CacheNewObject(PickupableObject.PickupableType.Vase);
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
