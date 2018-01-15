using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class script_civilianIconState : MonoBehaviour {

    public GameObject icon;
    public Sprite alerted;
    public Sprite retreat;
    public Sprite scared;

    public enum gameState
    {
        normal,
        alerted,
        retreat,
        scared
    }

    public  gameState myState = gameState.normal;

    private Animator iconAc;
    private Image iconImage;

    private gameState oldState = gameState.normal;


    // Use this for initialization
    void Start () {
        iconAc = icon.GetComponent<Animator>();
        iconImage = icon.GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
        if (oldState != myState)
            {
            if (oldState != gameState.normal) { iconAc.SetTrigger("stopAnim"); }
            if (myState != gameState.normal) {

                iconAc.SetTrigger("playAnim"); }
            switch (myState)
            {
                case gameState.alerted:
                    iconImage.sprite = alerted;
                    break;
                case gameState.retreat:
                    iconImage.sprite = retreat;
                    break;
                case gameState.scared:
                    iconImage.sprite = scared;
                    break;
            }

                    oldState = myState;
            }




    }
}
