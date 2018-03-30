using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSoundPlayer : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        AkSoundEngine.PostEvent("menu_start", gameObject);
    }

    private void OnDestroy()
    {
        AkSoundEngine.PostEvent("menu_stop", gameObject);
    }
}
