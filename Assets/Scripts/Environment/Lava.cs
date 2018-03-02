using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Lava : MonoBehaviour {

    // These dictionaries have parallel keys corresponding to the player ID
    private Dictionary<int, CameraFollow> cameraFollowDict;
    private Dictionary<int, bool> cameraDeactivatedDict;
    private Dictionary<int, float> cameraFollowTimeDict;

    private void Start()
    {
        cameraFollowDict = new Dictionary<int, CameraFollow>();
        cameraDeactivatedDict = new Dictionary<int, bool>();
        cameraFollowTimeDict = new Dictionary<int, float>();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            // Deactivate camera follow
            int playerID = other.GetComponent<PlayerMove>().PlayerID;

            // Get player's camera then deactivate it
            if (!cameraFollowDict.ContainsKey(playerID))
                cameraFollowDict.Add(playerID, GameObject.Find("Player " + playerID + " Camera").GetComponent<CameraFollow>());

            cameraFollowDict[playerID].enabled = false;

            // Keep track that the camera is disabled
            if (!cameraDeactivatedDict.ContainsKey(playerID))
                cameraDeactivatedDict.Add(playerID, true);
            else
                cameraDeactivatedDict[playerID] = true;

            // Update the time before camera is reenabled
            if (!cameraFollowTimeDict.ContainsKey(playerID))
                cameraFollowTimeDict.Add(playerID, other.GetComponent<PlayerMove>().cameraDelayTimerBeforeRespawn);
            else
                cameraFollowTimeDict[playerID] = other.GetComponent<PlayerMove>().cameraDelayTimerBeforeRespawn;

        }
    }

    private void Update()
    {
        // Attempt to reenable any disabled cameras
        int index;
        foreach (KeyValuePair<int, CameraFollow> cameraFollow in cameraFollowDict)
        {
            index = cameraFollow.Key;
            if (cameraDeactivatedDict[index])
            {
                cameraFollowTimeDict[index] -= Time.deltaTime;

                if (cameraFollowTimeDict[index] <= 0)
                {
                    cameraDeactivatedDict[index] = false;

                    cameraFollow.Value.enabled = true;
                }
            }
        }
    }
}
