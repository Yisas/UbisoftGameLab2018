using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flicker : MonoBehaviour {

    public int numberOfFlickers;
    public float flickerIterationInterval;

    private LayerMask originalLayer;
    private int flickerCount = 0;

    // Update is called once per frame
    void Update () {
        if (Input.GetButtonDown("Test"))
            FlickerStart();
	}

    public void FlickerStart()
    {
        originalLayer = gameObject.layer;
        flickerCount = 0;

        StartCoroutine(ChangeLayer());
    }

    private IEnumerator ChangeLayer()
    {
        while(flickerCount <= numberOfFlickers)
        {
            yield return new WaitForSeconds(flickerIterationInterval);

            if (gameObject.layer == originalLayer)
            {
                gameObject.layer = LayerMask.NameToLayer("Default");
                flickerCount++;
            }
            else
            {
                gameObject.layer = originalLayer;
                flickerCount++;
            }
        }
    }
}
