using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GManager : MonoBehaviour
{
    public static float lastLevelFinishedTime;
    public static float currentLevelTime;
    public static float adaptedCurrentLevelTime;
    public static bool isPaused = false;
    public float currentLevelInvisibleTime = 120; //120secs or any set. Delay time. After this time stuff will start appearing
    public float timeToMakeEverythingVisible = 200; //200secs to fade in everything
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
                go.AddComponent<InvisibleToVisible2>();
                go.GetComponent<InvisibleToVisible2>().numberOfRegressions = numberOfBlinks;
                go.GetComponent<InvisibleToVisible2>().delayToFadeInTime = currentLevelInvisibleTime;
                go.GetComponent<InvisibleToVisible2>().FadeInTimeout = timeToMakeEverythingVisible;
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
}
