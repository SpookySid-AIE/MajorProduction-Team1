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

    public enum Orientation { Least_Scary, Medium_Scary, Most_Scary }
    public enum Size { Miniature, Small, Medium, Large }
    public int timesThrownBeforeDestroyed = 1;
    private int timesThrown = 0;
    public Orientation ItemScaryRating;
    public Size itemSize;
    public bool hasBeenThrown;
    private bool hasBeenDamaged = false;


    [HideInInspector]
    public float timer = 0;
    [HideInInspector]
    public int baseScariness;
    [HideInInspector]
    public int ectoCost;

    //Animation boolean - turns off/on the animation
    [HideInInspector]public bool scare = false;
    public bool scaryObject = false; //Temporary until i can figure a way to make all scary objects with animations generic
    private Animator anim;    

    private void Awake()
    {
        gameObject.AddComponent<NavMeshObstacle>();
        gameObject.GetComponent<NavMeshObstacle>().carving = true;
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
            baseScariness = 2; //Edit this to edit how scared citizens get when struck with said item
            ectoCost = 2; //Edit this to edit the overall cost of ectoplasm for possesing an item that's the least expensive
        }
        else if (ItemScaryRating == Orientation.Medium_Scary)
        {
            baseScariness = 5;//Edit this to edit how scared citizens get when struck with said item
            ectoCost = 5; //Edit this to edit the overall cost of ectoplasm for possesing an item that's the medium expensive
        }
        else if (ItemScaryRating == Orientation.Most_Scary)
        {
            baseScariness = 10;//Edit this to edit how scared citizens get when struck with said item
            ectoCost = 10; //Edit this to edit the overall cost of ectoplasm for possesing an item that's the most expensive
        }
        else Debug.LogError("ItemScaryRating Error! No values selected for: " + gameObject.name);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (timesThrown == timesThrownBeforeDestroyed)
        {
            if (hasBeenDamaged)
            {             
                GameObject crashClone = Instantiate(GameObject.Find("PrefabController").GetComponent<PrefabController>().explosionEffect, gameObject.transform.position, gameObject.transform.rotation);
                gameObject.GetComponentInChildren<Renderer>().enabled = false;
                gameObject.tag = "Untagged";
                DestroyObject(gameObject, 2.0f);
                DestroyObject(crashClone, 2.0f);
                timesThrown = 0;

            }
        }
    }

    void Update()
    {

        if (transform.position.y < -0.5)
            transform.SetPositionAndRotation(new Vector3(transform.position.x, 0.5f, transform.position.z), transform.rotation);

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

        if (hasBeenThrown)
        {
            if (hasBeenDamaged == false)
            {
                timesThrown++;
                hasBeenDamaged = true;               
            }

            timer += Time.deltaTime;
            if (timer >= 3)
            {
                timer = 0;
                hasBeenThrown = false;
                hasBeenDamaged = false;
            }
            
        
        }
    } //End update

    private void OnTriggerEnter(Collider other)
    {
        if(tag == "Player" && other.tag == "Civillian" && other.GetComponent<CivillianController>().currentState == State.State_Alert) //Only run this code once the player is hiding inside the item, and a civillian is alerted
        {
            other.GetComponent<CivillianController>().navAgent.isStopped = true;

            //Play thinking? animation here

            other.GetComponent<Animator>().enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (tag == "Player" && other.tag == "Civillian" && other.GetComponent<CivillianController>().currentState == State.State_Alert) //Only run this code once the player is hiding inside the item, and a civillian is alerted
        {
            other.GetComponent<CivillianController>().navAgent.isStopped = true;

            //Play thinking? animation here

            other.GetComponent<Animator>().enabled = false;
        }
    }

}
