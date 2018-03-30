using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VignetteNarrationPlayer : MonoBehaviour
{

    public int vignetteNumber;

    // Use this for initialization
    void Start()
    {
        AkSoundEngine.PostEvent("footsteps_stop", gameObject);
        switch ("Vignette " + vignetteNumber)
        {
            case "Vignette 1":
                AkSoundEngine.PostEvent("cs_level1_start", gameObject);
                break;
            case "Vignette 2":
                AkSoundEngine.PostEvent("cs_level2_start", gameObject);
                break;
            case "Vignette 3":
                AkSoundEngine.PostEvent("cs_level3_start", gameObject);
                break;
            case "Vignette 4":
                AkSoundEngine.PostEvent("cs_level4_start", gameObject);
                break;
            case "Vignette 5":
                AkSoundEngine.PostEvent("cs_level5_start", gameObject);
                break;
            case "Vignette 6":
                AkSoundEngine.PostEvent("cs_level6_start", gameObject);
                break;
            case "Vignette 7":
                AkSoundEngine.PostEvent("cs_level7_start", gameObject);
                break;
            default:
                AkSoundEngine.PostEvent("cs_level1_stop", gameObject);
                AkSoundEngine.PostEvent("cs_level2_stop", gameObject);
                AkSoundEngine.PostEvent("cs_level3_stop", gameObject);
                AkSoundEngine.PostEvent("cs_level4_stop", gameObject);
                AkSoundEngine.PostEvent("cs_level5_stop", gameObject);
                AkSoundEngine.PostEvent("cs_level6_stop", gameObject);
                AkSoundEngine.PostEvent("cs_level7_stop", gameObject);
                break;
        }
    }

    private void OnDestroy()
    {
        AkSoundEngine.PostEvent("cs_level1_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level1_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level2_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level3_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level4_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level5_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level6_stop", gameObject);
        AkSoundEngine.PostEvent("cs_level7_stop", gameObject);
    }
}
