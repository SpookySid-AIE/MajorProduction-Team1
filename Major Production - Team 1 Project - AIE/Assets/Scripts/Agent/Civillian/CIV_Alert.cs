////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <15/01/18>                               
// Brief: <Description>  
////////////////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEngine.AI;

public class CIV_Alert : State_CIV
{
    //Variables
    CivillianController currentAgent;
    Vector3 itemPos;

    //Components

    public void OnEnter(CivillianController agent)
    {
        currentAgent = agent;
        currentAgent.currentState = State.State_Alert;
        currentAgent.txtState.text = "ALERT";
        itemPos = currentAgent.itemPosition;
    }

    public void OnExit(CivillianController agent)
    {
    }

    public void STATE_Update(CivillianController agent, StateMachine_CIV stateMachine, float deltaTime)
    {
        currentAgent.navAgent.SetDestination(itemPos);

        float stoppingdist = 2.0f;

        if (stoppingdist >= Vector3.Distance(currentAgent.transform.position, currentAgent.target.transform.position))
        { 
            currentAgent.navAgent.isStopped = true;

        //    //Play thinking? animation here

            currentAgent.GetComponent<Animator>().enabled = false;
        }
    }
}
