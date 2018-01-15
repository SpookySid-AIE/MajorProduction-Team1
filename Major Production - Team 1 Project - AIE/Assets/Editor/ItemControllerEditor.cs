//////////////////////////////////////////////////////////////
//// Author: <Jak Revai>                                     
//// Date Created: <29/07/17>                               
//// Brief: <Custom Editor for the ItemController>  
//////////////////////////////////////////////////////////////
//using System;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(ItemController))]
//public class ItemControllerEditor : Editor {

//    string materialList;
//    string[] materialTypes = new string[] { "Glass", "Wood", "Steel" };
//    int materialIndex = 0;

//    public override void OnInspectorGUI()
//    {
//        //Draw the default inspector
//        DrawDefaultInspector();
//        ItemController item = (ItemController)target; //The current object being inspected

//        //Set the index to the previously selected index in the inspector
//        materialIndex = Array.IndexOf(materialTypes, item.materialChosen);

//        //If the choice is not in the array then the index will be -1 so set back to 0
//        if (materialIndex < 0)
//            materialIndex = 0;

//        GUILayout.Label("\nChoose a Material Type from the drop down list.");
//        materialIndex = EditorGUILayout.Popup(materialIndex, materialTypes);

//        item.materialChosen = materialTypes[materialIndex]; //Set the material chosen in the current item to be the one selected in the dropdown

//        //Save the changes back to the object
//        EditorUtility.SetDirty(target);
//    }
    
//}
