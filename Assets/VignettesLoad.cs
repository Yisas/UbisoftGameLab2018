using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class VignettesLoad : MonoBehaviour
{
    LoadScene fadeScr;
    public int SceneNumb;
    private static int Level_Counter;
    public Image[] Vignettes_arr;

    public float Timer = 30;


    private void Awake()
    {
        fadeScr = GameObject.FindObjectOfType<LoadScene>();
        Debug.Log("Level Counter: " + Level_Counter);
        //fadeScr.EndScene(SceneNumb);
    }

    private void Start()
    {
        for (int i = 0; i < Vignettes_arr.Length; i++)
        {
            Vignettes_arr[i].enabled = false;
        }
    }

    private void Update()
    {
        LoadVignette();
        StartTimer();
    }

    private void LoadVignette()
    {
        switch (Level_Counter)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                Vignettes_arr[Level_Counter].enabled = true;
                break;
            default:
                Debug.Log("Default case");
                break;
        }
    }

    private void StartTimer()
    {
        Timer -= Time.deltaTime;
        if (Timer < 0)
        {
            Debug.Log(Timer +" ....seconds passed");
            UpdateLevelCounter();
            //Load Next Scene
            SceneManager.LoadScene(Level_Counter+1);
            
        }
    }

    private void UpdateLevelCounter()
    {
        Level_Counter++;
    }

}
