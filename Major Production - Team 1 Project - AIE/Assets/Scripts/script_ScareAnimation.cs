using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script_ScareAnimation : MonoBehaviour {

    public bool scare = false;

    //Needs to be placed on an object with a Animator and Animation Controller assigned
    private Animator ac;
    private bool oldScare = false;

	void Start () {
        ac = GetComponent<Animator>();
	}
	
	void Update () {
		
        if (scare != oldScare)
        {
            oldScare = scare;
            ac.SetBool("scare", scare); // Only called when Scare is changed
        }
	}
}
