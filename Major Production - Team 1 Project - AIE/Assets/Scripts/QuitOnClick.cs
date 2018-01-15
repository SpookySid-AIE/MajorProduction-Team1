////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <15/05/17>                               
// Brief: <Handles game exit>  
////////////////////////////////////////////////////////////
using UnityEngine;

public class QuitOnClick : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
	