using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

//#OPTIMISE - CACHE ALL OVERUSED GETCOMPONENT CALLS

public class playerPossession : MonoBehaviour
{    
    private GameObject player; //could be the real player or a possessed item
    private GameObject sneakTest; //Store old player here while possessing
    private float timer;
    private static bool resettingSidInvis;

    //Stores a reference to the current item we are possesing, used in CIV_Retreat
    public GameObject PossessedItem;
    public bool hasItemBeenThrown;
    public static Transform lastThrownItem;
    public GameObject itemThrown;

    //Used during hide for camera pivot
    [HideInInspector]public GameObject pivot;
    private static bool CamPivotSet = false;

    private static GameObject target; //Global target once that is set when we RaycastCheckItem is run
    private bool targetSet; //Flag to let us know if the raycast hit an object and set the target

    public Color possessionColour = Color.cyan;// Added by Mark - Added possession color for outline
    public float HeightAdjustment = .4f; //where to start the ray - need to align the spotlight to this position

    public float allowablePosessionRange;

    //Renamed possessed -> moveModeActive 
    private static bool moveModeActive = false;

    public bool IsPossessed()
    {
        return moveModeActive;
    }

    public bool IsHidden()
    {
        return hidden;
    }

    public float throwVelocity;
    
    private static bool hidden = false; //Determines when we can use the "Lure/Repel" ability
    private static bool lureUsed = false; //Lets us know when we have used Lure, so we can Repel afterwards only
    private static bool lureSphereCreated = false;

    //Player's position is stored here when the use the hide mechanic, so when we unhide they resume from the old position
    public static Vector3 oldPlayerPos;
    public static Quaternion oldPlayerRot;

    public float lureRange; //Range at which the lure ability will attract the ai
    [Header("Highlight Mat for Civ")]public Material highlightMat; //Material that will be set on the civillians
    [Header("LureSphere Prefab")] public GameObject lureSphere;

    //Cached script_willDisolve, Script that transitions camera and particles over to the intended object
    static script_WillDissolve disolveScript;

    //Lure/Scare particle effects references
    private GameObject lureEffect;
    private GameObject scareEffect;
    
    //Storing old speed values here to easily renable later - Jak
    struct OldSidValues
    {
        public static float speed;
        public static float floatspeed;
        public static float sinkspeed;
        public static float rigidMass;
        public static float rigidDrag;
        public static float rigidAngDrag;
    }

    //Lets us know when the values in the struct have been set
    private static bool oldSidValuesSet = false;

    //Enum to choose what direction to raycast to - Could be better?? - Used for the Quick-Drop - Jak
    public enum RayDirection
    {
        FORWARD,
        BACK, 
        LEFT,
        RIGHT,
        UP,
        DOWN,
        COUNT
    }

    //Fmod audio instances - these are set in Lure/Repel - Jak
    FMOD.Studio.EventInstance lureSound;
    FMOD.Studio.EventInstance scareSound;
    FMOD.Studio.EventInstance impactSound;

    // Use this for initialization - note that the player could be real or could be an item
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        lureRange = Camera.main.GetComponent<valueController>().lureRange;
        throwVelocity = Camera.main.GetComponent<valueController>().thrownItemVelocity;
        allowablePosessionRange = Camera.main.GetComponent<valueController>().possessionRange;

        //Set once catch, setting all the old values for the first instance of playerPossession.cs.
        //This lets us quickly unpossess, and look up the old values from the struct to return to our old "Sid" and no longer use the values from the item
        if (!oldSidValuesSet)
        {
            //Store old values in the struct for unpossesion            
            OldSidValues.speed = player.GetComponent<playerController>().speed;
            OldSidValues.floatspeed = player.GetComponent<playerController>().floatSpeed;
            OldSidValues.sinkspeed = player.GetComponent<playerController>().sinkspeed;
            OldSidValues.rigidMass = player.GetComponent<Rigidbody>().mass;
            OldSidValues.rigidDrag = player.GetComponent<Rigidbody>().drag;
            OldSidValues.rigidAngDrag = player.GetComponent<Rigidbody>().angularDrag;
            disolveScript = player.GetComponent<script_WillDissolve>();
            oldSidValuesSet = true;
        }

        //Update playerPossession Reference in gamemanager
        if(GameManager.Instance.player == null)
            GameManager.Instance.player = player.GetComponent<playerPossession>();

