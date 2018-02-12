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
using System;

public class ItemController : MonoBehaviour
{

    public enum Orientation { Least_Scary, Medium_Scary, Most_Scary }
    public enum Size { Miniature, Small, Medium, Large }
    public Orientation ItemScaryRating;
    public Size itemSize;
    public bool hasBeenThrown;

    [HideInInspector]
    public float timer = 0;
    [HideInInspector]
    public int baseScariness;
    [HideInInspector]
    public int ectoCost;
    //[HideInInspector]
    public Vector3 targetOffset;
    //[HideInInspector]
    public float distance;

    //Animation boolean - turns off/on the animation
    [HideInInspector]public bool scare = false;
    public bool scaryObject = false; //Temporary until i can figure a way to make all scary objects with animations generic
    private Animator anim;    

    private void Awake()
    {
        gameObject.AddComponent<NavMeshObstacle>();
        gameObject.GetComponent<NavMeshObstacle>().carving = true;
    }

    void OnValidate()
    {
        switch (itemSize) // == ItemController.Size.Miniature)
        {
            case Size.Miniature:
                distance = 1.95f;
                targetOffset = new Vector3(0, 1.25f, 1.25f);
                break;

            case Size.Small:
                distance = 2.25f;
                targetOffset = new Vector3(0, 1, 1);
                break;

            case Size.Large:
               distance = 7.0f;
                targetOffset = new Vector3(0, 0, 0);
                break;
        }
    }

    private void Reset()
    {
        if (CameraTransform() == null)
        {
            UnityEditor.EditorUtility.DisplayDialog("Error","ItemController requires a camera pin object as a child","Cancel");
        }
    }

    Transform CameraTransform()
    {
        int count = 0;
        Transform pinned = null;
        foreach( Transform child in transform)
        {
            if (child.tag == "CameraPin")
            {
                count++;
                pinned = child.transform;
            }
        }
        if (count == 1)
            return pinned;
        else
        {
            return null;
        }
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

    void Update()
    {

        if (transform.position.y < -0.5)
        {
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
            //transform.rotation);
        }
        //Animation activation for repel
        if (anim != null) //Error checking incase an Item doesnt have a controller
        {
            if (scaryObject)
            {
                if (scare == true)                
                    anim.SetBool("scare", scare); //This also means every Item that has a scared animation need this boolean in the controller
                else                
                    anim.SetBool("scare", scare);                
            }
   
        }

        if (hasBeenThrown)
        {
            timer += Time.deltaTime;
            if (timer >= 3)
            {
                timer = 0;
                hasBeenThrown = false;
            }
            
        }
    } //End update

    private void OnTriggerEnter(Collider other)
    {
        if(tag == "Player" && other.tag == "Civillian" && other.GetComponent<CivillianController>().currentState == State.State_Alert) //Only run this code once the player is hiding inside the item, and a civillian is alerted
        {
            other.GetComponent<CivillianController>().navAgent.Stop();//.isStopped = true;

            //Play thinking? animation here

            other.GetComponent<Animator>().enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (tag == "Player" && other.tag == "Civillian" && other.GetComponent<CivillianController>().currentState == State.State_Alert) //Only run this code once the player is hiding inside the item, and a civillian is alerted
        {
            other.GetComponent<CivillianController>().navAgent.Stop();// = true;

            //Play thinking? animation here

            other.GetComponent<Animator>().enabled = false;
        }
    }

}
