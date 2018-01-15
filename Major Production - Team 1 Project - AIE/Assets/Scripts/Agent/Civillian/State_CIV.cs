using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface State_CIV {
    void OnEnter(CivillianController agent);
    void STATE_Update(CivillianController agent, StateMachine_CIV stateMachine, float deltaTime);
    void OnExit(CivillianController agent);
}
