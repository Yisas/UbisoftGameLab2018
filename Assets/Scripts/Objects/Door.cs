using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Door : DoorAnimatorBehaviour
{
    //public Text displayTest;
    public int pressurePlateCount;
    private int pressedCount;
    public bool lockStay;


    private void Start()
    {
        pressedCount = pressurePlateCount;
        lockStay = false;
    }

    public void DecCount()
    {
        pressedCount -= 1;

        if (pressedCount < 0)
            pressedCount = 0;

        //displayTest.text = pressedCount.ToString();
        //play the locks moving animation 

        if (pressedCount == 0)
        {
            SetOpen();
            lockStay = true;
            Portal.Instance.enableRays();
            AkSoundEngine.PostEvent("door_unlock", gameObject);
            AkSoundEngine.PostEvent("portal", gameObject);
            //if(doorStaysOpen)
            //{
            //    displayTest.enabled = false;
            //}
            //else
            //{
            //    displayTest.text = "";
            //}
        }
    }

    public void IncCount()
    {
        if (pressedCount == 0)
            SetClosed();

        pressedCount += 1;

        //displayTest.text = pressedCount.ToString();

        if (pressedCount >=  pressurePlateCount)
        {
            pressedCount = pressurePlateCount;
        }
    }
}
