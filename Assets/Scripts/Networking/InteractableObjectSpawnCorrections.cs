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

    private void Start()
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

            if (isServer)
            {
                RpcPutBackLayer();
            }
        }
    }

    public void Spawned(float timeSpawned, int spawningPlayer)
    {
        idOfPlayerThatSpawnedMe = spawningPlayer;
        notInteractingWithPlayerThatSpawnedMe = true;
        gameObject.layer = LayerMask.NameToLayer("Player " + (idOfPlayerThatSpawnedMe == 1 ? 2 : 1) + " While Carried");
        this.timeSpanwed = timeSpawned;
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
