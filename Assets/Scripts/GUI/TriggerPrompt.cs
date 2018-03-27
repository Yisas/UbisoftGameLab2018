using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPrompt : MonoBehaviour {

    //PlayerMove m_Player;
    //ButtonPromptsNetworked Net_Prompt;

    //void Start()
    //{
    //    Net_Prompt = GetComponentInParent<ButtonPromptsNetworked>();
    //}

    //void OnTriggerStay(Collider other)
    //{

    //    if (Net_Prompt.isBeingControlled)   //Local player and isBeingControlled
    //    {
    //        return;
    //    }

    //    if (other.tag == "Player")
    //    {
    //        m_Player = other.gameObject.GetComponent<PlayerMove>();

    //        if (!m_Player.isLocalPlayer)    //Not the local player then return
    //        {
    //            return;
    //        }
           
    //        Net_Prompt.Player = m_Player;
    //        Net_Prompt.TurnOnPrompt();
    //    }
    //}

    //void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        m_Player = other.gameObject.GetComponent<PlayerMove>();
    //        if (!m_Player.isLocalPlayer)
    //        {
    //            return;
    //        }
    //        Net_Prompt.TurnOffPrompt();
    //    }
    //}
}
