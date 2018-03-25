using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class changeMenu : MonoBehaviour
{

    public GameObject controls;
    public GameObject mainMenu;
    public GameObject credits;
    public GameObject pause;
    public GameObject sid;

    public void controlsSwap()
    {
        if (mainMenu)
            mainMenu.SetActive(false);
        credits.SetActive(false);
        pause.SetActive(false);

        controls.SetActive(true);

        if (sid)
            sid.SetActive(false);
    }

    public void mainSwap()
    {
        controls.SetActive(false);
        credits.SetActive(false);
        pause.SetActive(false);

        if (mainMenu)
            mainMenu.SetActive(true);

        if (sid)
            sid.SetActive(true);
    }

    public void creditSwap()
    {
        controls.SetActive(false);
        if (mainMenu)
            mainMenu.SetActive(false);
        pause.SetActive(false);

        credits.SetActive(true);

        if (sid)
            sid.SetActive(false);
    }

    public void pauseSwap()
    {
        controls.SetActive(false);
        if (mainMenu)
            mainMenu.SetActive(false);
        credits.SetActive(false);

        pause.SetActive(true);
        if (sid)
            sid.SetActive(false);
    }

    public void Quit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
}
