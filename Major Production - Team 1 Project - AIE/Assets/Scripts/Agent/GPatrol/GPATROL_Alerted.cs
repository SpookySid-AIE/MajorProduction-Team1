////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <03/08/17>                               
// Brief: <Travels to where the agent heard a sound>  
////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GPATROL_Alerted : State_GPATROL
{
    AgentController currentAgent;
    public void OnEnter(AgentController agent)
    {
        currentAgent = agent;
        ///Debug.Log(currentAgent.gameObject.name + " State: ALERTED");
        agent.txtState.text = "ALERTED";
    }

    public void OnExit(AgentController agent)
    {
    }

    public void STATE_Update(AgentController agent, StateMachine_GPATROL stateMachine, float deltaTime)
    {
        for (int i = 0; i < agent.GetPOICount(); i++)
        {

            //double distance = System.Math.Sqrt((agent.transform.position.x - agent.GetPositionFromPOI(i).x) * (agent.transform.position.x - agent.GetPositionFromPOI(i).x)
            //                                 + (agent.transform.position.y - agent.GetPositionFromPOI(i).y) * (agent.transform.position.y - agent.GetPositionFromPOI(i).y)
            //                                 + (agent.transform.position.z - agent.GetPositionFromPOI(i).z) * (agent.transform.position.z - agent.GetPositionFromPOI(i).z));


            //some code to ignore broken paths (the agent cant reach the poi so dont send them there)
            NavMeshPath path = new NavMeshPath();
            agent.navAgent.CalculatePath(agent.GetPositionFromPOI(i), path);
            
            if (path.status == NavMeshPathStatus.PathPartial)
            {
                //Debug.Log("DodgyPath");
                agent.RemovePOI(i);
                break;
            }

            //path good set destination
            agent.navAgent.SetDestination(agent.GetPositionFromPOI(i));
                                
            if ((agent.transform.position - agent.navAgent.destination).magnitude <= agent.torch.range/2)//this code gets them close enough to spot will if he is around
            {
                agent.RemovePOI(i);
                
                if (agent.isWillInTorchLight() == true)
                {
                    stateMachine.ChangeState(agent, new GPATROL_Pursue());
                    break;
                }

            }
            else
            {
                if (agent.isWillInTorchLight() == true)
                    stateMachine.ChangeState(agent, new GPATROL_Pursue());
                break;
            }
        }

        //Check if Will is in torchlight
        if (agent.isWillInTorchLight() == true)
        {
            stateMachine.ChangeState(agent, new GPATROL_Pursue());
        }
        else
        {
            //Check if the PointsOfInterest list is empty - if it is then default to Wander
            if (agent.CheckForPOI() == false)
                stateMachine.ChangeState(agent, new GPATROL_Wander());
        }
    }
}
