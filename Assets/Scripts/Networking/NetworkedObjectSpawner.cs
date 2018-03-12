using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class NetworkedObjectSpawner : NetworkBehaviour {

    public GameObject objToSpawn;
    public bool overridePrefabScale = false;

    public override void OnStartServer () {
        base.OnStartServer();

        GameObject obj = Instantiate(objToSpawn, transform.position, transform.rotation);

        if (overridePrefabScale)
        {
            obj.transform.localScale = transform.localScale;
        }

        NetworkServer.Spawn(obj);
        Destroy(gameObject);
	}
}
