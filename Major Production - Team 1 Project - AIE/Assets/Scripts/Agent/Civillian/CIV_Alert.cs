////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <00/00/00>                               
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
        currentAgent.txtState.text = "ALERT";
        itemPos = currentAgent.itemPosition;
    }

    public void OnExit(CivillianController agent)
    {
        throw new NotImplementedException();
    }

    public void STATE_Update(CivillianController agent, StateMachine_CIV stateMachine, float deltaTime)
    {
        currentAgent.navAgent.SetDestination(itemPos);
    }
}
