using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fmod_PlayOneShot : MonoBehaviour {

    // FMOD stuff - MP
    [FMODUnity.EventRef]
    public string scareSoundOnRepel = "event:/Sid_Scare_Laugh";


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {

        // This tells FMOD to play the sound event when the animation plays - MP                     
        // FMODUnity.RuntimeManager.PlayOneShot(scareSoundOnRepel, this.gameObject.transform.position);
    }
}
