using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationTrigger : MonoBehaviour {

   public abstract void TriggerAction();

   protected void TriggerAnimator(bool isNested = false)
   {
       if(!isNested)
           GetComponent<Animator>().SetTrigger("toggleObject");
       else
       {
           Animator[] anims = GetComponentsInChildren<Animator>();
           foreach(Animator anim in anims)
           {
               GetComponent<Animator>().SetTrigger("toggleObject");
           }
       }
   }
}