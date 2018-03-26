using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PickupableObject : NetworkBehaviour
{
    public enum PickupableType { Box = 0, Vase = 1, Torch = 2, BigBox = 3, Player = 4 }
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
