////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <03/08/17>                               
// Brief: <Simple rotation script for the canvas on the Agent>  
////////////////////////////////////////////////////////////
using UnityEngine;

public class RotateCanvas : MonoBehaviour {

    private void Start()
    {
        #if !UNITY_EDITOR
        {
            this.enabled = false;
        }
        #endif
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 v = Camera.main.transform.position - transform.position;

        //Remove the x and z components from the vector
        v.x = 0.0f;
        v.z = 0.0f;

        transform.LookAt(Camera.main.transform.position - v);

        //Rotate around the Y axis
        transform.Rotate(0, 180, 0);
    }
}
