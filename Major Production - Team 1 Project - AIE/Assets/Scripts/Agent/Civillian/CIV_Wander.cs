////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <03/08/17>                               
// Brief: <Picks a random new position within a set radius and travels to it>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class CIV_Wander : State_CIV
{
    CivillianController currentAgent;
    int segments = 60; //Represents when the line renderer needs to draw a new line to make a circle, makes it more clearer
    float timer;
    bool waiting = false;
    
    public void OnEnter(CivillianController agent)
    {
        currentAgent = agent;
        //Debug.Log(currentAgent.gameObject.name + " State: WANDER");
        agent.txtState.text = "WANDER";
    }

    public void OnExit(CivillianController agent)
    {
        agent.navAgent.ResetPath();
    }

    public void STATE_Update(CivillianController agent, StateMachine_CIV stateMachine, float deltaTime)
    {
        if (agent.enableWander == true)
        {
            //If the agent doesnt have an INITIAL path then pick a new one 
            //This also picks a new one once the agent reaches the end of a path
            if (agent.navAgent.hasPath == false)
                agent.navAgent.SetDestination(PickNewWanderPoint());

            if (agent.navAgent.hasPath && Vector3.Distance(agent.transform.position, agent.navAgent.destination) <= 0.5)
            {
                agent.navAgent.ResetPath();
            }

            //if (agent.navAgent.remainingDistance > agent.navAgent.stoppingDistance)
            //    agent.Move(agent.navAgent.desiredVelocity, false, false);
            //else
            //    agent.Move(Vector3.zero, false, false);
        }
    }

    Vector3 PickNewWanderPoint()
    {
        //Returns a random point inside 3D space to move towards
        Vector3 randDirection = Random.insideUnitSphere * currentAgent.wanderRadius;

        randDirection += currentAgent.transform.position; //Update direction based on Agent's current position

        NavMeshHit navHit; //Stores the result of a NavMesh query
        NavMesh.SamplePosition(randDirection, out navHit, currentAgent.wanderRadius, -1); //Returns the closest point where randDirection is situated on the NavMesh

        return navHit.position; //Move towards the location
    }
}
