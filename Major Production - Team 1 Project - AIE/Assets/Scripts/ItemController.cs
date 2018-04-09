////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <29/11/17>                               
// Brief: <Generic Item Controller to control range of audio and others>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class ItemController : MonoBehaviour
{

    public enum Orientation { Least_Scary, Medium_Scary, Most_Scary } //A created variable used to determine the scariness of an item and therefore, values received for scaring with it.
    public enum Size { Miniature, Small, Medium, Large, Massive } //A created variable used to determine the size of an object, and therefore the camera distance.
    public int timesThrownBeforeDestroyed = 1; //The number of times an item can be thrown before it's destroyed. Default is once, can be edited without repercussion.
    [HideInInspector]public int timesThrown = 0; //A counter that tracks the number of times and item has been thrown. 
    public Orientation ItemScaryRating; // Creating a variable for scary ratings, using the first line of code above this.
    public Size itemSize; //Same as "ItemScaryRating" except for item size.
    public bool hasBeenThrown; //A boolean to check whether an item has been thrown or not.
    private bool hasBeenDamaged = false; //A boolean to check whether an item has been damaged recently. Prevents an item breaking instantly.
    private bool itemDestroying = false;
    private bool noCollideSet;


    [HideInInspector]
    public float timer = 0; //A timer used for calculating when an item can be classified as not thrown again.
    [HideInInspector]
    public float baseScariness; //The base scariness of an item.
    [HideInInspector]
    public float ectoCost; //The cost in ectoplasm for throwing a given item.

    //Animation boolean - turns off/on the animation
    private bool scare = false;

    //Sets scare to true/false
    public void SetAnimScare(bool b)
    {
        scare = b;
    }

    //Set via inspector - Tells us that this object will be used to scare civs
    [SerializeField]private bool scaryObject = false; //Temporary until i can figure a way to make all scary objects with animations generic

    public bool isScaryObject()
    {
        return scaryObject;
    }


    private Animator anim;    

    private void Awake()
    {
        gameObject.AddComponent<NavMeshObstacle>();
        gameObject.GetComponent<NavMeshObstacle>().carving = true;

        if (gameObject.GetComponent<BoxCollider>())
        {
            gameObject.GetComponent<NavMeshObstacle>().size = gameObject.GetComponent<BoxCollider>().size;
        }

        //gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    void Start()
    {
#if UNITY_EDITOR
        {          
            //GameObject canvasObject = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/ItemCanvas.prefab", typeof(GameObject)) as GameObject;//Load the POI prefab to instantiate later
            //GameObject realCanvas = Instantiate(canvasObject);
            //realCanvas.name = "Canvas " + this.name;
            //realCanvas.transform.SetParent(this.transform, false);

            //txtValue = transform.GetChild(0).GetChild(0).GetComponent<Text>(); //This will only work if the child is in the first slot of each child object

            //if (showValue == true)
            //    txtValue.text = itemValue.ToString();
            //else
            //    txtValue.text = "";
        }
#endif
        //Check for an animator controller - error checking
        anim = GetComponent<Animator>();

        if (ItemScaryRating == Orientation.Least_Scary)
        {
            baseScariness = Camera.main.GetComponent<valueController>().leastScaryNPCResponse; //Edit this to edit how scared citizens get when struck with said item
            ectoCost = Camera.main.GetComponent<valueController>().leastScaryUsedCost; //Edit this to edit the overall cost of ectoplasm for possesing an item that's the least expensive
        }
        else if (ItemScaryRating == Orientation.Medium_Scary)
        {
            baseScariness = Camera.main.GetComponent<valueController>().mediumScaryNPCResponse; ;//Edit this to edit how scared citizens get when struck with said item
            ectoCost = Camera.main.GetComponent<valueController>().mediumScaryUsedCost; //Edit this to edit the overall cost of ectoplasm for possesing an item that's the medium expensive
        }
        else if (ItemScaryRating == Orientation.Most_Scary)
        {
            baseScariness = Camera.main.GetComponent<valueController>().mostScaryNPCResponse; ;//Edit this to edit how scared citizens get when struck with said item
            ectoCost = Camera.main.GetComponent<valueController>().mostScaryUsedCost; ; //Edit this to edit the overall cost of ectoplasm for possesing an item that's the most expensive
        }
        else Debug.LogError("ItemScaryRating Error! No values selected for: " + gameObject.name);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (timesThrown == timesThrownBeforeDestroyed) //If an item reaches the "TimesThrownBeforeDestroyed" Threshold.
        {
            if (hasBeenDamaged && !itemDestroying) //If it collides with something one last time
            {
                itemDestroying = true;
                hasBeenDamaged = false;
                GameObject crashClone = Instantiate(GameObject.Find("PrefabController").GetComponent<PrefabController>().explosionEffect, gameObject.transform.position, gameObject.transform.rotation); //Creates the explosion effect, from the prefab controller.
                gameObject.GetComponentInChildren<Renderer>().enabled = false; //Derenders the thrown item.
                if (gameObject.GetComponentInChildren<SkinnedMeshRenderer>())
                gameObject.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                //gameObject.tag = "Untagged"; //Removes the items tag.
                DestroyObject(gameObject, 2.0f); //Destroys the item after two seconds, to prevent it disappearing before sid is reactivated.
                timesThrown = 0; //Resets the times thrown value.
            }
        }

        //Play sound clip
        FMODUnity.RuntimeManager.PlayOneShot(GameManager.Instance.audioItemImpact, transform.position);
    }

    void Update()
    {

        if (transform.position.y < -0.5)
            transform.SetPositionAndRotation(new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation); //If an item glitches below -0.5 on height, then move it to 0.5 (If something goes underground, put it above ground)

        //Animation activation for repel
        if (anim != null) //Error checking incase an Item doesnt have a controller
        {
            if (scaryObject)
            {
                if (scare == true)                
                    anim.SetBool("possessed", scare); //This also means every Item that has a scared animation need this boolean in the controller
                else                
                    anim.SetBool("possessed", scare);                
            }
   
        }

        if (hasBeenThrown) //If something has been thrown.
        {
            if (!noCollideSet)
            {
                gameObject.layer = 8;
                noCollideSet = true;
            }

            if (hasBeenDamaged == false) //If the hasBeenDamaged hasn't been set yet.
            {
                timesThrown++; //Add one to times thrown
                hasBeenDamaged = true; //Set item has been thrown to true
            }

            timer += Time.deltaTime; //Add real time to the timer

            if (timer >= 0.25 && gameObject.layer != 1)
                gameObject.layer = 1;

            if (timer >= 3) //If it hasn't been three seconds yet.
            {               
                timer = 0; //Reset the timer
                hasBeenThrown = false; //Reset the has been thrown check
                hasBeenDamaged = false; //Reset the has been damaged check
                gameObject.layer = 1;
                noCollideSet = false;
            }
            
        
        }
    } //End update
}
