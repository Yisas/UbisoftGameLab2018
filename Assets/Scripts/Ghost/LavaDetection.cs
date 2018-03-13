using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaDetection : MonoBehaviour {

    public Ghost ghost;

    public void OnTriggerEnter(Collider other)
    {
        Lava lava = other.GetComponent<Lava>();
        if (lava != null)
            ghost.velocity.y = 1;
    }
}
