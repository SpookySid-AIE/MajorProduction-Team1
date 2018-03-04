using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeMenu : MonoBehaviour
{

    public GameObject controls;
    public GameObject mainMenu;
    public GameObject credits;
    public GameObject pause;
    public GameObject sid;

    public void controlsSwap()
    {
        mainMenu.SetActive(false);
        credits.SetActive(false);
        pause.SetActive(false);

        controls.SetActive(true);

        sid.SetActive(false);
    }

    public void mainSwap()
    {
        controls.SetActive(false);
        credits.SetActive(false);
        pause.SetActive(false);

        mainMenu.SetActive(true);

        sid.SetActive(true);
    }

    public void creditSwap()
    {
        controls.SetActive(false);
        mainMenu.SetActive(false);
        pause.SetActive(false);
        
        credits.SetActive(true);

        sid.SetActive(false);
    }
}
