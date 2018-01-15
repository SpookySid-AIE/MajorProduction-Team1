using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface State_GPATROL {
    void OnEnter(AgentController agent);
    void STATE_Update(AgentController agent, StateMachine_GPATROL stateMachine, float deltaTime);
    void OnExit(AgentController agent);
}
