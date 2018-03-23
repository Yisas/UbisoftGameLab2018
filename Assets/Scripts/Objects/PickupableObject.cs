using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PickupableObject : NetworkBehaviour
{
    public enum PickupableType { Box, Vase, Torch, BigBox, Player }
    [SerializeField]
    private PickupableType type;
    public float timeToRenableInteractionWithSpawningPlayer;
    [SyncVar]
    private int idOfPlayerThatSpawnedMe;
    [SyncVar]
    private float timeSpanwed;
    [SyncVar]
    private bool notInteractingWithPlayerThatSpawnedMe = false;

    private void Start()
    {
        if (notInteractingWithPlayerThatSpawnedMe)
        {
            gameObject.layer = LayerMask.NameToLayer("Player " + (idOfPlayerThatSpawnedMe == 1 ? 2 : 1) + " While Carried");
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

    public PickupableType Type
    {
        get
        {
            return type;
        }
    }
}
