////////////////////////////////////////////////////////////
// Author: <Ben Thompson(ORIGINAL) + Jak Revai>                                     
// Date Created: <20/03/18>                               
// Brief: <Basic Movement motion script + control animation params/states for Sid> 
// Note: Original creator of this script was Ben Thompson, originally using a Character Controller here,
// we moved away from that and used rigidbody instead, so i needed to create a basic movement system here - Jak.
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Animator))] //Added by Jak 6.04.18
public class playerController : MonoBehaviour
{
    private static float Ectoplasm = 100.0f;
    [HideInInspector] public float GetEctoplasm { get{ return Ectoplasm;} set{ Ectoplasm = value;} }
    [Range(1, 20)]public float speed = 5.0f;
    
    [Header("Rigidbody drag should be same as this value.")][Range(1, 5)]public float floatSpeed = 3; //Speed of how fasty you float upwards when holding space.
    [Range(1, 4.75f)]public float sinkspeed = 2.75f;

    private Text txt_ectoplasm;
    private Vector3 moveDirection = Vector3.zero;
    private Rigidbody m_Rigid;
    [HideInInspector]public Animator m_Anim;
    private float damagedTimer;

    public float velocity;

    FMOD.Studio.EventInstance damageSound;

    void Start()
    {
        m_Rigid = GetComponent<Rigidbody>();
        m_Anim = GetComponent<Animator>();
        txt_ectoplasm = GameObject.Find("Ectoplasm").GetComponent<Text>();

        damageSound = FMODUnity.RuntimeManager.CreateInstance("event:/Sid_Damaged");
    }

    void Update()
    {
        //Update health on UI
        txt_ectoplasm.text = GetEctoplasm.ToString() + "%";
        
        // Use input "W" and "S" for direction, multiplied by speed
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
       
        //transform moveDirection to worldspace coordinates
        moveDirection = transform.TransformDirection(moveDirection);
        
        //Cancel out y vector incase anything is stored
        moveDirection.y = 0;

        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection.y = floatSpeed;
            m_Rigid.velocity = new Vector3(0, moveDirection.y, 0);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveDirection.y = -sinkspeed;
            m_Rigid.velocity = new Vector3(0, moveDirection.y, 0);
        }

        //New movement - Jak
        Vector3 newVelocity = (moveDirection *= speed);

        //Keep the y current velocity.
        newVelocity.y = m_Rigid.velocity.y;
        m_Rigid.velocity = newVelocity; //Set velocity directly(could behave weirdly). Its easier because we just need the simple movement/collision detection
                                        //Could cause some physics issues

        UpdateAnimator();
        //Debug.Log(m_Rigid.velocity.magnitude);
    }

    //Update animator params - Jak
    void UpdateAnimator()
    {
        float m_vel = m_Rigid.velocity.magnitude;
        velocity = m_vel;
        m_Anim.SetFloat("Velocity", m_vel, 0.1f, Time.deltaTime);

        if (m_Anim.GetBool("Damaged") == true)
            damagedTimer += Time.deltaTime;

        if (damagedTimer >= 1.0f) //Backup timer turning off animation incase trigger exit didnt catch it
        {
            damagedTimer = 0;
            m_Anim.SetBool("Damaged", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Minus health if hit by a bullet and calculate accuracy from the agent - Jak
        if (other.gameObject.tag == "Bullet")
        {
            //Get the gun accuracy from the bullet - I store the gameobject that spawns the bullet in AgentReference - which is attached to the instantiated bullet
            float acc = other.gameObject.GetComponent<AgentReference>().spawner.GetComponent<AgentController>().gunAccuracy;
            acc = acc / 100; //Convert to decimal based - easier for range Random.Range

            //If the range is LESS than the acc then shoot - so if the Accuracy is 5 percent(0.05), that means Random.Range has to return 0.05 or less for it to shoot
            if (Random.Range(0.0f, 1.0f) < acc) //Process if hit or not
            {
                GetEctoplasm--;

                if (m_Anim.GetBool("Damaged") == false)
                    m_Anim.SetBool("Damaged", true);

                //Sound Effect ----------------------------------
                FMOD.Studio.PLAYBACK_STATE stateDamaged;
                damageSound.getPlaybackState(out stateDamaged); //Poll the audio events to see if playback is happening

                //Check if any audio is still playing and stop it to prevent overlap - Then play the required clip
                if (stateDamaged == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                {
                    damageSound.start(); // Starts the event
                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(damageSound, GetComponent<Transform>(), GetComponent<Rigidbody>()); //Setup the 3D audio attributes
                }
                //End Sound Effect ----------------------------
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Bullet")
            if (m_Anim.GetBool("Damaged") == true)
                m_Anim.SetBool("Damaged", false);
    }
}