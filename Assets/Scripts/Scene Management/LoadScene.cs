using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadScene : MonoBehaviour {
 
    public Image m_ImageToFade;
    public float FadeInSpeed;
    public float FadeOutSpeed;
    private bool FadeIn;
    private bool FadeOut;
    private bool HasFadedOut;

    // Use this for initialization
    private void Awake()
    {
        m_ImageToFade.enabled = true;
        FadeIn = true;
        HasFadedOut = false;
    }

    private void Update()
    {
        // If the scene is starting...
        if (FadeIn)
        {
            m_ImageToFade.enabled = true;
            StartFade();
        }

        if (FadeOut)
        {
            m_ImageToFade.enabled = true;
            EndFade();
        }
    }

    void StartFade()
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
            FadeIn = false;
            HasFadedOut = true;
        }
    }

    void EndFade()
    {
        // Fade the texture to Black.
        FadeToBlack();

        // If the texture is almost clear...
        if (m_ImageToFade.color.a >= 0.83f)
        {
            m_ImageToFade.color = Color.black;
            
            FadeOut = false;
            Debug.Log("Herre.." + FadeOut);
        }
    }

    void FadeToClear()
    {
        // Lerp the colour of the image between itself and transparent.
        m_ImageToFade.color = Color.Lerp(m_ImageToFade.color, Color.clear, FadeInSpeed * Time.deltaTime);
    }

    void FadeToBlack()
    {
        // Lerp the colour of the image between itself and black.
        m_ImageToFade.color = Color.Lerp(m_ImageToFade.color, Color.black, FadeOutSpeed * Time.deltaTime);
    }
   

    //public IEnumerator EndSceneRoutine(int SceneNumber)
    //{
    //    // Make sure the RawImage is enabled.
    //    m_ImageToFade.enabled = true;
    //    do
    //    {
    //        // Start fading towards black.
    //        FadeToBlack();

    //        // If the screen is almost black...
    //        if (m_ImageToFade.color.a >= 0.95f)
    //        {
    //            // ... reload the level
    //            SceneManager.LoadScene(SceneNumber);
    //            yield break;
    //        }
    //        else
    //        {
    //            yield return null;
    //        }
    //    } while (true);

    //}

    public void SetFadeOut(bool isFading)
    {
        FadeOut = isFading;
    }
    
    public bool GetHasFadedOut()
    {
        return HasFadedOut;
    }
}
