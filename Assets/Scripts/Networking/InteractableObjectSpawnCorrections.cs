using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InteractableObjectSpawnCorrections : NetworkBehaviour
{
    [SyncVar]
    public float timeToRenableInteractionWithSpawningPlayer;
    [SyncVar]
    private int idOfPlayerThatSpawnedMe;
    [SyncVar]
    private float timeSpanwed;
    [SyncVar]
    private bool notInteractingWithPlayerThatSpawnedMe = false;
    [HideInInspector]
    [SyncVar]
    public bool turnOnPhysicsAtStart = false;

    private void SpawnedOnOtherSide()
    {
        if (notInteractingWithPlayerThatSpawnedMe)
        {
            gameObject.layer = LayerMask.NameToLayer("Player " + (idOfPlayerThatSpawnedMe == 1 ? 2 : 1) + " While Carried");

            // Tell the local client they are ready to hide the fake object
            if (!isServer)
            {
                GManager.Instance.ForceHideFakeObject(idOfPlayerThatSpawnedMe);
            }
        }

        if (turnOnPhysicsAtStart)
        {
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Collider>().isTrigger = false;
        }
    }

    void Update()
    {
        if (notInteractingWithPlayerThatSpawnedMe && Time.time > timeSpanwed + timeToRenableInteractionWithSpawningPlayer)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            notInteractingWithPlayerThatSpawnedMe = false;
            GManager.Instance.ForceHideFakeObject(idOfPlayerThatSpawnedMe);

            if (isServer)
            {
                RpcPutBackLayer();
            }
        }
    }

    /// <summary>
    /// this function should be called from the local player throwing object
    /// </summary>
    /// <param name="timeSpawned"></param>
    /// <param name="spawningPlayer"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void LocalPlayerSpawningObject(float timeSpawned, int spawningPlayer, Vector3 position, Quaternion rotation)
    {
        if (GManager.Instance.LocalPlayerID == spawningPlayer)
        {
            if (isServer)
                RpcNonLocalPlayerSpawnCorrections(timeSpanwed, spawningPlayer, position, rotation);
            else
                CmdNonLocalPlayerSpawnCorrections(timeSpawned, spawningPlayer, position, rotation);

        }
    }

    private void NonLocalPlayerSpawnCorrections(float timeSpawned, int spawningPlayer, Vector3 position, Quaternion rotation)
    {
        idOfPlayerThatSpawnedMe = spawningPlayer;
        notInteractingWithPlayerThatSpawnedMe = true;
        gameObject.layer = LayerMask.NameToLayer("Player " + (idOfPlayerThatSpawnedMe == 1 ? 2 : 1) + " While Carried");
        this.timeSpanwed = timeSpawned;
        transform.position = position;
        transform.rotation = rotation;
    }

    [Command]
    public void CmdNonLocalPlayerSpawnCorrections(float timeSpawned, int spawningPlayer, Vector3 position, Quaternion rotation)
    {
        NonLocalPlayerSpawnCorrections(timeSpanwed, spawningPlayer, position, rotation);
    }

    [ClientRpc]
    public void RpcNonLocalPlayerSpawnCorrections(float timeSpawned, int spawningPlayer, Vector3 position, Quaternion rotation)
    {
        if (!isServer)
        {
            NonLocalPlayerSpawnCorrections(timeSpanwed, spawningPlayer, position, rotation);
        }
    }

    [ClientRpc]
    private void RpcPutBackLayer()
    {
        if (!isServer)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            notInteractingWithPlayerThatSpawnedMe = false;
        }
    }
}
