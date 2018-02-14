using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

//#OPTIMISE - CACHE ALL OVERUSED GETCOMPONENT CALLS

public class playerPossession : MonoBehaviour
{
    private GameObject player; //could be the real player or a possessed item
    private GameObject sneakTest;

    //Stores a reference to the current item we are possesing, used in CIV_Retreat
    public GameObject PossessedItem;
    public bool hasItemBeenThrown;
    public static Transform lastThrownItem;

    //Used during hide for camera pivot
    [HideInInspector]public GameObject pivot;
    private static bool CamPivotSet = false;

    private static GameObject target; //Global target once that is set when we RaycastCheckItem is run
    private bool targetSet; //Flag to let us know if the raycast hit an object and set the target

    public Color possessionColour = Color.cyan;// Added by Mark - Added possession color for outline
    public float HeightAdjustment = .4f; //where to start the ray - need to align the spotlight to this position

    public float allowablePosessionRange = 10;

    private static bool possessed = false;

    public bool isPossessed()
    {
        return possessed;
    }

    public bool isHidden()
    {
        return hidden;
    }

    public float throwVelocity = 30;
    
    private static bool hidden = false; //Determines when we can use the "Lure/Repel" ability
    private static bool lureUsed = false; //Lets us know when we have used Lure, so we can Repel afterwards only
    bool click = false;
    //Player's position is stored here when the use the hide mechanic, so when we unhide they resume from the old position
    public static Vector3 oldPlayerPos;
    public static Quaternion oldPlayerRot;

    //Store old speed here to easily renable later
    float oldSpeed;

    public int lureRange = 10; //Range at which the lure ability will attract the ai

    //Cached script_willDisolve, Script that sends camera and particles over to the intended object
    script_WillDissolve disolveScript;


    // Use this for initialization - note that the player could be real or could be an item
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        disolveScript = player.GetComponent<script_WillDissolve>();

        //Set old speed to revert to after hide
        oldSpeed = player.GetComponent<playerController>().speed;

        //Update playerPossession Reference in gamemanager
        GameManager.Instance.player = player.GetComponent<playerPossession>();

