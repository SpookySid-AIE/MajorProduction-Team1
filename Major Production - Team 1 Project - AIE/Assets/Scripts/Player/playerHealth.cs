using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerHealth : MonoBehaviour {
    private GameObject player;
    private GameObject sneakTest;
    private Rigidbody playerrb;
    public float currentPlayerHealth;
    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); //Gets All necessary values from tags, and sets players current health.
        playerrb = player.GetComponent<Rigidbody>();
        sneakTest = GameObject.FindGameObjectWithTag("Sneak");        
        currentPlayerHealth = 100.0f;
    }
	
	// Update is called once per frame
	void Update () {
	}
}
