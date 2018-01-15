using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_CIV : IBehaviour_CIV
{
    public StateMachine_CIV() {
        currentState = null;
        previousState = null;
    }

    public void ChangeState(CivillianController agent, State_CIV state)
    {
        //Exit the currentState
        if (currentState != null)
        {
            currentState.OnExit(agent);
        }

        //Calls OnEnter from the new state
        if (state != null)
        {
            state.OnEnter(agent);
        }

        previousState = currentState;
        currentState = state;
    }

    //Inherited from IBehaviour
    public void Update(CivillianController pAgent, float deltaTime)
    {
        if (currentState != null)
        {
            currentState.STATE_Update(pAgent, this, deltaTime); //Pass in agent and current statemachine
        }
    }

    public State_CIV GetCurrentState() { return currentState; }
    public State_CIV GetPreviousState() { return previousState; }

    private State_CIV currentState;
    private State_CIV previousState;
}
