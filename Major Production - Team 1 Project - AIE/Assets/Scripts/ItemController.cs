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
    public Orientation ItemScaryRating;
    public bool hasBeenThrown;

    [HideInInspector]
    public float timer = 0;
    [HideInInspector]
    public int baseScariness;
    [HideInInspector]
    public int ectoCost; 

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

}
