using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerGameManagerCommunicators : NetworkBehaviour
{
    [Command]
    public void CmdResetCachedObject(PickupableObject.PickupableType type)
    {
        GManager.Instance.ResetCachedObject(type);
    }
}
