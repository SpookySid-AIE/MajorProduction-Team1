using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// This script controls: 
// - character controller movement (up and down using left shift and space, forward and back using W and S)
// - does not control Sids rotation
// - the ectoplasm value (is the ectoplasm value being stored in more than one place? - MP)
// - calculates ectoplasm damage based on a random accuracy range when Sid is hit (is this redundant? - MP)
// - 

public class playerController : MonoBehaviour
{
    public float Ectoplasm = 100.0f;
    [HideInInspector] public float ectoplasmValue; // What is this for? Why is this hidden? - MP
    public float speed = 6.0f;
    public float floatSpeed; //Speed of how fasty you float upwards when holding space.
    public float sinkspeed = 1.0f;

    private Text txt_ectoplasm;
    private Vector3 moveDirection = Vector3.zero;
    public CharacterController controller;

    void Start()
    {
        // Store reference to attached component
        controller = GetComponent<CharacterController>();
        txt_ectoplasm = GameObject.Find("Ectoplasm").GetComponent<Text>();
    }

    void Update()
    {

        txt_ectoplasm.text = Ectoplasm.ToString() + "%";
        
        // Use input "W" and "S" for direction, multiplied by speed
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var staticmove = moveDirection;
       
        moveDirection = transform.TransformDirection(moveDirection);
        
        moveDirection.y = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection.y = floatSpeed;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveDirection.y = -sinkspeed;
        }

        moveDirection *= speed;

        // Move Character Controller
        controller.Move(moveDirection * Time.deltaTime);

        //tilt Sid over
        var rot = transform.rotation;
        rot.eulerAngles = transform.rotation.eulerAngles + new Vector3(staticmove.z * 5.0f, 0, staticmove.x * 5.0f);
        //transform.rotation = rot;
    }

    private void OnDisable()
    {
        //controller.Move(Vector3.zero);
    }



    // Is this redundant? Shooting no longer uses game objects - MP
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            //Get the gun accuracy from the bullet - I store the gameobject that spawns the bullet in AgentReference - which is attached to the instantiated bullet
            float acc = other.gameObject.GetComponent<AgentReference>().spawner.GetComponent<AgentController>().gunAccuracy;
            acc = acc / 100; //Convert to decimal based - easier for range Random.Range

            //If the range is LESS than the acc then shoot - so if the Accuracy is 5 percent(0.05), that means Random.Range has to return 0.05 or less for it to shoot
            //Giving it a less likely chance to shoot

            if (Random.Range(0.0f, 1.0f) < acc) //Process if hit or not
            {
                Ectoplasm--;
                //Get the Agent that spawned the current bullet that hit //Access player object(current Item possesed) and unposses
                //if (GetComponent<playerPossession>().CheckIsPossesed() == true)
                //{
                //    other.gameObject.GetComponent<AgentReference>().spawner.GetComponent<AgentController>().target.GetComponent<playerPossession>().UnpossessItem();
                //}
            }
        }

    }
}