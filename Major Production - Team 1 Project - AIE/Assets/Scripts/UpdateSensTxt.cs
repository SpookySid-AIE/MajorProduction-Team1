////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <20/08/17>                               
// Brief: <simple script to update the text in the controls menu - unsure if i could've done it through inspector>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSensTxt : MonoBehaviour {

    //These values will be passed into the main scene
    public static float mouseSensX = 1;
    public static float mouseSensY = 1;
    public static float controllerSensX = 1;
    public static float controllerSensY = 1;
    public static bool invertToSend = false;

    //slider to update the text object and pass values into the static variables, all set in the inspector using the Slider.OnChange event
    [Header("This only needs to be set on the menu. Leave it empty in Main.")]public Slider slider;

    public void UpdateText(Text text) //function to update the text objects on the ui
    {
        text.text = slider.value.ToString();

        //Forcing it to set the static floats - NAME dependant
        if(text.name == "Text_MouseXVal")
            mouseSensX = slider.value;
        if (text.name == "Text_MouseYVal")
            mouseSensY = slider.value;
        if (text.name == "Text_ControllerXVal")
            controllerSensX = slider.value;
        if (text.name == "Text_ControllerXVal")
            controllerSensY = slider.value;
    }

    public void SetInvert(Toggle invert)
    {
        invertToSend = invert.isOn;
    }
}