        //Setup the references
        lureSound = FMODUnity.RuntimeManager.CreateInstance(GameManager.Instance.audioLure);
        scareSound = FMODUnity.RuntimeManager.CreateInstance(GameManager.Instance.audioScare);
        impactSound = FMODUnity.RuntimeManager.CreateInstance(GameManager.Instance.audioItemImpact);

        sneakTest = GameObject.FindGameObjectWithTag("Sneak");
    }

    //as the code enables and disables the player, this is required to initialise the code
    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sneakTest = GameObject.FindGameObjectWithTag("Sneak");

        //Update playerPossession Reference in gamemanager
        //if(player == null)
        //    GameManager.Instance.player = player.GetComponent<playerPossession>();
    }    

    private void FixedUpdate()
    {
        if (resettingSidInvis)
        {
            timer += Time.deltaTime;
            if (timer >= 0.25f)
            {
                timer = 0;              
                resettingSidInvis = false;
                //Debug.Log("Sid Layer Reset");
            }
        }
    }

    private void Update()
    {
        if (sneakTest)
        {
            sneakTest.transform.position = player.transform.position;
            //sneakTest.transform.rotation = player.transform.rotation;
        }

        //Streamlined Controls - Jak
        //Everything is only run once an item is possessed
        //LMB -> Hide()
        //"HideMode"->LMB - > Lure()
        //"HideMode"->LMB AFTER Lure() -> Repel()
        //"HideMode -> RMB -> MoveMode(Go to PossessMode)
        //PossessMode - Free moving object
        //"PossessMode"->LMB->UnPossess/Throw()
        //"PossessMode -> E -> DropItem()
        //"PossessMode" -> RMB -> Back to Hide()

        //LMB - Possess/Throw - Jak
        if (Input.GetMouseButtonDown(0))
        {
            if (!hidden && !moveModeActive) //The hidden flag only detects when a player is hiding in an item - Jak
            {
                StartCoroutine(ParticleTransition());
            }
            else if (!hidden && moveModeActive)
            {
                StartCoroutine(ThrowPossessedItemAway());
                resettingSidInvis = true;
            }
        }

        //Lure/Repel mechanics
        if (hidden && GetComponent<Animator>() != null && GetComponent<ItemController>().isScaryObject())
        {
            if (!lureUsed && CamPivotSet)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Lure();
                }
            }
            else if (lureUsed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Repel();
                }
            }
        }

        //RMB - Possess/Revert to Hide      
        if (hidden && !moveModeActive)
        {
            if (Input.GetMouseButtonDown(1))
            {
                MoveMode(); //Allow moving the item around
            }
        }
        else if (moveModeActive)
        {
            if (Input.GetMouseButtonDown(1))
            {
                //Revert back to Hide
                Hide();
            }
        }

        //Quick Drop - Jak
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (IsHidden() || IsPossessed())
            {
                Vector3 dir = Vector3.zero;
                Vector3 startPos = transform.position;
                RaycastHit hit;
                float length = 3;
                Collider col = GetComponent<Collider>();
                Vector3 colSizeUp = transform.up * sneakTest.GetComponent<CapsuleCollider>().height / 2;
                Vector3 colSizeSide = transform.right * sneakTest.GetComponent<CapsuleCollider>().radius / 2;

                for (int i = 0; i < (int)RayDirection.COUNT; i++)
                {
                    //Pass in dir variable and assign correct direction vector based on direction passed in
                    AssignRayDirection((RayDirection)i, ref dir, ref startPos);

                    Vector3 newPos = startPos;
                    newPos = new Vector3(newPos.x + dir.x, newPos.y + dir.y, newPos.z + dir.z);

                    //Check so we dont fall in the ground on eject
                    if (newPos.y <= 1)
                        newPos = new Vector3(newPos.x, 1, newPos.z);

                    //Note test every direction extent
					
					//Maybe change the overall length of the raycast
					//Move raycast start pos over by half of the size of the collider?
					//Then raycast total length acoording to max size of the collider?
					
                    //startPos.z = startPos.z + GetComponent<Collider>().bounds.extents.z;
                    Debug.DrawRay(startPos, dir, Color.blue, 100f);
                    //Debug.DrawRay(newPos, colSizeUp, Color.green, 100f);
                    //Debug.DrawRay(newPos, -colSizeUp, Color.green, 100f);
                    //Debug.DrawRay(newPos, colSizeSide, Color.red, 100f);
                    //Debug.DrawRay(newPos, -colSizeSide, Color.red, 100f);

                    if (!Physics.Raycast(startPos, dir, out hit, length)) //Original Direction
                        if (!Physics.Raycast(newPos, colSizeUp, out hit, sneakTest.GetComponent<CapsuleCollider>().height / 2)) //Up
                            if (!Physics.Raycast(newPos, -colSizeUp, out hit, -sneakTest.GetComponent<CapsuleCollider>().height / 2)) //Down
                                if (!Physics.Raycast(newPos, colSizeSide, out hit, sneakTest.GetComponent<CapsuleCollider>().radius / 2)) //Left to capsule sise
                                    if (!Physics.Raycast(newPos, -colSizeSide, out hit, sneakTest.GetComponent<CapsuleCollider>().radius / 2)) //Right to capsule size
                                    {
                                        sneakTest.transform.position = newPos;
                                        this.GetComponent<ItemController>().SetAnimScare(false);//Stop the scare animation incase it is still playing when we eject
                                        FMODUnity.RuntimeManager.PlayOneShot("event:/Sid_Drop", transform.position);
                                        UnpossessItem();
                                        break;
                                    }
                } //End loop
            } //End If
        }//End Quick-Drop


    }//End update

    private void OnCollisionEnter(Collision collision)
    {
        if (IsPossessed())
        {
            //Sound Effect ----------------------------------
            FMOD.Studio.PLAYBACK_STATE stateImpact;
            
            impactSound.getPlaybackState(out stateImpact); //Poll the audio events to see if playback is happening

            //Check if any audio is still playing and stop it to prevent overlap - Then play the required clip
            if (stateImpact != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                impactSound.start(); // Starts the event
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(impactSound, GetComponent<Transform>(), GetComponent<Rigidbody>()); //Setup the 3D audio attributes
            }
            else //If none is playing start immediatly
            {
                impactSound.start(); // Starts the event
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(impactSound, GetComponent<Transform>(), GetComponent<Rigidbody>());
            }
            //End Sound Effect ----------------------------
        }
    }

    //Sets dir based on given rayDirection - Jak
    void AssignRayDirection(RayDirection rayDirection, ref Vector3 dir, ref Vector3 startpos)
    {
        float length = 3;

        switch (rayDirection)
        {
            case RayDirection.FORWARD:
                dir = transform.forward * length;
                //startpos.z = startpos.z + GetComponent<Collider>().bounds.extents.z;
                break;
            case RayDirection.BACK:
                dir = -transform.forward * length;
                //startpos.z = startpos.z - GetComponent<Collider>().bounds.extents.z * 2;
                break;
            case RayDirection.LEFT:
                dir = -transform.right * length;
                //startpos.x = startpos.x -GetComponent<Collider>().bounds.extents.x * 2;
                break;
            case RayDirection.RIGHT:
                dir = transform.right * length;
                //startpos.x = startpos.x + GetComponent<Collider>().bounds.extents.x * 2;
                break;
            case RayDirection.UP:
                dir = transform.up * length;
                //startpos.y = startpos.y + GetComponent<Collider>().bounds.extents.y * 2;
                break;
            case RayDirection.DOWN:
                dir = -transform.up * length;
                //startpos.y = startpos.y - GetComponent<Collider>().bounds.extents.y * 2;
                break;
            default:
                break;
        }
    }

    //Enabling the movement on the Possessed object that we control, splitting up "PossessItem" - Jak
    void MoveMode()
    {
        //We are no longer hidden
        hidden = false;

        //Reset the lureused
        lureUsed = false;

        //Change UI
        if (target.GetComponent<ItemController>().isScaryObject())
        {
            GameManager.Instance.EnableHideScaryLure(false);
            GameManager.Instance.EnableHideScary(false);
            GameManager.Instance.EnableMoveMode(true);
        }
        else
        {
            GameManager.Instance.EnableHideNonScary(false);
            GameManager.Instance.EnableMoveMode(true);
        }

        //Turn off camera while we update the target with required values
        Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

        //Remove lure sphere
        if (lureSphereCreated)
        {
            Destroy(target.GetComponentInChildren<TriggerHighlight>().gameObject);
            lureSphereCreated = false;
        }

        //Enable movement         
        target.GetComponent<playerController>().enabled = true;

        //Switch off gravity for the target and redo the rigidbody values so they are correct for movement
        target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        target.GetComponent<Rigidbody>().drag = OldSidValues.rigidDrag;
        target.GetComponent<Rigidbody>().angularDrag = OldSidValues.rigidAngDrag;
        target.GetComponent<Rigidbody>().useGravity = false;
        
        //Switch off the hide aura
        target.GetComponentInChildren<Renderer>().material.SetFloat("_AuraOnOff", 0);
        target.GetComponentInChildren<Renderer>().material.SetColor("_ASEOutlineColor", Color.yellow);

        //Revert Camera back
        Camera.main.transform.SetParent(null);
        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().target = target.transform;

        //Testing camera snap fix - now the snap bug should only happen when going into move mode
        Camera.main.gameObject.GetComponent<CamLock>().currentHorizontal = target.transform.eulerAngles.y;
        Camera.main.gameObject.GetComponent<CamLock>().currentVertical = target.transform.eulerAngles.x;

        //switch the camera back on to follow the player
        Camera.main.gameObject.GetComponent<CamLock>().enabled = true;

        //Stop the scare animation incase it is still playing when we eject
        this.GetComponent<ItemController>().SetAnimScare(false);

        moveModeActive = true;
    }

    //This method is now just the adding and pre-setup to becoming an item
    //Hide(), MoveMode() will be controling the specific actions of what the item can do
    //Mainly written by Ben - Jak tweaked here and there when restructuring player controls
    void PossessItem() 
    {
        if (player.GetComponent<playerController>().GetEctoplasm > 0.0f)
        {
            Camera.main.GetComponent<CamLock>().floatSpeedOfSid = player.GetComponent<playerController>().floatSpeed;

            //At this point playerPossesion 'should' be attached to the player so minus the ecto cost
            this.GetComponent<playerController>().GetEctoplasm -= target.GetComponent<ItemController>().ectoCost; //Deducts the amount of ectoplasm based on item thrown - Ben

            //rename the player tag so they dont participate in any collisions
            player.tag = "Sneak";

            //name it player so that it behaves like one in collisions
            target.tag = "Player";
            
            SkinnedMeshRenderer[] meshRenderer = player.GetComponentsInChildren<SkinnedMeshRenderer>();
            
            //turn off the real players renderer
            foreach (SkinnedMeshRenderer smr in meshRenderer)
            {
                if (smr.transform.name != "geo_willTongue_low")
                    smr.enabled = false;
            }

            //Disable old player scripts because we are becoming a new item
            player.GetComponent<playerController>().enabled = false;
            player.GetComponent<playerPossession>().PossessedItem = target.gameObject;
            itemThrown = target.gameObject;        
            player.GetComponent<playerPossession>().enabled = false;
            //oldColliderHeight = player.GetComponent<CharacterController>().height;
            //oldColliderRadius = player.GetComponent<CharacterController>().radius;
            //player.GetComponent<CharacterController>().enabled = false;
            player.GetComponent<CapsuleCollider>().enabled = false;
            player.GetComponent<script_ToonShaderFocusOutline>().enabled = false; // Added by Mark - Disable toon focus outline script on player so it stops annoying me

            //set up the new possesed object with these scripts
            //playerController, playerPossession, are all disabled here because the other methods decides what to do with them
            target.AddComponent<playerController>();
            playerController playerController = target.GetComponent<playerController>();

            //Carry over all sid values to the item
            playerController.GetEctoplasm = player.GetComponent<playerController>().GetEctoplasm;
            playerController.speed = OldSidValues.speed;
            playerController.floatSpeed = OldSidValues.floatspeed;
            playerController.sinkspeed = OldSidValues.sinkspeed;
            target.GetComponent<Rigidbody>().mass = OldSidValues.rigidMass;
            target.GetComponent<Rigidbody>().drag = OldSidValues.rigidDrag;
            target.GetComponent<Rigidbody>().angularDrag = OldSidValues.rigidAngDrag;
            playerController.enabled = false;

            target.AddComponent<playerPossession>();                           
            target.GetComponent<playerPossession>().highlightMat = highlightMat;//Copy the material reference over           
            target.GetComponent<playerPossession>().lureSphere = lureSphere; //Copy the prefab reference over
            target.GetComponent<playerPossession>().enabled = false;

            //target.AddComponent<CharacterController>();
            //target.GetComponent<CharacterController>().height = 0.01f;
            //target.GetComponent<CharacterController>().radius = 0.01f;
            //target.GetComponent<CharacterController>().enabled = false;

            Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().target = target.transform;

            //Adjusts camera distance based on item size
            if (target.GetComponent<ItemController>().itemSize == ItemController.Size.Miniature)
            {
                Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance = 1.95f;
                Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().targetLookAtOffset = new Vector3(0, 1.25f, 1.25f);
            }
            else if (target.GetComponent<ItemController>().itemSize == ItemController.Size.Small)
            {
                Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance = 2.25f;
                Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().targetLookAtOffset = new Vector3(0, 1, 1);
            }

            else if (target.GetComponent<ItemController>().itemSize == ItemController.Size.Large)
            {
                Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance = 7.0f;
            }
            else if (target.GetComponent<ItemController>().itemSize == ItemController.Size.Massive)
            {
                Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance = 11.0f;
                Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().targetLookAtOffset = new Vector3(0, 20, 20);
                Quaternion temp = gameObject.transform.rotation;
                temp.x = -10;
                gameObject.transform.rotation = temp;


            }

            //Tell the other scripts we are now possessed
            moveModeActive = true;        
        }
    }

    //Written by Ben
    IEnumerator ThrowPossessedItemAway()
    {
        //throw the object;
        //may need to identify that the object was "hitby" will so that it will register a point of interest when it colides with something.
        gameObject.GetComponent<playerPossession>().hasItemBeenThrown = true;
        gameObject.GetComponent<ItemController>().hasBeenThrown = true;
        UnpossessItem();
        player.GetComponent<Rigidbody>().velocity = player.GetComponent<Rigidbody>().transform.forward * throwVelocity;        
        lastThrownItem = this.gameObject.transform;
        yield return new WaitForSeconds(0);
    }

    //Written by Ben - Jak tweaked here and there when restructuring player controls
    public void UnpossessItem()
    {
        //Remove lure sphere incase it still exists
        if (lureSphereCreated)
        {
            Destroy(player.GetComponentInChildren<TriggerHighlight>().gameObject);
            lureSphereCreated = false;
        }

        //turn this item back into a regular item
        //At this point the player reference has changed it is the Possessed Item
        player.tag = "Item";
        player.GetComponent<Rigidbody>().drag = 0;
        player.GetComponent<Rigidbody>().angularDrag = 0;
        player.GetComponent<Rigidbody>().useGravity = true;
        player.GetComponent<Rigidbody>().freezeRotation = false; //Added by Jak - 13/11/17
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None; //Added by Jak - 4/12/17

        //Switch off the hide aura
        player.GetComponentInChildren<Renderer>().material.SetFloat("_AuraOnOff", 0);
        player.GetComponentInChildren<Renderer>().material.SetColor("_ASEOutlineColor", Color.black);

        Destroy(player.GetComponent<playerController>());
        //Destroy(player.GetComponent<CharacterController>());

        //player.GetComponent<ItemController>().enabled = true; //Added by Jak - 4/12/17
        //player.GetComponent<ItemController>().hasBeenThrown = true;

        //disable the items
        player.GetComponent<playerPossession>().enabled = false;

        eRenderer = player.GetComponentInChildren<Renderer>();
        mat = eRenderer.material; //let the code know which objects renderer to change
        mat.SetColor("_Color", Color.white);// Added by Mark - Change main colour of object back to white
        mat.SetColor("_OutlineColor", Color.black);// Added by Mark - Change  colour of object outline back to black

        //Reset variables - Jak
        lureUsed = false;
        moveModeActive = false;
        hidden = false;
        PossessedItem = null;

        //Disabling UI - just calling all of them for now, i could easily add a lastCalled variable
        //and just disable that one, but i think its fine for the small amount we have currently - Jak
        GameManager.Instance.EnableHideNonScary(false);
        GameManager.Instance.EnableHideScary(false);
        GameManager.Instance.EnableHideScaryLure(false);
        GameManager.Instance.EnableItemSelect(false);
        GameManager.Instance.EnableMoveMode(false);

        //switch off the camera tracking whilst we reset the player back to what it is supposed to be
        Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

        sneakTest.tag = "Player";//need to do this here as the agent code needs a player at all times
        //sneakTest.transform.position = oldPlayerPos;
        sneakTest.GetComponent<CapsuleCollider>().enabled = true;

        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().distance = 3.0f;
        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().targetLookAtOffset = new Vector3(0, 1, 1);
        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().target = sneakTest.transform;

        EnablePlayer();//re-enable Player after a short time at this position  needed so that Player does not colide with the object he is unposessing
    }

    //written by Jak
    public void Hide()
    {
        moveModeActive = false;

        //Disable camera rotation
        Camera.main.GetComponent<CamLock>().enabled = false; //This is renabled when the object has stopped moving in update

        playerPossession possessedItem = target.GetComponent<playerPossession>();
        possessedItem.enabled = true; //Enable the item playerPossesion script

        //Disable movement
        possessedItem.GetComponent<playerController>().enabled = false;

        //switch off gravity for the target
        possessedItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        possessedItem.GetComponent<Rigidbody>().drag = 0; //set drag to 0 so item falls like a sack of potatoes without affecting gravity
        possessedItem.GetComponent<Rigidbody>().angularDrag = 0;
        possessedItem.GetComponent<Rigidbody>().useGravity = true;
        possessedItem.GetComponent<Rigidbody>().freezeRotation = true; //Freeze item rotation while possesed, caused the camera to glitch - Jak - 13/11/17

        possessedItem.GetComponentInChildren<Renderer>().material.SetFloat("_AuraOnOff", 1);
        possessedItem.GetComponentInChildren<Renderer>().material.SetColor("_ASEOutlineColor", Color.black);

        //Camera Pivot setup
        //Create pivot object for camera orbiting - check when rigidbody is grounded, then add - might need to move this to update function
        pivot = new GameObject("Pivot");
        pivot.transform.position = possessedItem.transform.position;
        pivot.transform.rotation = possessedItem.transform.rotation;
        pivot.transform.SetParent(possessedItem.transform);

        //Child camera to pivot point
        Camera.main.transform.SetParent(pivot.transform);
        
        //Spawn lureSphere
        if (possessedItem.GetComponent<ItemController>().isScaryObject() && !lureSphereCreated)
        {
            Instantiate(lureSphere, possessedItem.transform);
            lureSphereCreated = true;
        }

        //Renable rotation while falling
        Camera.main.GetComponent<CamLock>().enabled = true;
        //End Camera Orbit

        //Change UI
        GameManager.Instance.EnableHideScaryLure(false);
        GameManager.Instance.EnableMoveMode(false);

        if (possessedItem.GetComponent<ItemController>().isScaryObject())
            GameManager.Instance.EnableHideScary(true);
        else
            GameManager.Instance.EnableHideNonScary(true);
        //End UI

        hidden = true; //set that we are now hidden in an object
        CamPivotSet = true;
    }

    //#OPTIMISE - These two methods both run physics.overlap, possibly merge that call into one list for use in repel, and if repel detects that list empty, then run its own?
    //Gotta think about how id do it, what happens if list has 1, but then 4 AI walk in during a repel...
    void Lure() //Written by Jak
    {
        //Change UI
        GameManager.Instance.EnableHideScary(false);
        GameManager.Instance.EnableHideScaryLure(true);

        //Destroy the scareEffect incase it hasnt already.
        if (scareEffect != null)
            Destroy(scareEffect);

        //Create particle effect
        lureEffect = Instantiate(GameObject.Find("PrefabController").GetComponent<PrefabController>().lureEffect, this.gameObject.transform);
        lureEffect.transform.localPosition = new Vector3(0, 0, 0);

        //Sound Effect ----------------------------------
        FMOD.Studio.PLAYBACK_STATE stateLure;
        FMOD.Studio.PLAYBACK_STATE stateScare;
        lureSound.getPlaybackState(out stateLure); //Poll the audio events to see if playback is happening
        scareSound.getPlaybackState(out stateScare);

        //Check if any audio is still playing and stop it to prevent overlap - Then play the required clip
        if (stateLure == FMOD.Studio.PLAYBACK_STATE.PLAYING || stateScare == FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            lureSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            scareSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            lureSound.start(); // Starts the event
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(lureSound, GetComponent<Transform>(), GetComponent<Rigidbody>()); //Setup the 3D audio attributes
        }
        else //If none is playing start immediatly
        {
            lureSound.start(); // Starts the event
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(lureSound, GetComponent<Transform>(), GetComponent<Rigidbody>());
        }
        //End Sound Effect ----------------------------

        //Lure Enemies to us
        //#OPTIMISE //Refactor this so it finds tags first instead of all colliders
        //Or maybe have an already populated list of civs on startup that we loop through and check if in range?
        Collider[] civillians = Physics.OverlapSphere(transform.position, lureRange);

        //Sample this object position before sending it to the ai
        NavMeshHit navHit;
        NavMesh.SamplePosition(this.gameObject.transform.position, out navHit, lureRange, -1);

        //Check for any civs in the radius and "lure" them to us
        foreach (Collider civ in civillians)
        {
            if (civ.tag == "Civillian")
            {
                CivillianController civillian = civ.GetComponent<CivillianController>();

                if (civillian.currentState != State.State_Retreat) //Only LURE the CIVS if they arent already in a retreat state / Prevents spam
                {                    
                    civillian.itemPosition = navHit.position;
                    civillian.alertedByItem = true;
                }
            }
        }

        Destroy(lureEffect, 2.0f);

        lureUsed = true;        
    }

    void Repel()//Written by Jak
    {
        //Change UI
        GameManager.Instance.EnableHideScaryLure(false);
        GameManager.Instance.EnableHideScary(true);

        //Destroy the lureEffect incase it hasnt already.
        if (lureEffect != null)
            Destroy(lureEffect);

        //Spawn scareEffect
        scareEffect = Instantiate(GameObject.Find("PrefabController").GetComponent<PrefabController>().scareEffect, this.gameObject.transform);
        scareEffect.transform.localPosition = new Vector3(0, 0, 0);

        //Play scare animation
        GetComponent<ItemController>().SetAnimScare(true);

        //Sound Effect ----------------------------------
        FMOD.Studio.PLAYBACK_STATE stateLure;
        FMOD.Studio.PLAYBACK_STATE stateScare;
        lureSound.getPlaybackState(out stateLure); //Poll the audio events to see if playback is happening
        scareSound.getPlaybackState(out stateScare);

        //Check if any audio is still playing and stop it to prevent overlap - Then play the required clip
        if (stateLure == FMOD.Studio.PLAYBACK_STATE.PLAYING || stateScare == FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            lureSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            scareSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            scareSound.start(); // Starts the event
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(scareSound, GetComponent<Transform>(), GetComponent<Rigidbody>()); //Setup the 3D audio attributes
        }
        else //If none is playing start immediatly
        {
            scareSound.start(); // Starts the event
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(scareSound, GetComponent<Transform>(), GetComponent<Rigidbody>());
        }
        //End Sound Effect ----------------------------

        //Get all colliders
        //#OPTIMISE
        Collider[] civillians = Physics.OverlapSphere(transform.position, lureRange); //Refactor this so it finds tags first instead of all colliders

        foreach (Collider civ in civillians)
        {
            if (civ.tag == "Civillian")
            {
                //I change the target gameobject in the civillians, so we can easily access the ItemScaryRating
                //The target is also used in CIV_Retreat to know which item to run away from
                CivillianController civillian = civ.GetComponent<CivillianController>();
                if (civillian.currentState != State.State_Retreat) //Only LURE the CIVS if they arent already in a retreat state / Prevents spam
                {
                    civillian.target = gameObject;
                    civillian.TRIGGERED_repel = true;

                    if (civillian.GetComponent<NavMeshObstacle>() == null)
                        continue;
                    else
                        civillian.GetComponent<NavMeshObstacle>().enabled = false;

                    civillian.GetComponent<NavMeshAgent>().enabled = true;
                }
            }
        }

        Destroy(scareEffect, 2.0f);

        lureUsed = false;
        //End Repel - Jak
    }

    //enable player
    void EnablePlayer()
    {
        //re enable the real player
        SkinnedMeshRenderer[] meshRenderer = sneakTest.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer smr in meshRenderer)
        {
            smr.enabled = true;
            smr.material = Camera.main.GetComponent<GameManager>().sidSkin;
        }

        //sneakTest.transform.position = player.transform.position;
        sneakTest.GetComponent<playerController>().speed = OldSidValues.speed;
        sneakTest.GetComponent<playerController>().enabled = true;

        sneakTest.GetComponent<playerPossession>().PossessedItem = null; //Reset this object now that we are nolonger possessing something
        sneakTest.GetComponent<playerPossession>().enabled = true;

        //Destroy the old possession componenet on the last possessed object
        Destroy(player.GetComponent<playerPossession>());

        //sneakTest.GetComponent<CapsuleCollider>().enabled = true;
        //sneakTest.GetComponent<CharacterController>().height = oldColliderHeight;
        //sneakTest.GetComponent<CharacterController>().radius = oldColliderRadius;
        //sneakTest.GetComponent<CharacterController>().enabled = true;

        sneakTest.GetComponent<script_ToonShaderFocusOutline>().enabled = true;// Added by Mark - Reenable outline focus script on sid

        Camera.main.gameObject.GetComponent<CamLock>().enabled = true;
    }

    //This ONLY returns if an ITEM has been hit - I can make this general if need be - Jak
    //Stores the ITEM that was hit in "target" gameobject
    void RaycastCheckItem()
    {
        //try to posess an item
        RaycastHit hit;
        Vector3 adjustedPlayerPosition = player.transform.position + (player.transform.up * HeightAdjustment); //adjust beacuse the players pivot point is at its base

        Ray testRay = new Ray(adjustedPlayerPosition, player.transform.forward);
        Ray secondTest = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        //Debug.DrawRay(adjustedPlayerPosition, player.transform.forward * allowablePosessionRange, Color.yellow ,3f);

        if (Physics.Raycast(secondTest, out hit, allowablePosessionRange))
        {
            if(hit.transform.tag == "Item" && hit.transform.GetComponent<ItemController>().timesThrown <= hit.transform.GetComponent<ItemController>().timesThrownBeforeDestroyed)
            {                
                target = hit.collider.gameObject;
                //Debug.DrawLine(Camera.main.transform.position, hit.point, Color.green, 100f);
                targetSet = true;
            }
            else
            {
                target = null;
                targetSet = false;
            }
        }
    }

    //Coroutines
    IEnumerator ParticleTransition() //Written by Jak
    {
        if (targetSet == false)
            RaycastCheckItem();

        if (targetSet == true && disolveScript.transferring == false)
        {
            disolveScript.transferring = true;
            disolveScript.target = target;            
            disolveScript.startDissolve = true;  //Start the particle transition

            //Stop movement on sid while transitioning
            GameManager.Instance.EnableItemSelect(false); //Incase "Hide Image" is still up
            gameObject.GetComponent<script_ToonShaderFocusOutline>().enabled = false;
            gameObject.GetComponent<playerController>().speed = 0;
            gameObject.GetComponent<playerController>().enabled = false;
            //gameObject.GetComponent<playerPossession>().enabled = false;
            //gameObject.GetComponent<CharacterController>().enabled = false;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Sid_Possession", transform.position);

            Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

            //Wait until the animation is finished
            while (disolveScript.transferred != true) //This will change to true in script_WillDisolve once the animation is done
            {
                yield return null; // wait until next frame
            }

            //PossessItem() sets up the target item with all the scripts, then we immediatly call Hide()
            PossessItem();
            Hide();
           
            //Reset variables for next animation
            targetSet = false;
            disolveScript.target = null;
            disolveScript.transferred = false;
            disolveScript.transferring = false;

            Camera.main.gameObject.GetComponent<CamLock>().enabled = true; //This will also Re-find the new player which is now the item
        }
    }    

    private Renderer eRenderer;
    private Material mat;

    private void EmitColour(Color color, float emission)
    {
        //allow for combos
        mat.EnableKeyword("_EMISSION");
        Color baseColor = color; //Replace this with whatever you want for your base color at emission level '1'
        Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
        mat.SetColor("_EmissionColor", finalColor);
        mat.SetColor("_OutlineColor", possessionColour);// Added by Mark - Change color of outline to possession color
        mat.SetColor("_Color", color);// Added by Mark - Change main colour of object to possession color
    }

    private void EmitNothing()
    {
        mat.DisableKeyword("_EMISSION");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(0, transform.position.y / 2, 0), lureRange);
    }

}
