using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateNew : DoorAnimatorBehaviour
{
    public GameObject target;
    private Light myLight;
    private float targetPositionStart;
    private float targetPositionDown;
    private float targetPosition;
    public bool isActive;
    public GameObject[] wires;

    private GameObject objectOnMe = null;

    //Adding new:
    public Animation lockAnim;
    public int lockNum;

    public AudioSource onSound;
    public AudioSource offSound;

    private Door DoorInfo;

    void Start()
    {
        myLight = GetComponent<Light>();
        myLight.enabled = false;
        targetPositionStart = transform.position.y;
        targetPositionDown = transform.position.y - 0.07f;
        targetPosition = targetPositionStart;
        isActive = false;
        DoorInfo = GetComponent<Door>();
    }

    void Update()
    {
        if (isActive && transform.position.y > targetPosition)
        {
            Vector3 position = transform.position;
            position.y -= 0.005f;
            transform.position = position;
        }

        if (!isActive && transform.position.y < targetPosition)
        {
            Vector3 position = transform.position;
            position.y += 0.005f;
            transform.position = position;
        }
    /*
        //Debug.Log("TRIGGER EXIT--- isOpen: " + isOpen + " doorStaysOpen: " + doorStaysOpen + " isActive: " + isActive);
        Debug.Log("DOOR INFO --: " + DoorInfo.GetComponent<Door>().lockStay);
        //Disableing the lock animations once the main gate is open
        //Note: its wierd a weird condition, seems like it is controdicting xD but it works
        if (DoorInfo.GetComponent<Door>().lockStay)
        {
            lockAnim.enabled = false;
        }
        */

    }

    // Pierre - Required to fix a bug with clones
    public void forceExit()
    {
        if (isActive)
        {
            //Door[] doors = target.GetComponentsInChildren<Door>();
            //foreach (Door d in doors)
            //    d.DecCount();
            targetPosition = targetPositionStart;
            myLight.enabled = false;
            isActive = false;

            offSound.Play();

            SetClosed();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Pushable" || other.tag == "Pickup")
        {
            if(objectOnMe != null)
            {
                Debug.Log("Object already on me, do nothing");
                return;
            }
            else
            {
                objectOnMe = other.gameObject;
            }

            //Lock Opens
            lockAnim.Play(string.Concat("lock", lockNum, "Open"));
            Open();
        }

        // If the object is a pickup set the boolean that its on a pressure plate
        ResettableObject resettableObject = other.GetComponent<ResettableObject>();
        if(resettableObject != null && resettableObject.CompareTag("Pickup"))
        {
            resettableObject.IsOnPressurePlate = true;
        }
        
    }

    private void Open()
    {
        if (!isOpen)
        {
            targetPosition = targetPositionDown;
            myLight.enabled = true;
            isActive = true;

            onSound.Play();
            
            //Lock Opens
            lockAnim.Play(string.Concat("lock", lockNum, "Open"));

            SetOpen();

            Door[] doors = target.GetComponentsInChildren<Door>();
            foreach (Door d in doors)
                d.DecCount();                
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(objectOnMe == null && (other.tag == "Player" || other.tag == "Pushable" || other.tag == "Pickup" ))
        {
            Open();
            objectOnMe = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.tag == "Player" || other.tag == "Pushable" || other.tag == "Pickup")
        {
            if(other.gameObject != objectOnMe)
            {
                return;
            }
            else
            {
                objectOnMe = null;
            }

            targetPosition = targetPositionStart;
            myLight.enabled = false;
            isActive = false;
                        
            //Player leave the plate the lock closes          
            lockAnim.Play(string.Concat("lock", lockNum, "Close"));

            SetClosed();
            offSound.Play();

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

