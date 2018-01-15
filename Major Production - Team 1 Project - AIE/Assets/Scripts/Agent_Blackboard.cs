////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <27/07/17>                               
// Brief: <Handles the sending/recieving of information to our agents>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent_Blackboard : MonoBehaviour {

    private static Agent_Blackboard m_instance = null;
   // private Vector3[] PointsOfInterests; //Stores locations agents should travel too

    public static Agent_Blackboard Instance
    {
        get
        {
            return m_instance;
        }
    }

    //Retrieve the array
    //public Vector3[] getPointsOfInterest()
    //{
    //    return PointsOfInterests;
    //}

    //public void AddPointOfInterest(Vector3 poi)
    //{
    //    PointsOfInterests[PointsOfInterests.Length - 1] = poi;
    //}

    void Awake() //Called before start
    {
        //Check if instance already exists
        if (m_instance == null)
            m_instance = this;

        //If instance already exists and it's not this:
        else if (m_instance != this)
            Destroy(gameObject);		
	}

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update () {
		
	}
}
