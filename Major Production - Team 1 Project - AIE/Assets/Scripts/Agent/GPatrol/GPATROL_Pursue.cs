////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <03/08/17>                               
// Brief: <Chases and shoots at Will until no longer in sight>  
////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GPATROL_Pursue : State_GPATROL
{
    AgentController currentAgent;
    script_ProtonBeam_v5 protonBeam;
    StateMachine_GPATROL currentStateMachine;
    float timer;
    float internalTimer;
    bool initialEnter;

    public void OnEnter(AgentController agent)
    {
        currentAgent = agent;
        protonBeam = agent.GetComponent<script_ProtonBeam_v5>();
        ///Debug.Log(currentAgent.gameObject.name + " State: PURSUE");
        agent.txtState.text = "PURSUE";
        initialEnter = true;
        internalTimer = 0;
    }

    public void OnExit(AgentController agent)
    {
        agent.anim.SetBool("stream", false);
    }

    public void STATE_Update(AgentController agent, StateMachine_GPATROL stateMachine, float deltaTime)
    {
        agent.navAgent.ResetPath();
        agent.navAgent.velocity = Vector3.zero;
        currentStateMachine = stateMachine;

        //Rotating Agent to face Will - LookAt does snap towards Will, talk to designers and see if they want lerping instead
        Vector3 v = agent.target.transform.position - agent.transform.position;

        //Remove the x and z components from the vector
        v.x = 0.0f;
        v.z = 0.0f;

        agent.transform.LookAt(agent.target.transform.position - v);

        //Rotate around the Y axis
        agent.transform.Rotate(0, agent.target.transform.rotation.y, 0);
        //End Rotation

        //SHOOT MECHANIC
        timer += Time.deltaTime; //minus the time

        //When player first enters the light, fire straight away before starting the charge timer
        if (initialEnter == true)
        {
            if (internalTimer < agent.bulletShootTime)
            {
                Vector3 target = agent.target.transform.position + new Vector3(0, 0.4f, 0);
                //target.x += UnityEngine.Random.Range(-agent.gunAccuracy, agent.gunAccuracy);
                //target.y += UnityEngine.Random.Range(-agent.gunAccuracy, agent.gunAccuracy);
                //target.z += UnityEngine.Random.Range(-agent.gunAccuracy, agent.gunAccuracy);

                protonBeam.target = target;
                protonBeam.fire = true;
                agent.anim.SetBool("stream", true); //Play stream animation

                internalTimer += Time.deltaTime;
                CheckTorch();
            }
            else //done shooting for x amount of time so begin the recharge timer
            {
                timer = 0;
                internalTimer = 0;
                protonBeam.fire = false;
                agent.anim.SetBool("stream", false); //Play stream animation
                initialEnter = false;
            }
        }
        else
        {
            if (timer > agent.bulletRecharge) //Once the timer is greater than the recharge wait time, fire for set amount of time
            {
                if (internalTimer < agent.bulletShootTime)
                {
                    Vector3 target = agent.target.transform.position + new Vector3(0, 0.4f, 0);
                    //target.x += UnityEngine.Random.Range(-agent.gunAccuracy, agent.gunAccuracy);
                    //target.y += UnityEngine.Random.Range(-agent.gunAccuracy, agent.gunAccuracy);
                    //target.z += UnityEngine.Random.Range(-agent.gunAccuracy, agent.gunAccuracy);

                    protonBeam.target = target;
                    protonBeam.fire = true;
                    agent.anim.SetBool("stream", true); //Play stream animation

                    internalTimer += Time.deltaTime;
                    CheckTorch();
                }
                else //done shooting for x amount of time so begin the recharge timer
                {
                    timer = 0;
                    internalTimer = 0;
                    protonBeam.fire = false;
                    agent.anim.SetBool("stream", false); //Play stream animation
                }
            }
        } //End Else

        CheckTorch();
        
    }

//State changing away from PURSUE
    void CheckTorch()
    {
        if (currentAgent.isWillInTorchLight() == false)
        {
            protonBeam.fire = false;
            currentAgent.anim.SetBool("shoot", false);
            timer = 0;

            //Will is now out of sight - go to where you last saw Will
            currentAgent.AddPointOfInterest(currentAgent.target.transform.position);

            //Check for points of interest
            if (currentAgent.CheckForPOI() == true)
                currentStateMachine.ChangeState(currentAgent, new GPATROL_Alerted());
            else
                //Default to wander if all else fails
                currentStateMachine.ChangeState(currentAgent, new GPATROL_Wander());

            //More state changing to come
        }
    }
}
