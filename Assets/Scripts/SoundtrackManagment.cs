using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundtrackManagment : MonoBehaviour {

    AudioSource source;
    float volumeControl = 0.2f;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }
    void Start () 
    {
        source.volume = volumeControl;
    }
	
	void Update () { }



}
