using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    public Animation anim;

    void Start()
    {
        anim = GetComponent<Animation>();

        //yield return new WaitForSeconds(anim.clip.length);
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            print("space key was pressed");
            anim.Play(anim.clip.name);
        }
    }
}
