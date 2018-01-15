using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script_ToonShaderFocusOutline : MonoBehaviour
{
    private GameObject player; //could be the real player or a possessed item
    public float HeightAdjustment = .4f; //where to start the ray - need to align the spotlight to this position

    public float allowablePosessionRange = 10;
    public GameObject passInObject;

    public Renderer oldToonOutline;
    public Renderer toonOutline;

    public Color focusColor = Color.yellow;
    
    public Color defaultColor = Color.black;

    private void Start()
    {
        player = this.gameObject; //The player variable is now set to whatever object this script is attached too.
    }

    private void FixedUpdate()
    {
        PassInToonOutline();
    }


    void AssignOldTooneOutline()
    {
        if (oldToonOutline != toonOutline)
        {
            oldToonOutline = passInObject.GetComponentInChildren<Renderer>();

            if (oldToonOutline == null)
                Debug.Log("CANT FIND RENDERER");

            toonOutline.material.SetColor("_OutlineColor", defaultColor);
            
        }
    }

    void PassInToonOutline()
    {
        RaycastHit hit;
        Vector3 adjustedPlayerPosition = player.transform.position + (player.transform.up * HeightAdjustment); //adjust beacuse the players pivot point is at its base

        Ray testRay = new Ray(adjustedPlayerPosition, player.transform.forward);
        //Debug.DrawRay(adjustedPlayerPosition, player.transform.forward * allowablePosessionRange, Color.yellow, .1f);

        if (Physics.Raycast(testRay, out hit, allowablePosessionRange))
        {
            if (hit.transform.tag == "Item")
            {
                passInObject = hit.collider.gameObject;
                toonOutline = passInObject.GetComponentInChildren<Renderer>();
                toonOutline.material.SetColor("_OutlineColor", focusColor);
            }
            else
            {
                passInObject = null;
                toonOutline = null;
            }
        }else
        {
            passInObject = null;
            toonOutline = null;
        }

        if (oldToonOutline != toonOutline) 
        {
            if (oldToonOutline != null)
            {
                oldToonOutline.material.SetColor("_OutlineColor", defaultColor);
                //Debug.Log("No way");
            }
            oldToonOutline = toonOutline;

        }


    }
}