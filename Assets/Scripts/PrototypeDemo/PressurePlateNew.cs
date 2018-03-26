using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateNew : DoorAnimatorBehaviour
{
    public GameObject target;
    private Light myLight;
    public bool isActive;
    public GameObject[] wires;

    private GameObject objectOnMe = null;

    //Adding new:
    public Animation lockAnim;
    public int LockID;

    void Start()
    {
        myLight = GetComponent<Light>();
        myLight.enabled = false;
        isActive = false;
    }

    void LateUpdate()
    {
        //Disableing the lock animations once the main gate is open
        //I dont know if it need to be in late update, but it seems to work here..
        if (target.GetComponent<Door>().lockStay && !isActive)
        {
            lockAnim.enabled = false;
        }

    }

    public void forceExit()
    {
        if (isActive)
        {
            //Door[] doors = target.GetComponentsInChildren<Door>();
            //foreach (Door d in doors)
            //    d.DecCount();
            string animName = string.Concat("lock", LockID, "Open");
            if (!lockAnim.IsPlaying(animName))
            {
                lockAnim[animName].time = lockAnim[animName].length;
            }
            lockAnim[animName].speed = (lockAnim.enabled) ? -1 : 1;
            lockAnim.Play(animName);

            myLight.enabled = false;
            isActive = false;

            AkSoundEngine.PostEvent("PlateOff", gameObject);

            SetClosed();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Pushable" || other.tag == "Pickup")
        {
            if (objectOnMe != null)
            {
                Debug.Log("Object already on me, do nothing");
                return;
            }
            else
            {
                objectOnMe = other.gameObject;
            }

            //Lock Opens
            string animName = string.Concat("lock", LockID, "Open");
            lockAnim[animName].speed = 1;
            lockAnim.Play(animName);

            Open();
        }

        // If the object is a pickup set the boolean that its on a pressure plate
        ResettableObject resettableObject = other.GetComponent<ResettableObject>();
        if (resettableObject != null && resettableObject.CompareTag("Pickup"))
        {
            resettableObject.IsOnPressurePlate = true;
        }

    }

    private void Open()
    {
        if (!isOpen)
        {
            myLight.enabled = true;
            isActive = true;

            //Lock Opens
            string animName = string.Concat("lock", LockID, "Open");
            lockAnim[animName].speed = 1;
            lockAnim.Play(animName);

            AkSoundEngine.PostEvent("PlateOn", gameObject);

            SetOpen();

            Door[] doors = target.GetComponentsInChildren<Door>();
            foreach (Door d in doors)
                d.DecCount();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (objectOnMe == null && (other.tag == "Player" || other.tag == "Pushable" || other.tag == "Pickup"))
        {
            Open();
            objectOnMe = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.tag == "Player" || other.tag == "Pushable" || other.tag == "Pickup")
        {
            if (other.gameObject != objectOnMe)
            {
                return;
            }
            else
            {
                objectOnMe = null;
            }

            myLight.enabled = false;
            isActive = false;

            //Player leave the plate the lock closes      
            string animName = string.Concat("lock", LockID, "Open");
            if (!lockAnim.IsPlaying(animName))
            {
                lockAnim[animName].time = lockAnim[animName].length;
            }
            lockAnim[animName].speed = (lockAnim.enabled) ? -1 : 1;
            lockAnim.Play(animName);
            //lockAnim.Play(string.Concat("lock", LockID, "Close"));

            SetClosed();
            AkSoundEngine.PostEvent("PlateOff", gameObject);

            Door[] doors = target.GetComponentsInChildren<Door>();
            foreach (Door d in doors)
                d.IncCount();
        }


        // If the object is a pickup set the boolean that its on a pressure plate
        ResettableObject resettableObject = other.GetComponent<ResettableObject>();
        if (resettableObject != null && resettableObject.CompareTag("Pickup"))
        {
            resettableObject.IsOnPressurePlate = false;
        }
    }
}

