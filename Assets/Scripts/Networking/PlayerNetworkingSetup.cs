using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkingSetup : MonoBehaviour {

    public SkinnedMeshRenderer playerMeshRenderer;

	public void SetPlayerIndex(int index)
    {
        // TODO
        throw new System.NotImplementedException();
    }

    public void SetPlayerMaterial(Material material)
    {
        playerMeshRenderer.material = material;
    }
}
