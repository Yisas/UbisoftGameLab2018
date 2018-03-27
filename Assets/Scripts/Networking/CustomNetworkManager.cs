using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

#if UNITY_EDITOR
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
            return;
#endif

        GetComponent<NetworkManagerHUD>().showGUI = false;
        CmdActivateCanvas();
        //GameObject.FindGameObjectWithTag("MenuUI").GetComponent<Canvas>().enabled = true;
        GameObject[] menuList = GameObject.FindGameObjectsWithTag("MenuUI");
        foreach (GameObject canvas in menuList)
            canvas.GetComponent<Canvas>().enabled = true;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {

        base.OnClientConnect(conn);

#if UNITY_EDITOR
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
            return;
#endif

        GetComponent<NetworkManagerHUD>().showGUI = false;
        //GameObject.FindGameObjectWithTag("MenuUI").SetActive(true);
        //GameObject[] menuList = GameObject.FindGameObjectsWithTag("MenuUI");
        //foreach (GameObject canvas in menuList)
        //    canvas.GetComponent<Canvas>().enabled = true;

    }

    [Command]
    private void CmdActivateCanvas()
    {
        GameObject.FindGameObjectWithTag("MenuUI").GetComponent<Canvas>().enabled = true;
    }
}
