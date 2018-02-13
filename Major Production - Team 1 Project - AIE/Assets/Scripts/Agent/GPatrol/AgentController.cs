////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <08/07/17>                               
// Brief: <Main Agent script>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AgentController : MonoBehaviour
{
    [Header("New Location Search Radius.")] public float wanderRadius; //Radius used to pick a new location within
    [Header("Agent Move Speed")]public float movementSpeed;
    [Header("Audio Search Radius")] public float audioSearch; //The range at which the AGENT can hear the sound
    [Header("Show Search Radius")] public bool ShowRadius;
    [Header("Display State Debug?")] public bool ShowState;
    [HideInInspector]public GameObject target; //used to SEEK or PURSUE a target eg. Will                                                                         
    [Header("Animation Controller")]public Animator anim;

    //Used in WanderState
    [HideInInspector] public LineRenderer line; //Used to draw a debug circle to show the search radius
    [HideInInspector] public NavMeshAgent navAgent; //Agent(Spook Squad AI)
    [HideInInspector] public Text txtState;

    //Used in PursueState
    [HideInInspector]public Light torch;
    [Header("How often does the gun damage the player. Percent Based.")][Range(0, 100)]public float gunAccuracy;
    [Header("How long to shoot for")]public float bulletShootTime;
    [Header("How long to wait before shooting again")]public float bulletRecharge;    

    //Used in AlertedState

    private SphereCollider colliderSphere; //Used to determine where an "Audio" point of interest has appeared
    private List<Vector3> pointsOfInterest;
    private StateMachine_GPATROL m_stateMachine;

    //Debugging the Points of Interests
    private GameObject temp;
    private GameObject POIPrefab;
    private List<GameObject> tempPOIList;

    //TESTING - DISABLE WANDER BOOL
    public bool enableWander;

    //Possibly use this later for now unused
    //Agent_Blackboard blackboard = Agent_Blackboard.Instance;

    //For when we BUILD the exe, dont spawn objects, causing issues currently
    bool showPOI;

    // Use this for initialization
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>(); //Find the navMesh component
        navAgent.speed = movementSpeed; //Set the agent speed through the AgentController Script
        target = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();

        torch = transform.GetChild(1).GetComponent<Light>(); //Gets the lightsource the agent is using

        colliderSphere = GetComponent<SphereCollider>();
        colliderSphere.radius = audioSearch;

        //Line Renderer for debugging the wander radius
        line = GetComponent<LineRenderer>();
        line.enabled = false;
        if (ShowRadius) //Enables the component if the inspector flag is true
            line.enabled = true;        

        //If the text canvas is not in this position - will throw an error
        txtState = transform.GetChild(0).GetChild(0).GetComponent<Text>(); //Accesses the txtState Text object in the heirachy attached to this Agent

        //StateMachine creation - Setting Default State - Will inherit this from inspector
        m_stateMachine = new StateMachine_GPATROL();
        m_stateMachine.ChangeState(this, new GPATROL_Wander());

        if (ShowState == false) //Simply display the state above their head
            txtState.enabled = false;

        //Visual Debugging for the points of interest
        pointsOfInterest = new List<Vector3>();
        tempPOIList = new List<GameObject>();

#if UNITY_EDITOR
        {
            //Load the POI prefab to instantiate later
            POIPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/POI.prefab", typeof(GameObject)) as GameObject;
            showPOI = true;
        }
#else
        showPOI = false;
#endif

    }

    // Update is called once per frame
    void Update()
    {
        navAgent.speed = movementSpeed; //Update agent speed to the one in passed in from inspector

        if (navAgent.velocity.magnitude > 0.0001f)
            anim.SetFloat("speed", navAgent.speed); //Set the speed in the animation controller, only hooked speed up for now
        else //Agent stopped movement so stop animation   
            anim.SetFloat("speed", 0);

        //This is stupid and expensive, surely there is another way, just havnt thought about it much
        //Probably should move this GetComponent call out of update
        if (target.GetComponent<playerPossession>().isPossessed() == true)
        {
            target = GameObject.FindGameObjectWithTag("Player"); //find the new possesed item and change the target on the agent controller
        }
        else
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }

        //Show the current State they are in above their head
        if (ShowState == true)
            txtState.enabled = true;
        else
            txtState.enabled = false;

        //Call update from this agents FSM
        if (m_stateMachine != null)
        {
            m_stateMachine.Update(this, Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            m_stateMachine.ChangeState(this, new GPATROL_Retreat());
        }

        if(collision.gameObject.tag == "Item")
        {
            anim.SetBool("hit", true);
            GetComponent<script_ProtonBeam_v5>().fire = false; //Stop shooting, noticed a bug that the beam would continue to fire when running away, hopefully this fixes it
            m_stateMachine.ChangeState(this, new GPATROL_Retreat());
        }
    }

    /*
    public bool isWillInTorchLight()
    {
        if (torch == null) //Incase the light doesnt exist for some reason let me know
        {
            Debug.LogError("Cannot find the Spotlight for the torch on Agent.");
        }

        Vector3 direction = target.transform.position + new Vector3(0, -1.2f, 0) - transform.position + new Vector3(0, 1.2f, 0);

        //If Will is within the Agent's torch range and angle of the spotlight
        if (direction.magnitude <= torch.range && Vector3.Angle(transform.forward, direction) <= torch.spotAngle / 2)
        {
            //Create a raycast from WILL towards the AGENT to see if the AGENT has direct line of sight
            Ray ray = new Ray(target.transform.position + new Vector3(0, 1.2f, 0), direction); //-direction
            RaycastHit[] hits = Physics.RaycastAll(ray, torch.range); //Check all the objects within the torch range

            Debug.DrawRay(target.transform.position + new Vector3(0, 1.2f, 0), -direction); //-direction

            foreach (RaycastHit hit in hits)
            {
                //Looks at all transforms in range and checks for line of sight towards will
                if (hit.transform != transform && hit.transform != target.transform && hit.distance < direction.magnitude) //If the raycast hit isnt the light or the AI casting it
                {
                    Debug.DrawRay(transform.position + new Vector3(0, 1.2f, 0), -direction, Color.red);
                    return false; //Will not found so break immediatly
                }
            }
            Debug.DrawRay(transform.position + new Vector3(0, 1.2f, 0), -direction, Color.green);
            return true; //If it reaches this point
        }
        else
        {
            return false;
        }
    }

    */

    public bool isWillInTorchLight()
    {
        if (torch == null) //Incase the light doesnt exist for some reason let me know
        {
            Debug.LogError("Cannot find the Spotlight for the torch on Agent.");
        }

        Vector3 targetAdjustedPosition = (target.transform.position + (target.transform.up * 1.1f));//aim for wills head
        Vector3 adjustedPosition = (transform.position + (transform.up * 1.4f));//aim from the GP's head


        Vector3 direction = targetAdjustedPosition - adjustedPosition;
        //height adjustments very important, without them you will not be hitting Will as it appears his center is above his actual collider

        float dot = Vector3.Dot(transform.TransformDirection(Vector3.forward).normalized, direction.normalized); //-1 = directly behind, 1=directly infront

        //just draws a  line towards Will
        //Debug.DrawRay(adjustedPosition, direction, Color.white);

        //If Will is within the Agent's torch range and angle of the spotlight
       if (direction.magnitude <= torch.range && dot > 0f)
        {
            //Create a raycast from the AGENT to will to see if the AGENT has direct line of sight
            Ray ray = new Ray(adjustedPosition, direction); //-direction
            RaycastHit[] hits = Physics.RaycastAll(ray, torch.range); //Check all the objects within the torch range

            foreach (RaycastHit hit in hits)
            {
                //Looks at all transforms in range and checks for line of sight towards will and whether they are in front of Will
                if (hit.transform.tag != "GPatrol" && hit.transform.name != "Beam" && hit.transform != target.transform && hit.distance < direction.magnitude)
                {
                    //Debug.DrawRay(adjustedPosition, direction, Color.red);
                    //Debug.Log(hit.transform.name);
                    return false;
                }
            }
            //Debug.DrawRay(adjustedPosition, direction, Color.green);
            //Debug.Log(direction.magnitude);
            return true;
        }
        else
        {
            return false;
        }
    }

    //Bool to check if a point of interest is registered - Used to switch to Alert state

    public bool CheckForPOI()
    {
        if (pointsOfInterest.Count >= 1)
            return true;
        else
            return false;
    }

    //Add a point of interest to the list
    public void AddPointOfInterest(Vector3 poi)
    {
        //only add valid positions on the navmesh
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(poi, out myNavHit, torch.range, -1))
        {
            pointsOfInterest.Add(myNavHit.position);

            //DEBUGGING for visualising the Points of interest   
            if (showPOI)
            {
                temp = Instantiate(POIPrefab);
                temp.transform.position = myNavHit.position; //Set its position 
                tempPOIList.Add(temp);
            }
        }
        else
        {
            Debug.Log("Invalid POI at: " + poi);
        }
    }

    //Returns the count for PointsOfInterest list
    public int GetPOICount()
    {
        return pointsOfInterest.Count;
    }

    //Remove the debug objects and the position from the list
    public void RemovePOI(int i)
    {
        //Agent has visited this POI      
        if (ShowState)
        {
            Destroy(tempPOIList[i]); //Destroy the debug object
            tempPOIList.RemoveAt(i); //Remove the object from the list
        }

        pointsOfInterest.RemoveAt(i); //Remove from the points of interest list
        navAgent.ResetPath(); //make sure we clear the path.
    }

    //Send over the Position from the list
    public Vector3 GetPositionFromPOI(int i)
    {
        return pointsOfInterest[i];
    }
}