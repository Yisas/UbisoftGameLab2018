using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VignettesLoad : MonoBehaviour
{
    LoadScene fadeScr;
    public int SceneNumb;

    
    void Awake()
    {
        fadeScr = GameObject.FindObjectOfType<LoadScene>();
        Debug.Log("Found FadeSrc: " + fadeScr);
        //fadeScr.EndScene(SceneNumb);
    }
}
