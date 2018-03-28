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

    [Command]
    public void CmdRegisterResettableObjectDestroyed(int id, PickupableObject.PickupableType type, bool objectDestroyedInServer)
    {
        GManager.Instance.RegisterResettableObjectDestroyed(id, type, objectDestroyedInServer);
    }
}
