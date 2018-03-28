using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipVignette : MonoBehaviour
{

    //Skip Vignette
    private void Update()
    {
        if (Input.GetButtonDown("Skip Vignette"))
        {
            Debug.Log("Skipping to next level");
            GameObject menu = GameObject.FindGameObjectWithTag("MenuUI");
            if (menu != null)
            {
                menu.GetComponent<StartOptions>().NextScene();

                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }

                this.enabled = false;
            }
        }
    }
}
