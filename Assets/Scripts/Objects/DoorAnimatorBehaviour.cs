using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimatorBehaviour : MonoBehaviour {

    public bool doorStaysOpen = true;
    public Animator animator;
    public bool isOpen = false;
    private bool firstAnimation = false;

    //Adding 03/11/2018:

    protected void FirstAnimationCheck()
    {
        if (!firstAnimation)
        {
            firstAnimation = true;
            animator.SetTrigger("Unlocked");
        }
    }

    public void ToggleOpen()
    {
        if(isOpen && doorStaysOpen)
        {
            return;
        }

        FirstAnimationCheck();

        isOpen = !isOpen;
        animator.SetBool("Open", isOpen);
    }

    public void SetOpen()
    {
        if (isOpen && doorStaysOpen)
        {
            return;
        }

        FirstAnimationCheck();

        isOpen = true;
        animator.SetBool("Open", isOpen);
    }

    public void SetClosed()
    {
        if (isOpen && doorStaysOpen)
        {
            return;
        }

        FirstAnimationCheck();

        isOpen = false;
        animator.SetBool("Open", isOpen);
    }

    //locks moving animation
}
