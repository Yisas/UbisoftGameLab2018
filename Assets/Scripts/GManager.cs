using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GManager : MonoBehaviour
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

    public static int invisiblePlayer1Layer = 9;
    public static int invisiblePlayer2Layer = 12;

    public static int SeeTP1NonCollidable = 20;
    public static int SeeTP2NonCollidable = 21;

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

    private void Update()
    {
        if (!isPaused)
        {
            currentLevelTime += Time.deltaTime;
        }
    }

    public void ResetAllResetableObjects(bool resetPlayers)
    {
        foreach (ResettableObject ro in GameObject.FindObjectsOfType<ResettableObject>())
        {
            if(!resetPlayers && ro.gameObject.tag == "Player")
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

    /// <summary>
    /// After player has been spawned, setup references on environmental objects
    /// </summary>
    public void NetworkingLevelReferencesSetup(PlayerMove playerSpawned)
    {
        Debug.Log("here");
        ButtonPromptsNetworked[] buttonPrompts = GameObject.FindObjectsOfType<ButtonPromptsNetworked>();
        foreach (ButtonPromptsNetworked bp in buttonPrompts)
        {
            bp.NetworkPlayerPromptReferenceStart(playerSpawned.transform.GetComponentInChildren<Canvas>());
        }
    }
}
