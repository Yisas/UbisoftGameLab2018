using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSoundPlayer : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        AkSoundEngine.PostEvent("cs_level1_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level2_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level3_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level4_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level5_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level6_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level7_stop", gameObject);

        AkSoundEngine.PostEvent("footsteps_stop", gameObject);
        AkSoundEngine.PostEvent("music_level_start", gameObject);
    }

    private void OnDestroy()
    {
        AkSoundEngine.PostEvent("music_level_stop", gameObject);
    }
}
