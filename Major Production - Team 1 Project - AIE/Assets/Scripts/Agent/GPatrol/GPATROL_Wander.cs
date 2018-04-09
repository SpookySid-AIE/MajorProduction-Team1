////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <03/08/17>                               
// Brief: <Picks a random new position within a set radius and travels to it>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GPATROL_Wander : State_GPATROL
{
    AgentController currentAgent;
    int segments = 60; ///Represents when the line renderer needs to draw a new line to make a circle, makes it more clearer
    float timer;
    bool waiting = false;
    
    public void OnEnter(AgentController agent)
    {
        currentAgent = agent;
        //Debug.Log(currentAgent.gameObject.name + " State: WANDER");
        agent.txtState.text = "WANDER";
    }

    public void OnExit(AgentController agent)
    {
        agent.navAgent.ResetPath();
    }

    public void STATE_Update(AgentController agent, StateMachine_GPATROL stateMachine, float deltaTime)
    {
        #region Visualize the WANDER radius
        if (agent.ShowRadius == true) //If the show radius flag is checked in inspector display the ring
        {
            //Set number of segments
            agent.line.positionCount = segments + 1;

            //Drawing the Circle around the agent to show its search radius
            for (int i = 0; i < segments + 1; i++)
            {
                int angle = (360 / segments) * i;

                agent.line.SetPosition(i, agent.transform.position + agent.wanderRadius * new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle)));
            }
        }
        #endregion

        if (agent.enableWander == true && Time.timeScale != 0)
        {
            //If the agent doesnt have an INITIAL path then pick a new one 
            //This also picks a new one once the agent reaches the end of a path
            if (agent.navAgent.hasPath == false)
                PickNewWanderPoint();

            if (agent.navAgent.hasPath && Vector3.Distance(agent.transform.position, agent.navAgent.destination) <= 0.5)
            {
                agent.navAgent.ResetPath();
            }

            //Check if Will is in torchlight
            if (agent.isWillInTorchLight() == true)
            {
                stateMachine.ChangeState(agent, new GPATROL_Pursue());
            }
            else
            {
                //Check if any points of interest has been registered
                if (agent.CheckForPOI() == true)
                    stateMachine.ChangeState(agent, new GPATROL_Alerted());
            }



            //if (agent.transform.position == agent.navAgent.pathEndPosition)
            //{
            //    waiting = true;
            //    timer += deltaTime;

            //    if (timer >= 1)
            //    {
            //        waiting = false;
            //        timer = 0;
            //    }
            //}
        }
    }

    //IEnumerator CheckIfWillInTorch()
    //{
    //    if (currentAgent.isWillInTorchLight() == true)
    //    {
    //        yield return true;
    //    }
    //    else
    //    {
    //        yield return false;
    //    }
    //}

    void PickNewWanderPoint()
    {
        //Returns a random point inside 3D space to move towards
        Vector3 randDirection = Random.insideUnitSphere * currentAgent.wanderRadius;

        randDirection += currentAgent.transform.position; //Update direction based on Agent's current position

        NavMeshHit navHit; //Stores the result of a NavMesh query
        NavMesh.SamplePosition(randDirection, out navHit, currentAgent.wanderRadius, -1); //Returns the closest point where randDirection is situated on the NavMesh

        currentAgent.navAgent.SetDestination(navHit.position); //Move towards the location
    }
}
