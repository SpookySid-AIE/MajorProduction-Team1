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
    [HideInInspector] public GameObject sid; //Permanent reference to the player object "sid"
    //Exit point that they will travel to and despawn
    [Header("Where the Civs run to despawn.")] public Transform endPoint;
    public int scareThreshHoldMax;
    [Header("Range for LoS")] public float lineOfSight = 5;

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
    [HideInInspector] public int currentScareValue;
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

    [HideInInspector] public bool initialSpawn = false; //CivSpawner will set this, used in Civ_Wander
    [HideInInspector] public Vector3 initialSpawnDest;

    private StateMachine_CIV m_stateMachine;

    private Rigidbody m_Rigidbody;

    //Forward and turn floats, changes animation
    private float m_TurnAmount;
    private float m_ForwardAmount;

    private float stationaryTimer;
    private bool isStationary = false;

    private Vector3 m_GroundNormal;

    public GameObject rendererGeo; // 31/01/2018 Added by Mark - For custom colours

    //DEBUGGING
    [Header("----[DEBUGGING]----")]
    public Transform t;
    public bool enableWander;
    public State currentState;
    [Header("Dont Set. Showing target to follow")] public GameObject target; //used to SEEK or PURSUE a target, this will change now when hit by an item

    //Testing - Mark
    public script_civilianIconState civIconStateScript; // 19-12-2017 Added by Mark 
    public Color civilianPantsColour = Color.black; // 31/01/2018 Added by Mark - For custom colours
    public Color civilianTop1Colour = Color.black; // 31/01/2018 Added by Mark - For custom colours
    public Color civilianTop2Colour = Color.black; // 31/01/2018 Added by Mark - For custom colours

    private Renderer rend;   // 31/01/2018 Added by Mark  

    // Use this for initialization
    void Start()
    {
        //Temporary possibly - shouldnt need to setup an enum for currentState when i already  have a way to detect it in statemachine
        currentState = State.State_Wander;

        hasDroppedEcto = false;
        currentScareValue = 0;
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

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
        sid = target;

        civIconStateScript = GetComponent<script_civilianIconState>();// 19-12-2017 Added by Mark 
        civIconStateScript.myState = script_civilianIconState.gameState.normal;// 19-12-2017 Added by Mark 

        //StateMachine creation - Setting Default State - Will inherit this from inspector
        m_stateMachine = new StateMachine_CIV();
        m_stateMachine.ChangeState(this, new CIV_Wander());

        rend = rendererGeo.GetComponent<Renderer>();// 31/01/2018 Added by Mark - For custom colours
        rend.material.SetColor("_PantsColour", civilianPantsColour);// 31/01/2018 Added by Mark - For custom colours
        rend.material.SetColor("_Top1Colour", civilianTop1Colour);// 31/01/2018 Added by Mark - For custom colours
        rend.material.SetColor("_Top2Colour", civilianTop2Colour);// 31/01/2018 Added by Mark - For custom colours
    }

    private void FixedUpdate()
    {
        //if (navAgent.hasPath == false)
        //{
        //    Vector3 endPoint = new Vector3(this.transform.forward.x, this.transform.forward.y, this.transform.forward.z);//PickNewWanderPoint();
        //    base.prefVelocity_ = (new Vector2(endPoint.x, endPoint.z) - base.position_).normalized * base.maxSpeed_;
        //    navAgent.SetDestination(endPoint);
        //}
        //if(Input.GetKeyDown(KeyCode.E))
        //{
        //    prefVelocity_ = new Vector2(transform.right.x, this.transform.right.z) * this.maxSpeed_;
        //}

        //base.computeNeighbors();
        //base.computeNewVelocity();
        //base.update();
        //base.transform.position = new Vector3(this.position_.x, this.transform.position.y, this.position_.y);
        //transform.position = new Vector3(this.position_.x, this.transform.position.y, this.position_.y);
    }

    private void Update()
    {
        //Re-enable the stationary agent
        if (isStationary)
        {
            stationaryTimer += Time.deltaTime;

            if (stationaryTimer >= 1f)
            {
                stationaryTimer = 0;
                gameObject.GetComponent<NavMeshObstacle>().enabled = false;


                Debug.Log("Timer entered");

                isStationary = false;
            }
        } //NOTE: STORE THE CURRENT PATH END POINT, AND IF WE BECOME STATIONARY BEFORE REACHING THEN WE RECALCULATE A NEW PATH AND CONTINUE TO IT
        else //Turn the nav agent back on in the next frame
        {
            if (!isStationary && !navAgent.enabled)
            {
                navAgent.enabled = true;
            }
        }



        if (navAgent.enabled)
        {
            if (navAgent.remainingDistance > navAgent.stoppingDistance)
                Move(navAgent.desiredVelocity, false, false);
            else
                Move(Vector3.zero, false, false);
        }


        if (sid.GetComponent<playerPossession>().IsPossessed() == true && TRIGGERED_floating == false && sid.GetComponent<playerPossession>().IsHidden() == false)
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
        //Debug.Log("Possessed: " + sid.GetComponent<playerPossession>().IsPossessed());
        //Debug.Log("Triggered Float: " + TRIGGERED_floating);
        //Debug.Log("Hidden: " + sid.GetComponent<playerPossession>().IsHidden());

        //State changing to INTRIGUED if an itemPosition has been set, that means the ai has been in range of a recent lure mechanic used by the player
        if (alertedByItem && currentScareValue != scareThreshHoldMax)
        {
            m_stateMachine.ChangeState(this, new CIV_Alert());
            alertedByItem = false;
            //civIconStateScript.myState = script_civilianIconState.gameState.alerted;
        }

        //State changing to Retreat, because the repel mechanic was used in playerPosession
        if (TRIGGERED_repel && currentScareValue != scareThreshHoldMax)
        {
            ItemScaryRating = target.GetComponent<ItemController>().ItemScaryRating; //Target at this point is the object we are HIDING in - Set in playerPossesion
            m_stateMachine.ChangeState(this, new CIV_Retreat());
            //civIconStateScript.myState = script_civilianIconState.gameState.retreat;
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

    public void Move(Vector3 move, bool crouch, bool jump)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);

        //RaycastHit hitInfo;
        //if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 0.1f))
        //{
        //    m_GroundNormal = hitInfo.normal;
        //    m_Animator.applyRootMotion = true;
        //}
        //else
        //{
        //    m_GroundNormal = Vector3.up;
        //    m_Animator.applyRootMotion = false;
        //}

        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;

        //ApplyExtraTurnRotation();

        //Send input and other state parameters to the animator
        UpdateAnimator();
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Civillian" && navAgent.remainingDistance < other.GetComponent<NavMeshAgent>().remainingDistance
            && other.GetComponent<NavMeshAgent>().enabled == true)
        {
            Debug.Log("Trigger");

            other.GetComponent<NavMeshAgent>().enabled = false;

            if (other.GetComponent<NavMeshObstacle>() == null)
            {
                NavMeshObstacle obstacle = other.gameObject.AddComponent<NavMeshObstacle>();
                obstacle.shape = NavMeshObstacleShape.Capsule;
                obstacle.radius = 0.3f;
                obstacle.center = new Vector3(0, 1, 0);
                obstacle.carving = true;
                other.gameObject.GetComponent<CivillianController>().isStationary = true;
            }
            else
            {
                other.gameObject.GetComponent<NavMeshObstacle>().enabled = true;
                other.gameObject.GetComponent<CivillianController>().isStationary = true;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Item" && collision.gameObject.GetComponent<ItemController>().hasBeenThrown == true)
        {
            //m_Animator.SetBool("hit", true);
            //Set values for the item controller
            //currentScareValue += (collision.gameObject.GetComponent<ItemController>().baseScariness * 2);
           
            //Set the target to run away from to be the item that hit us
            target = collision.gameObject;

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(0, transform.position.y + 1.0f, 0 + (lineOfSight / 2)), new Vector3(1, 2, lineOfSight));        
    }
}
