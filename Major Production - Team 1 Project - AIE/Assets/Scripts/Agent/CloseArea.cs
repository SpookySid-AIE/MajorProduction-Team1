////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <21/03/18>                               
// Brief: <Closes off a navmesh area once all agents have left>  
////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.AI;

public class CloseArea : MonoBehaviour
{
    //Variables
    public int currentInside;
    bool runUpdate = false;
	
	//Components
	
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(runUpdate) //Run once at least one has entered
        {
            if(currentInside <= 0)
            {
                gameObject.AddComponent<NavMeshObstacle>();
                gameObject.GetComponent<NavMeshObstacle>().size = gameObject.GetComponent<BoxCollider>().size;
                gameObject.GetComponent<NavMeshObstacle>().carving = true;
                this.enabled = false;
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        currentInside++;

        if (currentInside > 1 && runUpdate == false)
            runUpdate = true;
    }

    private void OnTriggerExit(Collider other)
    {
        currentInside--;
    }
}
