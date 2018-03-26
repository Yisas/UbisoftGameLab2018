using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public float soundEffectsVolume =100f;
    public float musicVolume = 100f;

    // Use this for initialization
    void Start () {
        AkSoundEngine.SetRTPCValue(1, soundEffectsVolume, gameObject);
        AkSoundEngine.SetRTPCValue(2, musicVolume, gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StopMusic()
    {
        AkSoundEngine.PostEvent("StopMainMenuMusic", gameObject);
        AkSoundEngine.PostEvent("StopVignettteMusic", gameObject);
        AkSoundEngine.PostEvent("StopLevelMusic", gameObject);
    }

    public void PlayMainMenuMusic()
    {
        AkSoundEngine.PostEvent("MenuMusic", gameObject);
    }

    public void PlayVignetteMusic()
    {
        AkSoundEngine.PostEvent("VignetteMusic", gameObject);
    }

    public void PlayLevelMusic()
    {
        AkSoundEngine.PostEvent("LevelMusic", gameObject);
    }

}
