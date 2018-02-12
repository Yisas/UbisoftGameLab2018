using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;
    public GameObject dialog1;
    public Text textBox1;
    public GameObject dialog2;
    public Text textBox2;

    // Use this for initialization
    void Start () {
        Instance = this;
	}

    public void showText(string text, int playerID)
    {
        if (playerID == 1)
        {
            textBox1.text = text;
            dialog1.SetActive(true);
        }
        else
        {
            textBox2.text = text;
            dialog2.SetActive(true);
        }
    }

    public void hideText(int playerID)
    {
        if (playerID == 1)
        {
            dialog1.SetActive(false);
        }
        else
        {
            dialog2.SetActive(false);
        }
    }
}
