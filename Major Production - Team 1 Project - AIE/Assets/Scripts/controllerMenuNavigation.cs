//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.SceneManagement;
//using UnityEngine;

//public class controllerMenuNavigation : MonoBehaviour {

//    public GameObject controlsCanvas; //used in menu scene
//    public GameObject UI_Manager;//used in menu scene

//    public GameObject PauseMenuCanvas;//used in main scene
//    public GameObject GameOverMenuCanvas;//used in main scene
//    public GameObject Camera;////used in main scene
//    public GameObject exitButton;////used in main scene

//    // Update is called once per frame
//    void Update()
//    {
//        if (SceneManager.GetActiveScene().name == "Menu")
//        {
//            if (controlsCanvas.activeInHierarchy) //if controls are up on screen a button is to exit
//            {
//                if (Input.GetKeyDown("joystick button 0"))
//                {
//                    controlsCanvas.SetActive(false);
//                }
//            }
//            else
//            {
//                if (Input.GetKeyDown("joystick button 0"))
//                {
//                    UI_Manager.GetComponent<LoadScene>().Load(1);
//                }
//            }
//            if (Input.GetKeyDown("joystick button 1"))
//            {
//                controlsCanvas.SetActive(true);
//            }
//            if (Input.GetKeyDown("joystick button 2"))
//            {
//                UI_Manager.GetComponent<QuitOnClick>().Quit();
//            }
//        }

//        if (SceneManager.GetActiveScene().name == "Main")
//        {

//            //cant implement this at the moment as it is in the update method of the game controller connected to the camera
//            //if(Input.GetKeyDown("joystick button 2") && !PauseMenuCanvas.activeInHierarchy)
//            //{//activate pause menu
//            //    PauseMenuCanvas.SetActive(true);
//            //}

//            if (PauseMenuCanvas.activeInHierarchy || GameOverMenuCanvas.activeInHierarchy) //if controls are up on screen a button is to exit
//            {
//                //not implemented yet

//                //if (controlsCanvas.activeInHierarchy) //if controls are up on screen a button is to exit
//                //{
//                //    if (Input.GetKeyDown("joystick button 0"))
//                //    {
//                //        controlsCanvas.SetActive(false);
//                //    }
//                //}
//                //else
//                //{
//                    if (Input.GetKeyDown("joystick button 0"))
//                    {
//                    Camera.GetComponent<GameManager>().ResumeGameplay();
//                    }
//                //}

//                //not implemented yet
//                //if (Input.GetKeyDown("joystick button 1"))
//                //{
//                //    controlsCanvas.SetActive(true);
//                //}

//                if (Input.GetKeyDown("joystick button 2"))
//                {
//                    exitButton.GetComponent<QuitOnClick>().Quit();
//                }
//            }

//            if ( GameOverMenuCanvas.activeInHierarchy) //if controls are up on screen a button is to exit
//            {
//                if (Input.GetKeyDown("joystick button 0"))
//                {
//                    SceneManager.LoadScene("Main");
//                }

//                if (Input.GetKeyDown("joystick button 2"))
//                {
//                    exitButton.GetComponent<QuitOnClick>().Quit();
//                }
//            }
            

//        }
//    }
//}
