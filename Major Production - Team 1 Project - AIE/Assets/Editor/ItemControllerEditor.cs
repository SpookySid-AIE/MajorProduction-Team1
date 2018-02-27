//////////////////////////////////////////////////////////////
//// Author: <Jak Revai>                                     
//// Date Created: <27/02/18>                               
//// Brief: <Custom Editor for the ItemController>  
//////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemController))]
public class ItemControllerEditor : Editor
{
    string[] colliderList = new string[] { "BoxCollider", "SphereCollider", "CapsuleCollider", "MeshCollider" };
    int colliderIndex = 0;
    ItemController item;

    public override void OnInspectorGUI()
    {
        //Draw the default inspector
        item = (ItemController)target; //The current object being inspected

        if (!isValid())
        {
            GUILayout.Label("--Choose Collider--");

            //If the choice is not in the array then the index will be -1 so set back to 0
            if (colliderIndex < 0)
                colliderIndex = 0;

            //Assign index based on selected item
            colliderIndex = EditorGUILayout.Popup(colliderIndex, colliderList);

            if (GUILayout.Button("Setup Item"))
            {
                item.tag = "Item";
                AddCollider(item); //Add collider
                item.gameObject.AddComponent<Rigidbody>();

                //Set Correct shader to use - Will only be added if we are using proper materials and not just the default one, so we assume when we setup the item its on a correct model
                if (item.GetComponentInChildren<Renderer>().sharedMaterial.name == "Default-Material")
                {
                    //Prevents material leakage
                    Debug.LogError("Default-Material detected on item. Shader could not be set. Please 'Reset' Item, and change material so shader can be set.");
                }
                else //Else a proper material is found, then we set item - "proper" as in hopefully not a in-built unity one
                {
                    //Setup the aura possesionn shader textures - values will still needed to be tweaked
                    item.GetComponentInChildren<Renderer>().sharedMaterial.shader = Shader.Find("Custom/Possession Aura2");
                    Texture t = AssetDatabase.LoadAssetAtPath("Assets/Textures/Skull-blur2.png", typeof(Texture)) as Texture;
                    item.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_AuraTexture", t);
                    item.GetComponentInChildren<Renderer>().sharedMaterial.SetTexture("_AuraTexture2", t);
                }
            }
        }
        else if (isValid())
        {
            DrawDefaultInspector(); //Once the valid components are one - draw the default script values

            GUIStyle style = new GUIStyle();
            style.richText = true;
            GUILayout.Label("<size=18><color=red><b>WARNING</b></color></size> - Removes components that make an <b>item</b>.", style);

            if(GUILayout.Button("Reset Item")) //Reset the item
            {
                DestroyImmediate(item.GetComponent<Collider>());
                DestroyImmediate(item.GetComponent<Rigidbody>());
                item.tag = "Untagged";
                EditorGUIUtility.ExitGUI(); //Cancel the next draw frame so we can delete the components without error
            }
        }
    }

    //If all checks pass then return true
    bool isValid()
    {
        if (item.tag != "Item")
            return false;

        if (item.GetComponent<Collider>() == null)
            return false;
        else
            return true;
    }

    //Method to add a collider based on index from drop down list
    void AddCollider(ItemController item)
    {
        switch (colliderIndex)
        {
            case 0:
                {
                    //if(item.gameObject.GetComponent<BoxCollider>() == null)
                        item.gameObject.AddComponent<BoxCollider>();
                    break;
                }
            case 1:
                {
                    item.gameObject.AddComponent<SphereCollider>();
                    break;
                }
            case 2:
                {
                    item.gameObject.AddComponent<CapsuleCollider>();
                    break;
                }
            case 3:
                {
                    item.gameObject.AddComponent<MeshCollider>();
                    break;
                }
            default:
                break;
        }
    }

}
