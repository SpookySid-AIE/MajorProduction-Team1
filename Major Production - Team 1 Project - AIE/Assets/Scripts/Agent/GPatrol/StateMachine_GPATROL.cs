using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_GPATROL : IBehaviour_GPATROL
{
    public StateMachine_GPATROL() {
        currentState = null;
        previousState = null;
    }

    public void ChangeState(AgentController agent, State_GPATROL state)
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
    public void Update(AgentController pAgent, float deltaTime)
    {
        if (currentState != null)
        {
            currentState.STATE_Update(pAgent, this, deltaTime); //Pass in agent and current statemachine
        }
    }

    public State_GPATROL GetCurrentState() { return currentState; }
    public State_GPATROL GetPreviousState() { return previousState; }

    private State_GPATROL currentState;
    private State_GPATROL previousState;
}
