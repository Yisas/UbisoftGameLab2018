using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableObject : MonoBehaviour
{
    public enum PickupableType { Box, Vase, Torch, BigBox }
    [SerializeField]
    private PickupableType type;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public PickupableType Type
    {
        get
        {
            return type;
        }
    }
}
