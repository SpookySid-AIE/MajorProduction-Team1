////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <27/07/17>                               
// Brief: <Handles the playing of Audio and sending the audio "position" over to enemy>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioSource cupSmash; //Glass
    public AudioSource woodAudio; //Wood
    public AudioSource steelAudio; //Steel

    public static AudioManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    private static AudioManager m_instance = null;

    private Collider[] enemyColliders; //Stores all the enemies found in the Physics.Overlap sphere

    bool soundPlayed = false; //Check if the sound file played
    string[] materialTypes = new string[] { "Glass", "Wood", "Steel" };

    void Awake() //Called before start
    {
        //Check if instance already exists
        if (m_instance == null)
            m_instance = this;

        //If instance already exists and it's not this:
        else if (m_instance != this)
            Destroy(gameObject);
    }

    void SetTempSource(AudioSource correctSource, Transform newPos)
    {
        GameObject temp = new GameObject("TempAudio");//Create a temp audio object
        temp.transform.position = newPos.position; //Set its position 
        AudioSource aSource = temp.AddComponent<AudioSource>();//Add an audio source
        aSource.clip = correctSource.clip; //Set the temp audio source created to the correct one in the audio manager
        aSource.volume = correctSource.volume;
        aSource.Play();
        soundPlayed = true;
        Destroy(temp, aSource.clip.length); //Destroy object after clip duration
    }

    public void PlaySoundAtPosition(Transform newPos, float range, string material)
    {
        //This could get messy with a ton of audio clips, should be fine for this project 
        if (material == materialTypes[0]) //Glass
            SetTempSource(cupSmash, newPos);
        else if(material == materialTypes[1]) //Wood
            SetTempSource(woodAudio, newPos);
        else if(material == materialTypes[2]) //Steel
            SetTempSource(steelAudio, newPos);
        else
        {
            Debug.Log("The object: " + newPos.gameObject.name + " cannot play a sound because its material doesnt match against the one in the array.\n"
                      + "from AudioManager.cs");
        }

        if (soundPlayed == true) //Only enter this statement if a sound clip actually played
        {
            enemyColliders = Physics.OverlapSphere(newPos.position, range); //Returns an arrary of colliders of whoever is inside

            foreach (Collider enemy in enemyColliders)
            {
                if (enemy.tag == "GPatrol" && enemy.GetComponent<SphereCollider>())
                {
                    //Tell this enemy he heard something
                    //Need to store WHAT agent heard the sound so THAT SPECIFIC agent only navigates to it?
                    enemy.GetComponent<AgentController>().AddPointOfInterest(newPos.position);
                    //Debug.Log(enemy.gameObject.name + " heard the sound from " + transform.gameObject.name);
                }            
            }
            soundPlayed = false;
        }
        
    }
}
