using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PickupableObject : NetworkBehaviour
{
    public enum PickupableType { Box, Vase, Torch, BigBox, Player }
    [SerializeField]
    private PickupableType type;

    public PickupableType Type
    {
        get
        {
            return type;
        }
    }
}
