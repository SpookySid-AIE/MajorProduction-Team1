////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <04/12/17>                               
// Brief: <Turns the Barrier back on, during scene start, was causing frame drops during editor>  
////////////////////////////////////////////////////////////
using UnityEngine;

public class enableBarrier : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
        Renderer[] r = GetComponentsInChildren<Renderer>();

        foreach (Renderer render in r)
        {
            render.enabled = true;
        }
    }
}
