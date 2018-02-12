using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ResetButton : MonoBehaviour
{
    public GManager gameManager;
    public Animator anim;

    public void Push(int playerID)
    {
#if UNITY_EDITOR
        Debug.Log("Player  " + playerID + " pushing reset button");
#endif

        anim.SetTrigger("Push");
        gameManager.ResetAllResetableObjects();
    }
}
