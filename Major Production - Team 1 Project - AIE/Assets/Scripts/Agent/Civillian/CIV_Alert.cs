////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <15/01/18>                               
// Brief: <Travel to the alerted destination the "lure" mechanic is used from the player>  
////////////////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEngine.AI;

public class CIV_Alert : State_CIV
{
    //Variables    
    Vector3 itemPos;
    public bool stopped = false;

    //Components
    CivillianController currentAgent;

    public void OnEnter(CivillianController agent)
    {
        currentAgent = agent;
        currentAgent.currentState = State.State_Alert;
        agent.civIconStateScript.myState = script_civilianIconState.gameState.alerted;
        currentAgent.txtState.text = "ALERT";
        itemPos = currentAgent.itemPosition;
        currentAgent.m_Animator.SetBool("lured", true);
    }

    public void OnExit(CivillianController agent)
    {
        stopped = false;
        currentAgent.m_Animator.SetBool("lured", false);
        currentAgent.m_Animator.SetBool("idle", false);
    }

    public void STATE_Update(CivillianController agent, StateMachine_CIV stateMachine, float deltaTime)
    {
        if(!stopped)
            currentAgent.navAgent.SetDestination(itemPos);

        float stoppingdist = 2.0f;

        if (stoppingdist >= Vector3.Distance(currentAgent.transform.position, currentAgent.target.transform.position) && !stopped)
        {
            stopped = true;
            currentAgent.m_Animator.SetBool("idle", true);
            currentAgent.navAgent.isStopped = true;

            //Create nav obstacle
            if (currentAgent.GetComponent<NavMeshObstacle>() == null)
            {
                NavMeshObstacle obstacle = currentAgent.gameObject.AddComponent<NavMeshObstacle>();
                obstacle.shape = NavMeshObstacleShape.Capsule;
                obstacle.radius = 0.3f;
                obstacle.center = new Vector3(0, 1, 0);
                obstacle.carving = true;
            }
            else //Obstacle has already been added first time around so enable from here on out
            {
                currentAgent.GetComponent<NavMeshObstacle>().enabled = true;
            }

            currentAgent.navAgent.enabled = false;
        }
    }
}
