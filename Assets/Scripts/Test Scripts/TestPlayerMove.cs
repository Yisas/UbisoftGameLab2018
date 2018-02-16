using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestPlayerMove : NetworkBehaviour {
	
	// Update is called once per frame
	void Update () {

        if (!isLocalPlayer)
            return;

        float x = Input.GetAxis("Horizontal 1");
        float z = Input.GetAxis("Vertical 1");
        transform.Translate(x, 0, z);
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }
}
