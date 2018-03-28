﻿using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class StartOptions : NetworkBehaviour {
    public MenuSettings menuSettingsData;
	public int sceneToStart = 1;										//Index number in build settings of scene to load if changeScenes is true
	public bool changeScenes;											//If true, load a new scene when Start is pressed, if false, fade out UI and continue in single scene
	public bool changeMusicOnStart;										//Choose whether to continue playing menu music or start a new music clip
    public CanvasGroup fadeOutImageCanvasGroup;                         //Canvas group used to fade alpha of image which fades in before changing scenes
    public Image fadeImage;                                             //Reference to image used to fade out before changing scenes
    public float fadeOutTime = 0.6f;                                           // Amount of time for the screen to fade out
    public float fullFadeTime = 0.5f;                                          // Amount of time for screen to stay dark
    public float fadeInTime = 1f;                                             // Amount of time for screen to fade back in

    [HideInInspector] public bool inMainMenu = true;					//If true, pause button disabled in main menu (Cancel in input manager, default escape key)
	[HideInInspector] public AnimationClip fadeAlphaAnimationClip;		//Animation clip fading out UI elements alpha


	private PlayMusic playMusic;										//Reference to PlayMusic script
	private float fastFadeIn = .01f;									//Very short fade time (10 milliseconds) to start playing music immediately without a click/glitch
	private ShowPanels showPanels;										//Reference to ShowPanels script on UI GameObject, to show and hide panels
    private CanvasGroup menuCanvasGroup;


    void Awake()
	{
		//Get a reference to ShowPanels attached to UI object
		showPanels = GetComponent<ShowPanels> ();

		//Get a reference to PlayMusic attached to UI object
		playMusic = GetComponent<PlayMusic> ();

        //Get a reference to the CanvasGroup attached to the main menu so that we can fade it's alpha
        menuCanvasGroup = GetComponent<CanvasGroup>();

        fadeImage.color = menuSettingsData.sceneChangeFadeColor;
        AkSoundEngine.PostEvent("menu_start", gameObject);
	}

    private void Update()
    {
        if (Input.GetButtonDown("Next Level"))
        {
            Debug.Log("Skipping to next level");
            NextScene();
        }

        if(Input.GetButtonDown("Previous Level"))
        {
            Debug.Log("Skipping to previous level");
            PreviousScene();
        }
    }


    public void StartButtonClicked()
	{
        AkSoundEngine.PostEvent("ui_validate", gameObject);

        //If changeMusicOnStart is true, fade out volume of music group of AudioMixer by calling FadeDown function of PlayMusic
        //To change fade time, change length of animation "FadeToColor"
        if (menuSettingsData.musicLoopToChangeTo != null) 
		{
			playMusic.FadeDown(menuSettingsData.menuFadeTime);
		}

        //If changeScenes is true, start fading and change scenes halfway through animation when screen is blocked by FadeImage
        if (menuSettingsData.nextSceneIndex != 0)
        {
            StartCameraFade();

            // Start fade in other game instance
            if (isServer)
            {
                RpcStartCameraFade();
                NetworkedSceneChange();
            }
        } 

		//If changeScenes is false, call StartGameInScene
		else 
		{
			//Call the StartGameInScene function to start game without loading a new scene.
			StartGameInScene();
		}

	}

    [ClientRpc]
    public void RpcStartCameraFade()
    {
        StartCameraFade();
    }

    public void StartCameraFade()
    {
        //Use invoke to delay calling of LoadDelayed by half the length of fadeColorAnimationClip
        //Invoke("LoadDelayed", menuSettingsData.menuFadeTime);
        StartCoroutine(FadeCanvasGroupAlpha(0f, 1f, fadeOutImageCanvasGroup));
    }

    private void NetworkedSceneChange()
    {
        string path = SceneUtility.GetScenePathByBuildIndex(sceneToStart);
        string sceneName = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
        playSceneAudio(sceneName);
        NetworkManager.singleton.ServerChangeScene(sceneName);
       
    }
    
    private void playSceneAudio(string sceneName)
    {
        switch(sceneName)
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
            case "Final Level":
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

    public void NextScene()
    {
        AkSoundEngine.PostEvent("menu_stop", gameObject);
        if (!isServer)
            return;

        RpcActivateCanvas();

        //If changeMusicOnStart is true, fade out volume of music group of AudioMixer by calling FadeDown function of PlayMusic
        //To change fade time, change length of animation "FadeToColor"
        if (menuSettingsData.musicLoopToChangeTo != null)
        {
            playMusic.FadeDown(menuSettingsData.menuFadeTime);
        }

        StartCameraFade();

        RpcStartCameraFade();

        sceneToStart += 1;

        NetworkedSceneChange();
    }

    public void RestartGame()
    {
        //If changeMusicOnStart is true, fade out volume of music group of AudioMixer by calling FadeDown function of PlayMusic
        //To change fade time, change length of animation "FadeToColor"
        if (menuSettingsData.musicLoopToChangeTo != null)
        {
            playMusic.FadeDown(menuSettingsData.menuFadeTime);
        }

        sceneToStart = 0;

        //Use invoke to delay calling of LoadDelayed by half the length of fadeColorAnimationClip
        Invoke("LoadDelayed", menuSettingsData.menuFadeTime);

        StartCoroutine(FadeCanvasGroupAlpha(0f, 1f, fadeOutImageCanvasGroup, true));
    }

    public void PreviousScene()
    {
        //If changeMusicOnStart is true, fade out volume of music group of AudioMixer by calling FadeDown function of PlayMusic
        //To change fade time, change length of animation "FadeToColor"
        if (menuSettingsData.musicLoopToChangeTo != null)
        {
            playMusic.FadeDown(menuSettingsData.menuFadeTime);
        }

        StartCameraFade();

        RpcStartCameraFade();

        sceneToStart -= 1;
        if (sceneToStart <= 0)
            sceneToStart = 1;

        NetworkedSceneChange();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += SceneWasLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneWasLoaded;
    }

    //Once the level has loaded, check if we want to call PlayLevelMusic
    void SceneWasLoaded(Scene scene, LoadSceneMode mode)
    {
		//if changeMusicOnStart is true, call the PlayLevelMusic function of playMusic
		if (menuSettingsData.musicLoopToChangeTo != null)
		{
			playMusic.PlayLevelMusic ();
		}

        if(scene.buildIndex > 0)
            StartCoroutine(FadeCanvasGroupAlpha(1f, 0f, fadeOutImageCanvasGroup));
    }

    public void LoadDelayed()
	{
		//Pause button now works if escape is pressed since we are no longer in Main menu.
		inMainMenu = false;

		//Hide the main menu UI element
		showPanels.HideMenu ();

		//Load the selected scene, by scene index number in build settings
		SceneManager.LoadScene (sceneToStart);
	}

	public void HideDelayed()
	{
		//Hide the main menu UI element after fading out menu for start game in scene
		showPanels.HideMenu();
	}

	public void StartGameInScene()
	{
		//Pause button now works if escape is pressed since we are no longer in Main menu.
		inMainMenu = false;

		//If there is a second music clip in MenuSettings, fade out volume of music group of AudioMixer by calling FadeDown function of PlayMusic 
		if (menuSettingsData.musicLoopToChangeTo != null) 
		{
			//Wait until game has started, then play new music
			Invoke ("PlayNewMusic", menuSettingsData.menuFadeTime);
		}

        StartCoroutine(FadeCanvasGroupAlpha(1f,0f, menuCanvasGroup));
	}

    public IEnumerator FadeCanvasGroupAlpha(float startAlpha, float endAlpha, CanvasGroup canvasGroupToFadeAlpha, bool destroy = false)
    {

        float elapsedTime = 0f;
        float totalDuration = menuSettingsData.menuFadeTime;

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / totalDuration);
            canvasGroupToFadeAlpha.alpha = currentAlpha;
            yield return null;
        }

        HideDelayed();
        if (destroy)
            Destroy(gameObject);
        Debug.Log("Coroutine done. Game started in same scene! Put your game starting stuff here.");
    }

    // Fades the camera out, stays dark for a small time, then fades it back in. The player cannot move during this time
    public void FadeOutThenIn(PlayerMove player)
    {
        StartCoroutine(FadeOutThenInRoutine(0f, 1f, fadeOutImageCanvasGroup, player));
    }

    public IEnumerator FadeOutThenInRoutine(float startAlpha, float endAlpha, CanvasGroup canvasGroupToFadeAlpha, PlayerMove player, bool destroy = false )
    {
        // Stop player movement
        player.canMove = false;
        float elapsedTime = 0f;
        float totalDuration = menuSettingsData.menuFadeTime;

        // Fade the screen out
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeOutTime);
            canvasGroupToFadeAlpha.alpha = currentAlpha;
            yield return null;
        }

        elapsedTime = 0f;

        // Maintain darkness for a small amount of time
        while (elapsedTime < fullFadeTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Allow the player to move again as soon as screen begins turning visible 
        elapsedTime = 0f;
        player.canMove = true;

        // Fade the screen back in
        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(endAlpha, startAlpha, elapsedTime / fadeInTime);
            canvasGroupToFadeAlpha.alpha = currentAlpha;
            yield return null;
        }

        HideDelayed();
        if (destroy)
            Destroy(gameObject);
        Debug.Log("Coroutine done. Game started in same scene! Put your game starting stuff here.");
    }


    public void PlayNewMusic()
	{
		//Fade up music nearly instantly without a click 
		playMusic.FadeUp (fastFadeIn);
		//Play second music clip from MenuSettings
		playMusic.PlaySelectedMusic (menuSettingsData.musicLoopToChangeTo);
	}

    [ClientRpc]
    private void RpcActivateCanvas()
    {
        GetComponent<Canvas>().enabled = true;
    }
}
