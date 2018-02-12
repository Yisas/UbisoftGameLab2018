using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingLight : MonoBehaviour {

    public Light lighto;
    public float timer;
    public float minIntensity;
    public float maxIntensity;
    private float internalTimer;

    private bool forwards = false;

	// Use this for initialization
	void Start () {
        internalTimer = timer;
	}
	
	// Update is called once per frame
	void Update () {

        if (!forwards)
            internalTimer -= Time.deltaTime;
        else
            internalTimer += Time.deltaTime;

        if (internalTimer < 0 && !forwards)
        {
            internalTimer = 0;
            forwards = true;
        }
        else if(internalTimer > timer && forwards)
        {
            internalTimer = timer;
            forwards = false;
        }

        lighto.intensity = Mathf.Lerp(minIntensity, maxIntensity, internalTimer / timer);
	}
}
