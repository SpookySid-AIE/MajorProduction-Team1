////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                                                 
// Brief: <Simply trigger the changing of materials when a civ enters the collider>  
////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

public class TriggerHighlight : MonoBehaviour
{
    private Material highlightMaterial;

    private struct CivValues
    {
        public Material oldMat;
        public Renderer render; 
    }

    //Store all data for the material and renderer for EACH civ
    Dictionary<int, CivValues> civData = new Dictionary<int, CivValues>();     

    private void Start()
    {
        highlightMaterial = GetComponentInParent<playerPossession>().highlightMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Civillian" && !civData.ContainsKey(other.GetComponent<CivillianController>().GetID()))
        {
            //Build struct data
            CivValues v;
            v.render = other.GetComponentInChildren<Renderer>();
            v.oldMat = v.render.material;

            //Add the data into the dictionary so we can easily look up this civ and change its material in OnExit
            civData.Add(other.GetComponent<CivillianController>().GetID(), v);

            //Highlight the current civ
            v.render.material = highlightMaterial;

            //Debug.Log(v.oldMat);
            //Debug.Log("TriggerHighlight: " + other.GetComponent<CivillianController>().GetID());

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Civillian")
        {
            civData[other.GetComponent<CivillianController>().GetID()].render.material = civData[other.GetComponent<CivillianController>().GetID()].oldMat; //Revert highlight back to old mat
            civData.Remove(other.GetComponent<CivillianController>().GetID());
        }
    }

    private void OnDestroy()
    {
        foreach (var item in civData) //Final catch to change material back to old mat incase luresphere is deleted early and not every civ has the material reverted
        {
            item.Value.render.material = item.Value.oldMat;
        }

        civData.Clear(); //needed?? because script is being destroyed surely this will empty as well
    }
}
