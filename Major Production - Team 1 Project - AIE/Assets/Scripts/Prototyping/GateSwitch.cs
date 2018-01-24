using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateSwitch : MonoBehaviour {

    public Transform Closed;
    public Transform Open;
    

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Civillian")
        {
            Closed.transform.position = Open.transform.position;
            Closed.transform.rotation = Open.transform.rotation;
        }
    }

}
