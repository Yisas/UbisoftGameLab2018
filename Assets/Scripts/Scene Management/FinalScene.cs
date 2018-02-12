using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalScene : MonoBehaviour {

    public float timer;
    private float internalTimer;

	// Use this for initialization
	void Start () {
        internalTimer = timer;
	}
	
	// Update is called once per frame
	void Update () {
        internalTimer -= Time.deltaTime;

        if(internalTimer <= 0)
        {
            GameObject.FindGameObjectWithTag("MenuUI").GetComponent<StartOptions>().RestartGame();
            this.enabled = false;
        }
	}
}
