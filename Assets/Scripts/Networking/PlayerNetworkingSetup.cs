using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkingSetup : MonoBehaviour {

    public SkinnedMeshRenderer playerMeshRenderer;

	public void SetPlayerIndex(int index)
    {
        GetComponent<PlayerMove>().PlayerID = index;
    }

    public void SetPlayerMaterial(Material material)
    {
        playerMeshRenderer.material = material;
    }
}
