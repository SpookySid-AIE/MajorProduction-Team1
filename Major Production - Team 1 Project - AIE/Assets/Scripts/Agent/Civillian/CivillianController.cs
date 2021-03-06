using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum State
{
    State_Alert,
    State_Retreat,
    State_Wander
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CivillianController : MonoBehaviour
{
    [Header("New Location Search Radius.")] public float wanderRadius; //Radius used to pick a new location within
    [Header("Audio Search Radius")] public float audioSearch; //The range at which the AGENT can hear the sound    
     public GameObject sid; //Permanent reference to the player object "sid"
    //Exit point that they will travel to and despawn
    [Header("Where the Civs run to despawn.")] public Transform endPoint;
    [Header("Max Scare Threshold")]public int scareThreshHoldMax;
    [HideInInspector]public float lineOfSight;

    [Header("Particle References")]
    //Public particle objects to spawn, here so it runs on builds until we can think of a better way
    public GameObject ParticleGreen;
    public GameObject ParticlePink;
    public GameObject ParticleRed;

    //Flags for the different ways we trigger ectoplasm scoring
    [HideInInspector] public bool TRIGGERED_repel;
    [HideInInspector] public bool TRIGGERED_hit;
    [HideInInspector] public bool TRIGGERED_floating;

    [HideInInspector] public NavMeshAgent navAgent;
    [HideInInspector] public float currentScareValue;
    //Stores the item scary rating retrieved from the Item that was used to spook the ai
    [HideInInspector] public ItemController.Orientation ItemScaryRating;
    [HideInInspector] public Animator m_Animator;

    [HideInInspector] public Text txtState;
    [HideInInspector] public Text txtScaredValue;

    [HideInInspector] public bool hasDroppedEcto;

    //These are set in playerPossession
    //Position storing used to send over into CIV_Alert state when the AI has been lured
    [HideInInspector] public Vector3 itemPosition;
    [HideInInspector] public bool alertedByItem;

    //Item Position for Retreat when items collide state - WORLD POS NO NAV SAMPLING DONE HER compared to "itemPosition" above
    //Hackish fix for the nullreference that occurs
    public Vector3 collidedItemPos;

    [HideInInspector] public Vector3 firstSpawnDest; //Position to travel to on spawn
    public Vector3 currentDest; //I store a reference to the endPathDestination here, so the Civs can resume the same destination when waiting to move

    private StateMachine_CIV m_stateMachine;

    //private Rigidbody m_Rigidbody;

    //Forward and turn floats, changes animation
    private float m_TurnAmount;
    private float m_ForwardAmount;

    private Renderer rend;   // 31/01/2018 Added by Mark  
    private Vector3 m_GroundNormal;

    public GameObject rendererGeo; // 31/01/2018 Added by Mark - For custom colours

    //Repathing variables when stuck
    public Transform otherAgent;
    private float stationaryTimer; //Timer to re-enable NavAgent
    [HideInInspector]public bool isStationary = false;
    private RaycastHit hit;    

    //Store unique Agent ID for the TriggerHighlight.cs
    private int id;
    public int GetID() { return id; }

    //FMOD
    public FMOD.Studio.EventInstance FMOD_ScaredInstance;

    //DEBUGGING
    [Header("----[DEBUGGING]----")]
    public Transform testTarget;
    public GameObject testParticle;
    public bool enableWander;
    public bool drawLineOfSight = false;
    public bool initialSpawn = false; //CivSpawner will set this, used in Civ_Wander
    public State currentState;
    [Header("Dont Set. Showing target to follow")] public GameObject target; //used to SEEK or PURSUE a target, this will change now when hit by an item
    public float stuckTimer; //Timer to count how long we are stuck on another agent for and when to turn into a obstacle
    public bool isOnNavMesh;

    //Timing estimation for resetting path
    public float estimationTime;
    public float currTimeOnPath;
    private bool estimTimeSet = false;

    //Testing - Mark
    public script_civilianIconState civIconStateScript; // 19-12-2017 Added by Mark 
    public Color civilianPantsColour = Color.black; // 31/01/2018 Added by Mark - For custom colours
    public Color civilianTop1Colour = Color.black; // 31/01/2018 Added by Mark - For custom colours
    public Color civilianTop2Colour = Color.black; // 31/01/2018 Added by Mark - For custom colours    

    // Use this for initialization
    void Start()
    {
        //Debug.Log("GetInstanceID: " + GetInstanceID());
        id = GetInstanceID();
        //Debug.Log("GetID: " + GetID());

        //Temporary possibly - shouldnt need to setup an enum for currentState when i already  have a way to detect it in statemachine but cant actually seem to get that working...
        currentState = State.State_Wander;
        lineOfSight = Camera.main.GetComponent<valueController>().civillianLineOfSight;
        hasDroppedEcto = false;
        currentScareValue = 0;
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        //m_Rigidbody = GetComponent<Rigidbody>();

        //Find EndPoint
        endPoint = GameObject.FindWithTag("EndPoint").transform;

        //Assign a random priority to this agent, hopefully fixing the random npcs getting caught on eachother
        //navAgent.avoidancePriority = Random.Range(1, 99);

        //Increment the NPC count
        GameManager.Instance.NPCcount++;

        //If the text canvas is not in this position - will throw an error
        txtState = transform.GetChild(0).GetChild(0).GetComponent<Text>(); //Accesses the txtState Text object in the heirachy attached to this Agent
        txtScaredValue = transform.GetChild(0).GetChild(1).GetComponent<Text>();

        //Show the debug states in editor
#if UNITY_EDITOR
        {
            txtState.enabled = true;
            txtScaredValue.enabled = true;
        }
#else
        {
            txtState.enabled = false;
            txtScaredValue.enabled = false;
        }
#endif

        //Set target to run away from
        target = GameManager.Instance.player.gameObject;
        sid = target.gameObject;

        civIconStateScript = GetComponent<script_civilianIconState>();// 19-12-2017 Added by Mark 
        civIconStateScript.myState = script_civilianIconState.gameState.normal;// 19-12-2017 Added by Mark 

        //Fmod instance creation
        FMOD_ScaredInstance = FMODUnity.RuntimeManager.CreateInstance(GameManager.Instance.audioCivScared);

        //StateMachine creation - Setting Default State - Will inherit this from inspector
        m_stateMachine = new StateMachine_CIV();
        m_stateMachine.ChangeState(this, new CIV_Wander());

        rend = rendererGeo.GetComponent<Renderer>();// 31/01/2018 Added by Mark - For custom colours
        rend.material.SetColor("_PantsColour", civilianPantsColour);// 31/01/2018 Added by Mark - For custom colours
        rend.material.SetColor("_Top1Colour", civilianTop1Colour);// 31/01/2018 Added by Mark - For custom colours
        rend.material.SetColor("_Top2Colour", civilianTop2Colour);// 31/01/2018 Added by Mark - For custom colours


        //DEBUGGING
        name = "Civ " + id;
    }

    private void Update()
    {
        //Forward raycast checking for a possible blocked agent inside the avoid radius
        //Makes this agent a stationary obstacle if they have been blocked for a certain time
        if (Physics.Raycast(new Vector3(transform.position.x, 1f, transform.position.z), transform.forward, out hit, navAgent.radius + .5f)
            && navAgent.isOnNavMesh && !initialSpawn)
        {
            if (hit.transform.tag == "Civillian" && hit.transform != this.transform)                
            {
                stuckTimer += Time.deltaTime;

                if (hit.transform.GetComponent<CivillianController>().isStationary == false && stuckTimer >= 2f)
                {
                    //Stop this agent and wait until the other one repaths around you
                    stuckTimer = 0;
                    m_Animator.SetBool("idle", true);
                    navAgent.isStopped = true;

                    currentDest = navAgent.destination;

                    navAgent.ResetPath();
                    navAgent.enabled = false;
                    isStationary = true;

                    //Create nav obstacle
                    if (gameObject.GetComponent<NavMeshObstacle>() == null)
                    {
                        NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();
                        obstacle.shape = NavMeshObstacleShape.Capsule;
                        obstacle.radius = 0.3f;
                        obstacle.center = new Vector3(0, 1, 0);
                        obstacle.carving = true;
                    }
                    else //Obstacle has already been added first time around so enable from here on out
                    {
                        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
                    }

                    //Storing the hit agent
                    otherAgent = hit.transform;
                }                
            }

            if (hit.transform.tag == "GPatrol" && hit.transform != this.transform)
            {
                stuckTimer += Time.deltaTime;

                if (this.transform.GetComponent<CivillianController>().isStationary == false && stuckTimer >= 2f)
                {
                    //Stop this agent and wait until the other one repaths around you
                    stuckTimer = 0;
                    m_Animator.SetBool("idle", true);
                    navAgent.isStopped = true;

                    currentDest = navAgent.destination;

                    navAgent.ResetPath();
                    navAgent.enabled = false;
                    isStationary = true;

                    //Create nav obstacle
                    if (gameObject.GetComponent<NavMeshObstacle>() == null)
                    {
                        NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();
                        obstacle.shape = NavMeshObstacleShape.Capsule;
                        obstacle.radius = 0.3f;
                        obstacle.center = new Vector3(0, 1, 0);
                        obstacle.carving = true;
                    }
                    else //Obstacle has already been added first time around so enable from here on out
                    {
                        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
                    }

                    //Storing the hit agent
                    otherAgent = hit.transform;
                }
            }
        }
        else
        {
            stuckTimer = 0;
        }

        //Turns on Wander once initial destination has been hit otherwise resample the location and continue moving there
        if (initialSpawn && navAgent.enabled) 
        {
            //distance check? test if we have reached near the firstspawn dest?
            //What if its no longer reachable? possible error
            //if we have reached our destination (hopefully on the same path) then enable wander and disable this block
            if (navAgent.hasPath)
            {
                if (Vector3.Distance(transform.position, firstSpawnDest) <= 1.0f)
                {
                    initialSpawn = false;
                    enableWander = true;
                }
            }
            else //Check incase one of the items update the navmesh and breaks the current agents pathing
            {
                //Debug.Log("Else called initialSpawn");
                NavMeshHit navHit;
                NavMesh.SamplePosition(firstSpawnDest, out navHit, wanderRadius, -1);
                navAgent.SetDestination(navHit.position); //No error checking so possible error if SamplePosition fails
            }
        }

        //If otherAgent was found we wait until its out of the way before we repath
        if (otherAgent != null)
        {
            Vector3 dirToOther = otherAgent.position - transform.position;

            if (dirToOther.magnitude >= 2f)
            {
                //Re-enable the stationary agent - Prevents agent not being placed on navmesh error
                if (isStationary)
                {
                    stationaryTimer += Time.deltaTime;

                    if (stationaryTimer >= .5f)
                    {
                        stationaryTimer = 0;
                        gameObject.GetComponent<NavMeshObstacle>().enabled = false;
                        isStationary = false;
                    }
                }
                else //Turn the nav agent back on in the next update frame
                {
                    otherAgent = null;
                    navAgent.enabled = true;
                    navAgent.ResetPath(); //Clear any paths somehow created during the time navagent was turned off
                    navAgent.isStopped = false;

                    //Calculate a new path to the same destination from when the agent was stopped
                    navAgent.SetDestination(currentDest);

                    //Destroy(gameObject);

                    //NavMeshPath path = new NavMeshPath();
                    //navAgent.CalculatePath(currentDest, path);
                    //navAgent.SetPath(path);


                    m_Animator.SetBool("idle", false);
                }
            }
        }
        else
        {
            if (isStationary)
                Debug.Log("stationaryNull");
        }
        //Reset the path if we have arrived
        //Maybe check the distance between current corner we are on and last corner in the path to check if we have arrived?
        if (navAgent.hasPath && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {            
            //Debug.Log(name + " Reset Called");
            navAgent.ResetPath();
        }

        //Timing estimation, Reset path if the time it took to reach path end point is greater than the estimated time give or take 4 seconds for avoidance?
        //Distance / Speed = Time
        //if(currentState == State.State_Wander)
        //{
            if (navAgent.hasPath) //Added retreat state check to turn off repathing causing issues atm
            {
                if (!estimTimeSet)
                {
                    estimationTime = Vector3.Distance(transform.position, navAgent.pathEndPosition) / navAgent.speed;
                    estimationTime = estimationTime + 2.0f; //Adding a little leway
                    estimTimeSet = true;
                }

                currTimeOnPath += Time.deltaTime;                

                if (currTimeOnPath >= estimationTime)
                    navAgent.ResetPath();
            }
            else
            {
                currTimeOnPath = 0;
                estimationTime = 0;
                estimTimeSet = false;
            }
        //}

        isOnNavMesh = navAgent.isOnNavMesh;

        //if(navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        //{
        //    Debug.Log(name + " invalid path.");
        //}

        //Debug.Log(Vector3.Distance(navAgent.transform.position, currentDest));

        //Update the animator
        //if (navAgent.enabled)
        //{
        //    if (navAgent.remainingDistance > navAgent.stoppingDistance)
        //        Move(navAgent.desiredVelocity, false, false);
        //    else
        //        Move(Vector3.zero, false, false);
        //}

        //State changing to Retreat because we have spotted an item and is spooked
        if (sid.GetComponent<playerPossession>().IsPossessed() == true && TRIGGERED_floating == false && sid.GetComponent<playerPossession>().IsHidden() == false && currentState != State.State_Retreat)
        {
            if (isInLineOfSight() == true)
            {
                //Debug.Log("In sight!");
                ItemScaryRating = sid.GetComponent<playerPossession>().PossessedItem.GetComponent<ItemController>().ItemScaryRating;
                TRIGGERED_floating = true; //This needs to be set to update the code in CIV_Retreat
                                           //Debug.Log("TRIGGERED FLOATING");
                FMODUnity.RuntimeManager.PlayOneShot(GameManager.Instance.audioCivSpotted, transform.position);

                m_stateMachine.ChangeState(this, new CIV_Retreat());
            }
        }

        //State changing to INTRIGUED if an itemPosition has been set, that means the ai has been in range of a recent lure mechanic used by the player
        if (alertedByItem && currentScareValue != scareThreshHoldMax)
        {
            alertedByItem = false;
            m_stateMachine.ChangeState(this, new CIV_Alert());       
        }

        //State changing to Retreat, because the repel mechanic was used in playerPosession
        if (TRIGGERED_repel && currentScareValue != scareThreshHoldMax && currentState != State.State_Retreat)
        {            
            ItemScaryRating = target.GetComponent<ItemController>().ItemScaryRating; //Target at this point is the object we are HIDING in - Set in playerPossesion
            m_stateMachine.ChangeState(this, new CIV_Retreat());
        }

        //Call update from this agents FSM
        if (m_stateMachine != null)
        {
            m_stateMachine.Update(this, Time.deltaTime);
        }


        //this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(new Vector3(this.velocity_.x, 0.0f, this.velocity_.y), Vector3.up), Time.deltaTime * 10f);
        //Debug.DrawLine(transform.position, transform.position + transform.forward, Color.green);

        //transform.Translate(Vector3.forward * Time.deltaTime);
    }

    //Original function was from the ThirdPersonController package, purely here now to update animation speed
    public void Move(Vector3 move, bool crouch, bool jump)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);

        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 0.1f))
        {
            m_GroundNormal = hitInfo.normal;
            m_Animator.applyRootMotion = true;
        }
        else
        {
            m_GroundNormal = Vector3.up;
            m_Animator.applyRootMotion = false;
        }

        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;

        //ApplyExtraTurnRotation();

        //Send input and other state parameters to the animator
        UpdateAnimator();
    }

    //Update animator params
    void UpdateAnimator()
    {
        // update the animator parameters
        m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
    }

    //public void OnAnimatorMove()
    //{
    //    // we implement this function to override the default root motion.
    //    // this allows us to modify the positional speed before it's applied.
    //    if (Time.deltaTime > 0)
    //    {
    //        Vector3 v = (m_Animator.deltaPosition * 1f) / Time.deltaTime;

    //        // we preserve the existing y part of the current velocity.
    //        v.y = m_Rigidbody.velocity.y;
    //        m_Rigidbody.velocity = v;
    //    }
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Item" && collision.gameObject.GetComponent<ItemController>().hasBeenThrown == true)
        {
            //m_Animator.SetBool("hit", true);
            //Set values for the item controller
            //currentScareValue += (collision.gameObject.GetComponent<ItemController>().baseScariness * 2);
            
            //Used in Retreat state when we are hit by an item, runs away from this world pos - item is deleted next frame
            collidedItemPos = collision.transform.position;

            GameObject smoke = Instantiate(GameObject.Find("PrefabController").GetComponent<PrefabController>().smokeEffect, collision.gameObject.transform.position, collision.gameObject.transform.rotation);
            Destroy(smoke, 2.0f);

            if (TRIGGERED_hit == false)
            {
                ItemScaryRating = collision.gameObject.GetComponent<ItemController>().ItemScaryRating;
                TRIGGERED_hit = true; //This needs to be set to update the code in CIV_Retreat
            }
            
            //Calculate new direction away from the hit object so we can "Avoid" it
            //Vector3 direction = collision.contacts[0].point - transform.position;
            //direction = -direction.normalized;
            //CIV_Retreat.dirAwayFromObject = direction; //Pass into CIV_Retreat
            
            //Change into Retreat State
            m_stateMachine.ChangeState(this, new CIV_Retreat());
            civIconStateScript.myState = script_civilianIconState.gameState.retreat;// 19-12-2017 Added by Mark 
        }
    }

    public bool isInLineOfSight() //Adjusted to now only look for the "possessed item" instead of always sid
    {        
        //Vector3 targetAdjustedPosition = (sid.transform.position + (sid.transform.up * 1.1f));
        playerPossession player = sid.GetComponent<playerPossession>();
        //Debug.Log(player.PossessedItem.transform.position);
        Vector3 targetAdjustedPosition = (player.PossessedItem.transform.position + (player.PossessedItem.transform.up * 1.1f));
        Vector3 adjustedPosition = (transform.position + (transform.up * 1.4f));

        Vector3 direction = targetAdjustedPosition - adjustedPosition;

        float dot = Vector3.Dot(transform.TransformDirection(Vector3.forward).normalized, direction.normalized); //-1 = directly behind, 1=directly infront

        //Debug.DrawRay(adjustedPosition, direction, Color.red);
        //Debug.DrawRay(adjustedPosition, new Vector3(direction.x, direction.y, direction.z + dot));

        //If Sid is within the Agent's torch range and angle of the spotlight
        if (direction.magnitude <= lineOfSight && dot > 0.8f)
        {
            //Create a raycast from the AGENT to sid to see if the AGENT has direct line of sight
            Ray ray = new Ray(adjustedPosition, direction); //-direction
            RaycastHit[] hits = Physics.RaycastAll(ray, lineOfSight); //Check all the objects within the torch range

            foreach (RaycastHit hit in hits)
            {
                //Looks at all transforms in range and checks for line of sight towards will and whether they are in front of Sid
                if (hit.distance < direction.magnitude && hit.collider.tag != "Player")
                {
                    //Debug.DrawRay(adjustedPosition, direction, Color.red); //Draw the ray when another object has been hit that is not the target
                    //Debug.Log(hit.transform.name);
                    return false;
                }
            }
            //Debug.DrawRay(adjustedPosition, direction, Color.green); //Draw the ray when an object is insde the LOS
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PlayScaredSound()
    {
        //Sound Effect ----------------------------------
        FMOD.Studio.PLAYBACK_STATE stateScared;
        FMOD_ScaredInstance.getPlaybackState(out stateScared); //Poll the audio events to see if playback is happening

        if (stateScared == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            FMOD_ScaredInstance.start(); // Starts the event
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(FMOD_ScaredInstance, GetComponent<Transform>(), GetComponent<Rigidbody>()); //Setup the 3D audio attributes
        }
        //End Sound Effect ----------------------------
    }

    private void OnDrawGizmosSelected()
    {
        if (drawLineOfSight)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;

            //Length for line of sight
            Gizmos.DrawWireCube(new Vector3(0, transform.position.y + 1.0f, 0 + (lineOfSight / 2)), new Vector3(1, 2, lineOfSight));
        }
        //Ray detecting other agents
        Debug.DrawRay(new Vector3(transform.position.x, 1f, transform.position.z), transform.forward * (GetComponent<NavMeshAgent>().radius + .5f), Color.magenta);

        //Show path without going into the Navigation window        
        if (Application.isPlaying)
        {
            if (navAgent.hasPath)
            {
                for (int i = 0; i < navAgent.path.corners.Length - 1; i++)
                {
                    Debug.DrawLine(navAgent.path.corners[i], navAgent.path.corners[i + 1], Color.red);
                }

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(navAgent.pathEndPosition, .5f);
            }
        }
    }
}
