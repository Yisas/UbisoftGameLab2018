using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ripple : MonoBehaviour {
    ParticleSystem system
    {
        get
        {
            if (_CachedSystem == null)
                _CachedSystem = GetComponent<ParticleSystem>();
            return _CachedSystem;
        }
    }
    private ParticleSystem _CachedSystem;
    
    private float timeToRipple = 1;
    private float currentTimeToRipple = 0;
	
	// Update is called once per frame
	void Update () {
        if(transform.parent.GetComponent<Rigidbody>().velocity.magnitude > 0.01f)
        {
            if (system.isPlaying)
            {
                system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        else
        {
            if (!system.isPlaying)
            {
                currentTimeToRipple += Time.deltaTime;

                if (currentTimeToRipple > timeToRipple)
                {
                    system.Play(true);
                    currentTimeToRipple = 0;
                }
            }
        }
    }
}
