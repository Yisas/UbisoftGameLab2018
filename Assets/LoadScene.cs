using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadScene : MonoBehaviour {

    public static string m_SceneToLoad;
    public static int m_ImageToLoad;

    //public Animator m_CanvasAnimator;
    public Image m_ImageToFade;

    private float timer;
    public float fadeSpeed = 5f;
    private bool fading = true;

    // Use this for initialization
    private void Awake()
    {
        m_ImageToFade.rectTransform.localScale = new Vector2(Screen.width, Screen.height);
    }

    private void Update()
    {
        // If the scene is starting...
        if (fading)
            //call the StartScene function.
            StartScene();
    }

    void StartScene()
    {
        // Fade the texture to clear.
        FadeToClear();

        // If the texture is almost clear...
        if (m_ImageToFade.color.a <= 0.05f)
        {
            //set the colour to clear and disable the RawImage.
            m_ImageToFade.color = Color.clear;
            m_ImageToFade.enabled = false;

            // The scene is no longer starting.
            fading = false;
        }
    }

    void FadeToClear()

    {
        // Lerp the colour of the image between itself and transparent.
        m_ImageToFade.color = Color.Lerp(m_ImageToFade.color, Color.clear, fadeSpeed * Time.deltaTime);
    }

    void FadeToBlack()
    {
        // Lerp the colour of the image between itself and black.
        m_ImageToFade.color = Color.Lerp(m_ImageToFade.color, Color.black, fadeSpeed * Time.deltaTime);
    }
   

    public IEnumerator EndSceneRoutine(int SceneNumber)
    {
        // Make sure the RawImage is enabled.
        m_ImageToFade.enabled = true;
        do
        {
            // Start fading towards black.
            FadeToBlack();

            // If the screen is almost black...
            if (m_ImageToFade.color.a >= 0.95f)
            {
                // ... reload the level
                SceneManager.LoadScene(SceneNumber);
                yield break;
            }
            else
            {
                yield return null;
            }
        } while (true);

    }
    
    public void EndScene(int SceneNumber)
    {
        fading = false;
        StartCoroutine("EndSceneRoutine", SceneNumber);
    }

    //public void LoadScene()
    //{
    //    UnityEngine.Networking.NetworkManager.singleton.ServerChangeScene(m_SceneToLoad);
    //    SceneManager.LoadScene(m_SceneToLoad);
    //}
}
