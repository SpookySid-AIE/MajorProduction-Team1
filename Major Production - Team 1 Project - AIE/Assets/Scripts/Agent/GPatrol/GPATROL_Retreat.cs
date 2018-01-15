////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <05/08/17>                               
// Brief: <Runs in the opposite direction from the player and cower>  
////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GPATROL_Retreat : State_GPATROL
{
    AgentController currentAgent;
    bool scared;
    float scaredRadius = 15.0f; ///If the player enteres this radius then run away
    float timer = 3.0f; //Wait 3 seconds before switching states otherwise you are still scared if the player comes to you

    public void OnEnter(AgentController agent)
    {
        currentAgent = agent;
        //Debug.Log(currentAgent.gameObject.name + " State: RETREAT");
        agent.txtState.text = "RETREAT";
    }

    public void OnExit(AgentController agent)
    {
    }

    public void STATE_Update(AgentController agent, StateMachine_GPATROL stateMachine, float deltaTime)
    {
        scared = false;
        agent.anim.SetBool("scared", scared); //Set the animation controller

        //Get the direction
        Vector3 dirAwayFromWill = currentAgent.transform.position - agent.target.transform.position;

        //If will is not close to the agent then don't flee
        if (dirAwayFromWill.magnitude > scaredRadius)
        {
            currentAgent.navAgent.velocity = Vector3.zero; //Stop the agent from moving

            //Animation settings
            scared = true;
            agent.anim.SetBool("scared", scared);

            //Debug.Log(timer);

            timer -= Time.deltaTime; //minus the time
            if (timer <= 0f)
            {
                //Reset the animation settings for next state
                scared = false;
                agent.anim.SetBool("scared", scared);

                stateMachine.ChangeState(agent, new GPATROL_Wander()); //Change back to wander
                timer = 3.0f; //Reset timer
            }
        }
        else //The player has moved close to the agent so move the agent
        {
            timer = 3.0f; //Reset the timer

            if (agent.navAgent.hasPath == false)
            {
                Vector3 randDirection = UnityEngine.Random.insideUnitSphere * currentAgent.wanderRadius;

                randDirection += dirAwayFromWill; //Update direction based on Agent's current position

                NavMeshHit navHit; //Stores the result of a NavMesh query
                NavMesh.SamplePosition(randDirection, out navHit, currentAgent.wanderRadius, -1);

                agent.navAgent.SetDestination(navHit.position);
            }
        } //End else

    } //End update

}
