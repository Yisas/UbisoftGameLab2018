using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LightDetector : MonoBehaviour {

    public List<Light> lights;

	void Start () {
        lights = new List<Light>();
        lights = FindObjectsOfType<Light>().ToList();
        List<Light> lightsToRemove = new List<Light>();

        foreach(Light light in lights)
        {
            if (light.transform.parent != null)
                lightsToRemove.Add(light);
        }

        // Remove all non-root lights from the list
        foreach(Light light in lightsToRemove)
        {
            lights.Remove(light);
        }
    }
	
	void Update () {
        DetectVisibility();
	}

    public void DetectVisibility()
    {
        bool isVisible = false;
        foreach (Light light in lights)
        {
            if (Physics.Raycast(transform.position, light.transform.forward * -1, 1000))
            {
                isVisible = true;
            }
        }
        if (isVisible)
            print("Player is not visible");
        else
            print("Player is visible");
    }
}