        //This doesnt exist yet???
        sneakTest = GameObject.FindGameObjectWithTag("Sneak");
    }

    //as the code enables and disables the player, this is required to initialise the code
    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sneakTest = GameObject.FindGameObjectWithTag("Sneak");

        //Update playerPossession Reference in gamemanager
        //GameManager.Instance.player = player.GetComponent<playerPossession>();
    }

    private void Update()
    {
        if (sneakTest)
        {
            sneakTest.transform.position = player.transform.position;
            sneakTest.transform.rotation = player.transform.rotation;
        }

        //Streamlined Controls
        //Everything is only run once an item is possessed
        //LMB -> Possess()
        //"PossessMode"->LMB->UnPossess()
        //"PossessMode -> RMB -> Hide()
        //"HideMode"->LMB - > Lure()
        //"HideMode"->LMB AFTER Lure() -> Repel()
        //"HideMode -> RMB -> UnHide(Revert to PossessMode)

        //LMB - Possess/Throw
        if (Input.GetMouseButtonDown(0))
        {
            //Moved possessItem() around to left click - Jak
            if (!possessed) //The hidden flag only detects when a player is hiding in an item, not POSSESSED - Jak
            {
                StartCoroutine(ParticleTransition());//PossessItem();
            }
            else if (possessed && !hidden)
            {
                player.layer = 8;
                StartCoroutine(ThrowPossessedItemAway());
            }
        }

        //Lure/Repel mechanics
        if (possessed && hidden && !lureUsed)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Lure();
            }
        }
        else if(lureUsed)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Repel();
            }
        }

        //RMB - Hide/Unhide      
        if (possessed && !hidden)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Hide();
            }
        }
        else if (possessed && hidden)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Unhide");
                Unhide(); //Revert back to possess
            }
        }

        //Camera Pivot during hide
        //Outside of hide, because if the object is falling, we have to wait before we add the pivot point
        //This could cause issues if the object is always moving
        //At this point, HIDE has been called, therefore the script is no longer on player, but the ITEM - so player = item
        if (hidden && player.GetComponent<Rigidbody>().IsSleeping() == true && !CamPivotSet)
        {
            Debug.Log(player.name + " grounded");
            
            //Create pivot object for camera orbiting - check when rigidbody is grounded, then add - might need to move this to update function
            pivot = new GameObject("Pivot");
            pivot.transform.position = player.transform.position;

            //Child camera to pivot point
            Camera.main.transform.SetParent(pivot.transform);

            //Renable rotation while falling
            Camera.main.GetComponent<CamLock>().enabled = true;

            CamPivotSet = true;
        }
    }

    // Update is called once per frame
    void PossessItem()
    {
        if (player.GetComponent<playerController>().Ectoplasm > 0.0f)
        {
            Camera.main.GetComponent<CamLock>().floatSpeedOfSid = player.GetComponent<playerController>().floatSpeed;
               
            //Super quick fix to give inf ecto in other test scenes
            //if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level_1"))

            //At this point playerPossesion 'should' be attached to the player so minus the ecto cost
            this.GetComponent<playerController>().Ectoplasm -= target.GetComponent<ItemController>().ectoCost; //Deducts the amount of ectoplasm based on item thrown - Ben

            ////disable camera whilst we change the cameras target to the newly possesed item
            //Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

            //rename the player tag so they dont participate in any collisions
            player.tag = "Sneak";
            //player.GetComponent<CapsuleCollider>().enabled = false;

            //set the taget to what was hit in the raycast
            //GameObject target = hit.collider.gameObject;

            //name it player so that it behaves like one in collisions
            target.tag = "Player";

            //oldPlayerPos = gameObject.transform.position;

            //change the colour so we know we have selected it
            eRenderer = target.GetComponentInChildren<Renderer>();
            mat = eRenderer.material;
            //EmitColour(Color.green, .5f);

            //turn off the real players renderer, colliders and control scripts
            SkinnedMeshRenderer[] meshRenderer = player.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer smr in meshRenderer)
            {
                if (smr.transform.name != "geo_willTongue_low")
                    smr.enabled = false;
            }

            //if (player.GetComponent<CapsuleCollider>())
            //    player.GetComponent<CapsuleCollider>().enabled = false;

            player.GetComponent<playerController>().enabled = false;
            player.GetComponent<playerPossession>().PossessedItem = target.gameObject; //Added by Jak, setting the newly added playerPossession scipt to a new target
            player.GetComponent<playerPossession>().enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            player.GetComponent<script_ToonShaderFocusOutline>().enabled = false; // Added by Mark - Disable toon focus outline script on player so it stops annoying me

            //set up the target with these scripts
            if (target.GetComponent<playerController>() == null)
                target.AddComponent<playerController>();
            else
                target.GetComponent<playerController>().enabled = true;

            target.GetComponent<playerController>().Ectoplasm = player.GetComponent<playerController>().Ectoplasm;

            if (target.GetComponent<playerPossession>() == null)
            {
                target.AddComponent<playerPossession>();
            }
            else
            {
                target.GetComponent<playerPossession>().enabled = true;
            }

            //Add the Character controller so we can move the item - Jak - 13/11/17
            if (target.GetComponent<CharacterController>() == null)
            {
                target.AddComponent<CharacterController>();
            }

            //target.GetComponent<CharacterController>().enabled = true;

            //switch off gravity for the target
            target.GetComponent<Rigidbody>().useGravity = false;
            target.GetComponent<Rigidbody>().freezeRotation = true; //Freeze item rotation while possesed, caused the camera to glitch - Jak - 13/11/17

            //turn off the item controller script
            target.GetComponent<ItemController>().enabled = true; //Added by Jak - 4/12/17

            ////switch the camera back on to follow the player
            //Camera.main.gameObject.GetComponent<CamLock>().enabled = true;
            target.GetComponent<playerController>().floatSpeed = Camera.main.GetComponent<CamLock>().floatSpeedOfSid;

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
                Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance = 7.0f;

            possessed = true;
            Camera.main.gameObject.GetComponent<CamLock>().enabled = true;
            //Debug.Log(Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance);            
        }
    }

    IEnumerator ThrowPossessedItemAway()
    {
        //throw the object;
        //may need to identify that the object was "hitby" will so that it will register a point of interest when it colides with something.

        UnpossessItem();
        player.GetComponent<Rigidbody>().velocity = player.GetComponent<Rigidbody>().transform.forward * throwVelocity;
        sneakTest.GetComponent<playerPossession>().hasItemBeenThrown = true;
        lastThrownItem = this.gameObject.transform;
        yield return new WaitForSeconds(1.00f);

        Debug.Log("layer change hit");
        player.layer = 0;
    }

    public void UnpossessItem()
    {
        //turn this item back into a regular item
        //At this point the player reference has changed is the Possessed Item
        player.tag = "Item";
        player.GetComponent<Rigidbody>().useGravity = true;
        player.GetComponent<Rigidbody>().freezeRotation = false; //Added by Jak - 13/11/17
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None; //Added by Jak - 4/12/17

        Destroy(player.GetComponent<playerController>());
        Destroy(player.GetComponent<CharacterController>());

        player.GetComponent<ItemController>().enabled = true; //Added by Jak - 4/12/17
        player.GetComponent<ItemController>().hasBeenThrown = true;

        //disable the items
        player.GetComponent<playerPossession>().enabled = false;

        eRenderer = player.GetComponentInChildren<Renderer>();
        mat = eRenderer.material; //let the code know which objects renderer to change
        mat.SetColor("_Color", Color.white);// Added by Mark - Change main colour of object back to white
        mat.SetColor("_OutlineColor", Color.black);// Added by Mark - Change  colour of object outline back to black
        //player.GetComponent<ItemController>().TriggerHitByPlayerRemotely(); //this way the item thinks it is hit by the playr and can participate in scoring.

        //Invoke("EmitNothing", 2f);//this reset the emission to off
        possessed = false;
        PossessedItem = null;

        //switch off the camera tracking whilst we reset the player back to what it is supposed to be
        Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

        sneakTest.tag = "Player";//need to do this here as the agent code needs a player at all times
        //sneakTest.transform.position = oldPlayerPos;
        //sneakTest.GetComponent<CapsuleCollider>().enabled = true;
        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().distance = 3.0f;
        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().targetLookAtOffset = new Vector3(0, 1, 1);
        //Debug.Log(Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance);
        Invoke("EnablePlayer", .25f);//re-enable Player after a short time at this position  needed so that Player does not colide with the object he is unposessing
    }

    //Written by Jak
    void Unhide()
    {
        //Debug.Log("Unhide Called.");
        //disable camera while we switch back to the real player
        Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

        //rename the player
        //sneakTest.tag = "Player";

        //set the taget to what was hit in the raycast
        //GameObject target = this.gameObject; //Target equals the current object that this script is on(aka ITEM at this stage)

        //Rename the item
        //target.tag = "Item";

        //Set the current position of the player back to the oldPosition from when they hid
        //sneakTest.transform.position = oldPlayerPos;
        //sneakTest.transform.rotation = oldPlayerRot;

        //Turn off hide effect
        target.GetComponentInChildren<Renderer>().material.SetFloat("_AuraOnOff", 0);
        target.GetComponentInChildren<Renderer>().material.SetColor("_ASEOutlineColor", Color.yellow);

        //Revert Camera back, possibly no longer parent if ben implements those camera changes
        Camera.main.transform.SetParent(sneakTest.transform);
        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().target = sneakTest.transform;
        Destroy(target.GetComponent<playerPossession>().pivot);

        ////turn onn all player scripts
        //foreach (Behaviour childCompnent in target.GetComponentsInChildren<Behaviour>())
        //{
        //    if (childCompnent.tag != "MainCamera")
        //        childCompnent.enabled = true;
        //}

        
        target.GetComponent<playerController>().speed = 5;
        target.GetComponent<playerController>().enabled = true;
        target.GetComponent<CharacterController>().enabled = true;

        //switch on gravity for the target
        target.GetComponent<Rigidbody>().useGravity = false;
        target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; //Freeze item rotation while possesed, caused the camera to glitch - Jak - 13/11/17

        //switch the camera back on to follow the player
        Camera.main.gameObject.GetComponent<CamLock>().enabled = true;

        //Stop the scare animation if it is still playing when we eject
        this.GetComponent<ItemController>().scare = false;

        //Tell everyone that SID is no longer hidden
        hidden = false;

        //destroy this script instance from the ITEM
        //Destroy(target.GetComponent<playerPossession>());
    }

    //written by Jak - copypasted some stuff from "PossessItem()"
    public void Hide()
    {
        Debug.Log("Hide Called");
        //disable camera whilst we change the cameras target to the newly possesed item        
        //Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().enabled = false;

        //rename the player tag so they dont participate in any collisions
        //player.tag = "Sneak";

        //Debug.Log("HIDE called at: " + Time.time);

        ////name it player so that it behaves like one in collisions
        //target.tag = "Player";

        ////At this point - playerPossession.cs is still active on Player
        //oldPlayerPos = gameObject.transform.position;
        //oldPlayerRot = gameObject.transform.rotation;

        //turn off all player scripts
        //foreach (Behaviour childCompnent in player.GetComponentsInChildren<Behaviour>())
        //{
        //    if (childCompnent.tag != "MainCamera")
        //        childCompnent.enabled = false;
        //}

        //this.enabled = true;

        //////turn off all player renderers
        //SkinnedMeshRenderer[] meshRenderer = player.GetComponentsInChildren<SkinnedMeshRenderer>();

        //foreach (SkinnedMeshRenderer smr in meshRenderer)
        //{
        //    smr.enabled = false;
        //}

        ////Turn off colliders
        //player.GetComponent<CapsuleCollider>().enabled = false;
        //player.GetComponent<CharacterController>().enabled = false;

        hidden = true; //set that we are now hidden in an object

        //Enable hide aura
        //By the time Hide is called, SneakTest is now the "Player" and Player is now the PossessedItem
        playerPossession possessedItem = player.GetComponent<playerPossession>();
        possessedItem.GetComponentInChildren<Renderer>().material.SetFloat("_AuraOnOff", 1);
        possessedItem.GetComponentInChildren<Renderer>().material.SetColor("_ASEOutlineColor", Color.black);

        //Disable movement
        possessedItem.GetComponent<CharacterController>().enabled = false;
        possessedItem.GetComponent<playerController>().enabled = false;

        //Disable rotation while falling
        Camera.main.GetComponent<CamLock>().enabled = false;

        //////switch off gravity for the target
        possessedItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        possessedItem.GetComponent<Rigidbody>().useGravity = true;
        possessedItem.GetComponent<Rigidbody>().freezeRotation = true; //Freeze item rotation while possesed, caused the camera to glitch - Jak - 13/11/17

        CamPivotSet = false;

        //if (target.GetComponent<playerPossession>() == null)
        //    target.AddComponent<playerPossession>();

        //target.GetComponent<playerPossession>().PossessedItem = target;

        //this.enabled = false;

        //switch the camera back on to follow the player
       // Camera.main.gameObject.GetComponent<CamLock>().enabled = true;

        //Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().enabled = true;


        //Debug.Log("Hide finished: " + Time.time);
    }

    //#OPTIMISE - These two methods both run physics.overlap, possibly merge that call into one list for use in repel, and if repel detects that list empty, then run its own?
    //Gotta think about how id do it, what happens if list has 1, but then 4 AI walk in during a repel...
    void Lure()
    {
        //Start of lure mechanic - Jak
        if (GetComponent<Animator>() != null)
        {
            Debug.Log("Lure Called.");
            //Lure Enemies to us
            //#OPTIMISE //Refactor this so it finds tags first instead of all colliders
            Collider[] civillians = Physics.OverlapSphere(transform.position, lureRange);

            //Sample this object position before sending it to the ai
            NavMeshHit navHit;
            NavMesh.SamplePosition(this.gameObject.transform.position, out navHit, lureRange, -1);

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

            lureUsed = true;
        }
    }

    void Repel()
    {
        //Repel - Jak
        if (GetComponent<Animator>() != null)
        {
            Debug.Log("Repel Called.");
            //Play scare animation
            //We assume that we are correctly hidden in an ITEM, so they MUST have a ItemController script attached
            GetComponent<ItemController>().scare = true;

            //Get all colliders
            //#OPTIMISE
            Collider[] civillians = Physics.OverlapSphere(transform.position, lureRange); //Refactor this so it finds tags first instead of all colliders

            foreach (Collider civ in civillians)
            {
                if (civ.tag == "Civillian")
                {
                    //I change the target gameobject in the civillians, so we can easily access the ItemScaryRating
                    //The target is also used in CIV_Retreat to know which item to run away from
                    civ.GetComponent<CivillianController>().target = gameObject;
                    civ.GetComponent<CivillianController>().TRIGGERED_repel = true;
                }
            }
            lureUsed = false;
        }
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
        }
        
        //sneakTest.transform.position = player.transform.position;
        sneakTest.GetComponent<playerController>().enabled = true;

        sneakTest.GetComponent<playerPossession>().PossessedItem = null; //Reset this object now that we are nolonger possessing something
        sneakTest.GetComponent<playerPossession>().enabled = true;

        //Destroy the old possession componenet on the last possessed object
        Destroy(player.GetComponent<playerPossession>());

        //sneakTest.GetComponent<CapsuleCollider>().enabled = true;
        sneakTest.GetComponent<CharacterController>().enabled = true;

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
        //Debug.DrawRay(adjustedPlayerPosition, player.transform.forward * allowablePosessionRange, Color.yellow ,3f);

        if (Physics.Raycast(testRay, out hit, allowablePosessionRange))
        {
            if(hit.transform.tag == "Item")
            {
                target = hit.collider.gameObject;
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
        RaycastCheckItem();

        if (targetSet == true && disolveScript.transferring == false)
        {
            disolveScript.transferring = true;
            disolveScript.target = target;
            disolveScript.startDissolve = true;  //Start the particle transition
            
            //Stop movement on sid while transitioning
            gameObject.GetComponent<playerController>().speed = 0;
            gameObject.GetComponent<CharacterController>().enabled = false;

            Camera.main.gameObject.GetComponent<CamLock>().enabled = false;
            //Camera.main.GetComponent<SmoothFollowWithCameraBumper>().updatePosition = false;

            //Wait until the animation is finished
            while (disolveScript.transferred != true) //This will change to true in script_WillDisolve once the animation is done
            {
                yield return null; // wait until next frame
            }
            
            //Call possess code once possesion anim is complete
            PossessItem();            

            //Reset variables for next animation
            targetSet = false;
            disolveScript.target = null;
            disolveScript.transferred = false;
            disolveScript.transferring = false;

            Camera.main.gameObject.GetComponent<CamLock>().enabled = true; //This will also Refind the new player which is now the item
            //Camera.main.GetComponent<SmoothFollowWithCameraBumper>().updatePosition = true;

            gameObject.GetComponent<playerController>().speed = oldSpeed;

            
        }
    }

    static bool lastClicked = false; //needs to be static as this code is called from multiple objects 

    bool leftTriggerClicked()
    {
        bool tryingToClick = Input.GetAxis("LeftTrigger") > .25f;//code to determine if trigger has been pressed as it is an axis it has no keydown event

        //handle selecting the trigger
        if (!lastClicked && tryingToClick)
        {
            lastClicked = tryingToClick;
            //Debug.Log("ClickingLeft");
            return true;
        }
        else
        {
            // handle when deselecting the trigger
            if (lastClicked && !tryingToClick)
            {
                lastClicked = tryingToClick;
                //Debug.Log("ResettingLeft");
            }
            return false;
        }
    }

    static bool lastClickedRight = false;//needs to be static as this code is called from multiple objects

    bool RightTriggerClicked()
    {
        bool tryingToClick = Input.GetAxis("RightTrigger") > .25f;//code to determine if trigger has been pressed as it is an axis it has no keydown event

        //handle selecting the trigger
        if (!lastClickedRight && tryingToClick)
        {
            lastClickedRight = tryingToClick;
            // Debug.Log("ClickingRight");
            return true;
        }
        else
        {
            // handle when deselecting the trigger
            if (lastClickedRight && !tryingToClick)
            {
                lastClickedRight = tryingToClick;
                // Debug.Log("ResettingRight");
            }
            return false;
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
