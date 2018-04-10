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
        currentAgent.currentState = State.State_Wander;
        agent.civIconStateScript.myState = script_civilianIconState.gameState.normal;
        agent.txtState.text = "WANDER";


        //Testing
        //Vector3 dir = agent.transform.forward * 15f;
        //dir += agent.transform.position; //Update direction based on Agent's current position

        //NavMeshHit navHit; //Stores the result of a NavMesh query
        //NavMesh.SamplePosition(dir, out navHit, 15f, -1); //Returns the closest point where randDirection is situated on the NavMesh
        //agent.navAgent.SetDestination(navHit.position);


        //agent.navAgent.SetDestination(agent.testTarget.position);
    }

    public void OnExit(CivillianController agent)
    {
        agent.navAgent.ResetPath();
    }

    public void STATE_Update(CivillianController agent, StateMachine_CIV stateMachine, float deltaTime)
    {
        if (agent.enableWander == true)
        {
            //Just set a random destination
            if (agent.navAgent.hasPath == false && agent.navAgent.enabled == true)
            {
                agent.navAgent.SetDestination(PickNewWanderPoint());
                //agent.currentDest = agent.navAgent.destination;
            }

            //if (agent.navAgent.isPathStale == true)
            //    Debug.Log(agent.name + " is stale");

            //if (agent.navAgent.remainingDistance > agent.navAgent.stoppingDistance)
            //    agent.Move(agent.navAgent.desiredVelocity, false, false);
            //else
            //    agent.Move(Vector3.zero, false, false);
        }


    }

    Vector3 PickNewWanderPoint()
    {
        //Debug.Log("PickNewWander Called.");
        //Returns a random point inside 3D space to move towards
        Vector3 randDirection = Random.insideUnitSphere * currentAgent.wanderRadius;

        randDirection += currentAgent.transform.position; //Update direction based on Agent's current position

        NavMeshHit navHit; //Stores the result of a NavMesh query
        Vector3 result;

        //Looping through the sample a few times, dont really have a fail-safe yet for this
        for (int i = 0; i < 10; i++)
        {
            //Sample position and move the found position a little bit away from the edge taking into account the navMeshAgents radius hopefully fixing them being stuck on certain corners
            if (NavMesh.SamplePosition(randDirection, out navHit, currentAgent.wanderRadius, NavMesh.AllAreas))
            {
                result = navHit.position;
                Vector3 pathDir = randDirection - result;
                result += pathDir.normalized * (currentAgent.navAgent.radius / 2);
                return result;
            }
        }

        Debug.LogError(currentAgent.name + " failed to pick a wander point.");
        return Vector3.zero;
    }
}
