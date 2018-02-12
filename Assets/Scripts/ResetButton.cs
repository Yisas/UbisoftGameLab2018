using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ResetButton : MonoBehaviour {
    public GManager gameManager;
    public Animator anim;

	public void Push()
    {
        anim.SetTrigger("Push");
        gameManager.ResetAllResetableObjects();
    }
}
