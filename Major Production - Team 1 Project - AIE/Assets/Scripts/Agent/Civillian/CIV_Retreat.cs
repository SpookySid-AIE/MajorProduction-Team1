////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <05/08/17>                               
// Brief: <Runs in the opposite direction from the player and cower>  
////////////////////////////////////////////////////////////
//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CIV_Retreat : State_CIV
{
    CivillianController currentAgent;
    GameObject ectoplasm;
    bool scared; //Determines whether or not the agent has reaches the scare threshold, if it hasnt, then this will lower the scare rating overtime
    float scaredRadius = 15.0f; //If the player enteres this radius then run away
    float timer = 3.0f; //Wait 3 seconds before switching states otherwise you are still scared if the player comes to you
    float scareDecreaseTimer = 3.0f;

    [HideInInspector]public static Vector3 dirAwayFromObject; //Set in OnCollisionEnter

#if UNITY_EDITOR

    //GameObject ectoplasms = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/particleEctoplasm.prefab", typeof(GameObject)) as GameObject;

#endif

    public void OnEnter(CivillianController agent)
    {        
        //Reset path to prevent errors when switching
        if(agent.navAgent.hasPath)
            agent.navAgent.ResetPath();

        currentAgent = agent; //Storing reference
        currentAgent.currentState = State.State_Retreat; //Setting currentState - kinda temporary i hope

        //Update animator
        if (currentAgent.m_Animator.enabled == false) //This is here temporarilly until we get a intriged animation going, this just unfreezes anim
            currentAgent.m_Animator.enabled = true;

        currentAgent.navAgent.speed = 5.0f; //Add some additional speed to make them feel really spooked
        currentAgent.m_Animator.SetBool("Scared", true);

        //Debug.Log(currentAgent.gameObject.name + " State: RETREAT");
        agent.txtState.text = "RETREAT";

        if (agent.TRIGGERED_floating)   //Was scared by a floating object - multiply base scariness by x 1
        {
            CheckScaryRating();
            ectoplasm.GetComponent<ectoplasmController>().modifier = 1;
            currentAgent.currentScareValue += (currentAgent.sid.GetComponent<playerPossession>().PossessedItem.GetComponent<ItemController>().baseScariness * 2);
        }
        else if (agent.TRIGGERED_hit)   //Was hit by an item - multiply base scariness by x2
        {
            CheckScaryRating();
            ectoplasm.GetComponent<ectoplasmController>().modifier = 2;
            currentAgent.currentScareValue += (currentAgent.target.GetComponent<ItemController>().baseScariness * 5);
        }
        else if (agent.TRIGGERED_repel) //Was lured to an item and spooked - multiply by x5??
        {
            CheckScaryRating();
            ectoplasm.GetComponent<ectoplasmController>().modifier = 5;
            currentAgent.currentScareValue += (currentAgent.target.GetComponent<ItemController>().baseScariness * 10);
        }

    }

    public void OnExit(CivillianController agent)
    {
        //Once the agent has fully finished the retreat state, reset the TRIGGERED flags so we can once again gain ecto for scaring them
        agent.TRIGGERED_floating = false;
        agent.TRIGGERED_hit = false;
        agent.TRIGGERED_repel = false;
        agent.hasDroppedEcto = false;
        currentAgent.navAgent.speed = 1.5f;
        currentAgent.m_Animator.SetBool("Scared", false);
        
        //Reset target item that hit us
        //agent.target = null; //Trying to prevent multiple hits from the same object, but seems to be working ok so far
    }

    public void STATE_Update(CivillianController agent, StateMachine_CIV stateMachine, float deltaTime)
    {
        //Check if we reached the scare threshold other wise continue with normal retreat state
        RunScaredExit();

        if (agent.hasDroppedEcto == false)
        {
            agent.hasDroppedEcto = true;
            //GameObject.Instantiate(ectoplasm, currentAgent.transform.position, currentAgent.transform.rotation);
        }

        //This is the code that makes the agent run away from whatever target has been set
        if (scared == false)
        {
            //Get the direction away from the target object that they are trying to flee from
            dirAwayFromObject = currentAgent.transform.position - currentAgent.target.transform.position;

            //Just minus a default value for now from the scared score
            //Overtime minus 1 from the scared value - possibly change in the future
            if (currentAgent.currentScareValue <= 0)
                currentAgent.currentScareValue = 0;
            else
            {
                scareDecreaseTimer -= Time.deltaTime;

                if (scareDecreaseTimer <= 0f)
                {
                    currentAgent.currentScareValue = currentAgent.currentScareValue - 1;
                    scareDecreaseTimer = 3.0f;
                }                
            }
            //Update the debug text
            currentAgent.txtScaredValue.text = currentAgent.currentScareValue.ToString();

            //If will is not close to the agent then don't flee
            if (dirAwayFromObject.magnitude > scaredRadius)
            {
                currentAgent.navAgent.velocity = Vector3.zero; //Stop the agent from moving

                //Debug.Log(timer);            
                timer -= Time.deltaTime; //minus the time
                if (timer <= 0f)
                {           
                    stateMachine.ChangeState(agent, new CIV_Wander()); //Change back to wander
                    timer = 3.0f; //Reset timer
                }
            }
            else //The player has moved close to the agent so move the agent
            {
                timer = 3.0f; //Reset the timer

                //if (agent.navAgent.hasPath == false)
                //{
                //    Vector3 randDirection = Random.insideUnitSphere * currentAgent.wanderRadius;

                //    //randDirection = randDirection += dirAwayFromObject; //Update direction based on Agent's current position

                //    NavMeshHit navHit; //Stores the result of a NavMesh query
                //    NavMesh.SamplePosition(randDirection, out navHit, currentAgent.wanderRadius, -1);

                //    agent.navAgent.SetDestination(navHit.position);
                //}

                if(agent.navAgent.hasPath == false)
                {
                    agent.navAgent.SetDestination(RandomNavSphere(currentAgent.transform.position, 10, -1));
                }

            } //End else
        }//End Scared if

    } //End update

    //Calculate a random point to run to - may need a different solution if they keep running towards the "scary" item
    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randDirection = Random.onUnitSphere * distance;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, distance, layermask);

        //Debug.DrawLine(randDirection - Vector3.forward * 0.05f, randDirection + Vector3.forward * 0.05f, Color.green, 50.0f);
        //Debug.DrawLine(randDirection - Vector3.right * 0.05f, randDirection + Vector3.right * 0.05f, Color.green, 50.0f);
        //Debug.DrawLine(randDirection - Vector3.up * 0.05f, randDirection + Vector3.up * 0.05f, Color.green, 50.0f);

        return navHit.position;
    }

    //Updates animation to scared run, and run towards exit point and despawn once at max scaredness
    void RunScaredExit()
    {
        if (currentAgent.currentScareValue >= currentAgent.scareThreshHoldMax)
        {
            currentAgent.txtScaredValue.text = currentAgent.scareThreshHoldMax.ToString();
            currentAgent.currentScareValue = currentAgent.scareThreshHoldMax;

            scared = true;

            //Update animator
            currentAgent.navAgent.speed = 5f; //Add some additional speed to make them feel really spooked
            currentAgent.m_Animator.SetBool("Scared", true);

            //Run to exit point
            currentAgent.navAgent.SetDestination(currentAgent.endPoint.position);

            //Despawn
            if (Vector3.Distance(currentAgent.endPoint.transform.position, currentAgent.transform.position) < 2)
            {
                GameObject.Destroy(currentAgent.gameObject);
                GameManager.Instance.NPCcount--;
            }
        }
    }

    //Load ectoplasm prefabs based on scary rating
    void CheckScaryRating()
    {

#if UNITY_EDITOR
        if (currentAgent.ItemScaryRating == ItemController.Orientation.Least_Scary)
            ectoplasm = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/particleEctoplasmGreen.prefab", typeof(GameObject)) as GameObject;

        else if (currentAgent.ItemScaryRating == ItemController.Orientation.Medium_Scary)
            ectoplasm = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/particleEctoplasmPink.prefab", typeof(GameObject)) as GameObject;

        else if (currentAgent.ItemScaryRating == ItemController.Orientation.Most_Scary)
            ectoplasm = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/particleEctoplasmRed.prefab", typeof(GameObject)) as GameObject;

        else Debug.LogError("CIV_Retreat isn't receiving the values from ItemController.Dropdown successfully.");
#elif UNITY_STANDALONE
        if (currentAgent.ItemScaryRating == ItemController.Orientation.Least_Scary)
            ectoplasm = currentAgent.ParticleGreen;

        else if (currentAgent.ItemScaryRating == ItemController.Orientation.Medium_Scary)
            ectoplasm = currentAgent.ParticlePink;

        else if (currentAgent.ItemScaryRating == ItemController.Orientation.Most_Scary)
            ectoplasm = currentAgent.ParticleRed;        
#endif

    }
}
