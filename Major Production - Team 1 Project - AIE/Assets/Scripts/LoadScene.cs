////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <15/05/17>                               
// Brief: <Handles Scene Transition>  
////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{

    public GameObject Sid;
    public GameObject respawnPosition;

    public void Load(int sceneIndex) //Added by Jak //Loads scene by index, index of scenes is set via BuildSettings
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void respawn()
    {     
        Time.timeScale = 1;
        Sid.GetComponent<playerController>().Ectoplasm = 100;
        Sid.transform.position = respawnPosition.transform.position;
    }
}
