using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class playerPossession : MonoBehaviour
{
    private GameObject player; //could be the real player or a possessed item
    private GameObject sneakTest;

    //Stores a reference to the current item we are possesing, used in CIV_Retreat
    public GameObject PossessedItem;

    public Color possessionColour = Color.cyan;// Added by Mark - Added possession color for outline
    public float HeightAdjustment = .4f; //where to start the ray - need to align the spotlight to this position

    public float allowablePosessionRange = 10;

    private static bool isPossesed = false;

    public bool CheckIsPossesed()
    {
        return isPossesed;
    }

    private float throwVelocity = 30;

    //Determines when we can use the "Lure/Repel" ability
    private static bool hidden = false;

    //Player's position is stored here when the use the hide mechanic, so when we unhide they resume from the old position
    public static Vector3 oldPlayerPos;
    public static Quaternion oldPlayerRot;

    public int lureRange = 10; //Range at which the lure ability will attract the ai

    // Use this for initialization - note that the player could be real or could be an item
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sneakTest = GameObject.FindGameObjectWithTag("Sneak");
    }

    //as the code enables and disables the player, this is required to initialise the code
    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sneakTest = GameObject.FindGameObjectWithTag("Sneak");
    }

    private void Update()
    {
        if (sneakTest)
        {
            sneakTest.transform.position = player.transform.position;
            sneakTest.transform.rotation = player.transform.rotation;
        }

        //handle Throwing a Posessed Item - this needs to be before the handle posession item possession if statement below as it is possible to trigger both throw and trigger at the same time 
        //doing so causes the player to try and throw itself - which does not work very well :)
        if (Input.GetMouseButtonDown(0))
        {
            //Moved possessItem() around to left click - Jak
            if (hidden == false && !isPossesed) //The hidden flag only detects when a player is hiding in an item, not POSSESSED - Jak
            {
                PossessItem();
            }
            else if(isPossesed)
            {
                ThrowPossessedItemAway();
                UnpossessItem();
            }

            //Start of lure mechanic - Jak
            if(hidden && GetComponent<Animator>() != null)
            {
                //Lure Enemies to us
                //#OPTIMISE
                Collider[] civillians = Physics.OverlapSphere(transform.position, lureRange); //Refactor this so it finds tags first instead of all colliders

                //Sample this object position before sending it to the ai
                NavMeshHit navHit;
                NavMesh.SamplePosition(this.gameObject.transform.position, out navHit, lureRange, -1);

                foreach (Collider civ in civillians)
                {
                    if (civ.tag == "Civillian")
                    {
                        CivillianController civillian = civ.GetComponent<CivillianController>();

                        if(civillian.currentState != State.State_Retreat) //Only LURE the CIVS if they arent already in a retreat state / Prevents spam
                        {
                            civillian.itemPosition = navHit.position;
                            civillian.alertedByItem = true;
                        }                        
                    }
                }                
            }
            //End Jak's
        }
        else
        {
            //Hide/Unhide mechanic written by Jak
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (hidden == false)
                {
                    Hide();
                }
                else
                {
                    Unhide();
                }
            }
        }

        //Repel - Jak - need a fix for this so it isnt triggered when we are SID
        if (Input.GetMouseButtonDown(1) && hidden && GetComponent<Animator>() != null)
        {
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
        }
        //End Repel - Jak

    }

    // Update is called once per frame
    void PossessItem()
    {
        //try to posess an item
        RaycastHit hit;
        Vector3 adjustedPlayerPosition = player.transform.position + (player.transform.up * HeightAdjustment); //adjust beacuse the players pivot point is at its base

        Ray testRay = new Ray(adjustedPlayerPosition, player.transform.forward);
        //Debug.DrawRay(adjustedPlayerPosition, player.transform.forward * allowablePosessionRange, Color.yellow ,3f);

        if (Physics.Raycast(testRay, out hit, allowablePosessionRange))
        {
            if (hit.transform.tag == "Item")
            {
                if (player.GetComponent<playerController>().Ectoplasm > 0.0f)
                {
                    Camera.main.GetComponent<CamLock>().floatSpeedOfSid = player.GetComponent<playerController>().floatSpeed;
                    //Super quick fix to give inf ecto in other test scenes
                    //if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level_1"))
                    GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>().Ectoplasm -= hit.transform.GetComponent<ItemController>().ectoCost; //Deducts the amount of ectoplasm based on item thrown - Ben

                    //disable camera whilst we change the cameras target to the newly possesed item
                    Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

                    //rename the player tag so they dont participate in any collisions
                    player.tag = "Sneak";
                    player.GetComponent<CapsuleCollider>().enabled = false;

                    //set the taget to what was hit in the raycast
                    GameObject target = hit.collider.gameObject;

                    //name it player so that it behaves like one in collisions
                    target.tag = "Player";

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

                    if (player.GetComponent<CapsuleCollider>())
                        player.GetComponent<CapsuleCollider>().enabled = false;

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
                        target.GetComponent<CharacterController>().radius = 0;
                        target.GetComponent<CharacterController>().height = 0;
                    }
                    
                        //target.GetComponent<CharacterController>().enabled = true;

                    //switch off gravity for the target
                    target.GetComponent<Rigidbody>().useGravity = false;
                    target.GetComponent<Rigidbody>().freezeRotation = true; //Freeze item rotation while possesed, caused the camera to glitch - Jak - 13/11/17

                    //turn off the item controller script
                    target.GetComponent<ItemController>().enabled = true; //Added by Jak - 4/12/17

                    //switch the camera back on to follow the player
                    Camera.main.gameObject.GetComponent<CamLock>().enabled = true;
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
                    isPossesed = true;
                    Debug.Log(Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance);
                    //Debug.Log(isPossesed);
                }
            }
        }
    }

    void ThrowPossessedItemAway()
    {
        //throw the object;
        //may need to identify that the object was "hitby" will so that it will register a point of interest when it colides with something.
        player.GetComponent<Rigidbody>().velocity = player.GetComponent<Rigidbody>().transform.forward * throwVelocity;
    }

    public void UnpossessItem()
    {
        //turn this item back into a regular item
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

        Invoke("EmitNothing", 2f);//this reset the emission to off
        isPossesed = false;
        PossessedItem = null;

        //switch off the camera tracking whilst we reset the player back to what it is supposed to be
        Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

        sneakTest.tag = "Player";//need to do this here as the agent code needs a player at all times
        sneakTest.GetComponent<CapsuleCollider>().enabled = true;
        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().distance = 2.0f;
        Camera.main.GetComponent<SmoothFollowWithCameraBumper>().targetLookAtOffset = new Vector3(0, 1, 1);
        //Debug.Log(Camera.main.gameObject.GetComponent<SmoothFollowWithCameraBumper>().distance);
        Invoke("EnablePlayer", .25f);//re-enable Player after a short time at this position  needed so that Player does not colide with the object he is unposessing
    }

    //Written by Jak
    void Unhide()
    {
        //disable camera while we switch back to the real player
        Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

        //rename the player
        sneakTest.tag = "Player";

        //set the taget to what was hit in the raycast
        GameObject target = this.gameObject; //Target equals the current object that this script is on(aka ITEM)

        //Rename the item
        target.tag = "Item";

        //Set the current position of the player back to the oldPosition from when they hid
        sneakTest.transform.position = oldPlayerPos;
        sneakTest.transform.rotation = oldPlayerRot;        

        //turn onn all player scripts
        foreach (Behaviour childCompnent in sneakTest.GetComponentsInChildren<Behaviour>())
        {
            if (childCompnent.tag != "MainCamera")
                childCompnent.enabled = true;
        }

        //turn on all player renderers
        SkinnedMeshRenderer[] meshRenderer = sneakTest.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer smr in meshRenderer)
        {
            smr.enabled = true;
        }

        //Turn on colliders
        sneakTest.GetComponent<CapsuleCollider>().enabled = true;
        sneakTest.GetComponent<CharacterController>().enabled = true;

        //switch on gravity for the target
        target.GetComponent<Rigidbody>().useGravity = true;
        target.GetComponent<Rigidbody>().freezeRotation = false; //Freeze item rotation while possesed, caused the camera to glitch - Jak - 13/11/17

        //switch the camera back on to follow the player
        Camera.main.gameObject.GetComponent<CamLock>().enabled = true;

        //Stop the scare animation if it is still playing when we eject
        GetComponent<ItemController>().scare = false;

        //Tell everyone that SID is no longer hidden
        hidden = false;

        //destroy this script instance from the ITEM
        Destroy(target.GetComponent<playerPossession>());
    }

    //written by Jak - copypasted some stuff from "PossessItem()"
    void Hide()
    {
        //try to posess an item
        RaycastHit hit;
        Vector3 adjustedPlayerPosition = player.transform.position + (player.transform.up * HeightAdjustment); //adjust beacuse the players pivot point is at its base

        Ray testRay = new Ray(adjustedPlayerPosition, player.transform.forward);
        //Debug.DrawRay(adjustedPlayerPosition, player.transform.forward * allowablePosessionRange, Color.yellow ,3f);

        if (Physics.Raycast(testRay, out hit, allowablePosessionRange))
        {
            if (hit.transform.tag == "Item")
            {
                //disable camera whilst we change the cameras target to the newly possesed item
                Camera.main.gameObject.GetComponent<CamLock>().enabled = false;

                //rename the player tag so they dont participate in any collisions
                player.tag = "Sneak";

                //set the taget to what was hit in the raycast
                GameObject target = hit.collider.gameObject;

                //name it player so that it behaves like one in collisions
                target.tag = "Player";

                //At this point - playerPossession.cs is still active on Player
                oldPlayerPos = gameObject.transform.position;
                oldPlayerRot = gameObject.transform.rotation;

                //turn off all player scripts
                foreach (Behaviour childCompnent in player.GetComponentsInChildren<Behaviour>())
                {
                    if (childCompnent.tag != "MainCamera")
                        childCompnent.enabled = false;
                }

                //turn off all player renderers
                SkinnedMeshRenderer[] meshRenderer = player.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer smr in meshRenderer)
                {
                    smr.enabled = false;
                }

                //Turn off colliders
                player.GetComponent<CapsuleCollider>().enabled = false;
                player.GetComponent<CharacterController>().enabled = false;

                if (target.GetComponent<playerPossession>() == null)
                    target.AddComponent<playerPossession>();

                //target.GetComponent<playerPossession>().PossessedItem = target;


                //switch off gravity for the target
                target.GetComponent<Rigidbody>().useGravity = false;
                target.GetComponent<Rigidbody>().freezeRotation = true; //Freeze item rotation while possesed, caused the camera to glitch - Jak - 13/11/17

                //switch the camera back on to follow the player
                Camera.main.gameObject.GetComponent<CamLock>().enabled = true;
                hidden = true;
            }
        }
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


        sneakTest.GetComponent<CharacterController>().enabled = true;
        sneakTest.GetComponent<script_ToonShaderFocusOutline>().enabled = true;// Added by Mark - Reenable outline focus script on sid

        Camera.main.gameObject.GetComponent<CamLock>().enabled = true;
        sneakTest.GetComponent<CapsuleCollider>().enabled = true;
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
