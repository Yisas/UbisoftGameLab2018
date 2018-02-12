using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorCommunicator : MonoBehaviour {

    public PlayerObjectInteraction throwing;

    public void PushButton()
    {
        if (throwing)
        {
            throwing.PushButton();
        }
    }
}
