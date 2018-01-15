////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <11/08/17>                               
// Brief: <Stores a reference to the object that instantiated the object that this script is attached to.>  
////////////////////////////////////////////////////////////
using UnityEngine;

public class AgentReference : MonoBehaviour {
    [HideInInspector] public GameObject spawner;
}
