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
    [HideInInspector]public float Ectoplasm = 100.0f;
    [Range(1, 20)]public float speed = 5.0f;
    
    [Header("Rigidbody drag should be same as this value.")][Range(1, 5)]public float floatSpeed = 3; //Speed of how fasty you float upwards when holding space.
    [Range(1, 4.75f)]public float sinkspeed = 2.75f;

    private Text txt_ectoplasm;
    private Vector3 moveDirection = Vector3.zero;
    //public CharacterController controller;
    private Rigidbody rigid;
    Vector3 staticmove;

    void Start()
    {
        // Store reference to attached component
        //controller = GetComponent<CharacterController>();
        rigid = GetComponent<Rigidbody>();
        txt_ectoplasm = GameObject.Find("Ectoplasm").GetComponent<Text>();
    }

    void Update()
    {
        //Update health on UI
        txt_ectoplasm.text = Ectoplasm.ToString() + "%";
        
        // Use input "W" and "S" for direction, multiplied by speed
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        staticmove = moveDirection; //Store moveDirection before we transform it
       
        //transform moveDirection to worldspace coordinates
        moveDirection = transform.TransformDirection(moveDirection);
        
        //Cancel out y vector incase anything is stored
        moveDirection.y = 0;

        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection.y = floatSpeed;
            rigid.velocity = new Vector3(0, moveDirection.y, 0);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveDirection.y = -sinkspeed;
            rigid.velocity = new Vector3(0, moveDirection.y, 0);
        }

        //moveDirection *= speed;

        // Move Character Controller
        //controller.Move(moveDirection * Time.deltaTime);

        //New movement - Jak
        Vector3 newVelocity = (moveDirection *= speed);

        //Keep the y current velocity.
        newVelocity.y = rigid.velocity.y;
        rigid.velocity = newVelocity; //Set velocity directly(could behave weirdly). Its easier because we just need the simple movement/collision detection
                                      //Could cause some physics issues


        //tilt Sid over
        //var rot = transform.rotation;
        //rot.eulerAngles = transform.rotation.eulerAngles + new Vector3(staticmove.z * 5.0f, 0, staticmove.x * 5.0f);
        //transform.rotation = rot;
    }

    private void FixedUpdate()
    {
        ////Forward / Back
        //rigid.MovePosition((transform.position + new Vector3(staticmove.x *= speed,0,staticmove.z *= speed) * Time.deltaTime));
        //rigid.AddForce(new Vector3(0, 0, (staticmove.z *= speed) * Time.deltaTime));
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