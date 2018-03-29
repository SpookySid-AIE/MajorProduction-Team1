using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ectoplasmController : MonoBehaviour
{
    public float currentEctoplasmValue; //Set in editor, this is the amount of Ectoplasm you receive for picking up this item.
    public float modifier; //The modifier for the ectoplasm value. Base value is multiplied by this. - Ben
    public GameObject Ectoplasm; //Also set in editer, this determines the color of the Ectoplasm. - Ben
    [FMODUnity.EventRef]public string ectoPickup;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") //If item that's collided with is tagged as, "Player"
        {
            if ((other.gameObject.GetComponent<playerController>().Ectoplasm += (currentEctoplasmValue * modifier)) > 100.0f) 
                other.gameObject.GetComponent<playerController>().Ectoplasm = 100.0f; //If the number of ectoplasm being added exceeds 100, then just set it to 100.
            else other.gameObject.GetComponent<playerController>().Ectoplasm += (currentEctoplasmValue * modifier); //Otherwise, add the full amount.

            //Play sound clip
            FMODUnity.RuntimeManager.PlayOneShot(ectoPickup, transform.position);

            Destroy(gameObject);
        }
    }
}
