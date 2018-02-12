using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearingObject : MonoBehaviour {

    public int playerToAppearTo = 1;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Player")
        {
            int playerID = collision.transform.GetComponent<PlayerMove>().playerID;

            if(playerID==playerToAppearTo)
            {
                gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }
}
