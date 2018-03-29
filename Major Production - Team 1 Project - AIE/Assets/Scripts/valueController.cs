using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class valueController : MonoBehaviour {

    [Header("Value of 'Least Scary' Ectoplasm when picked up.")]
    public float leastScaryEctoDropValue;
    [Header("Value of 'Medium Scary' Ectoplasm when picked up.")]
    public float mediumScaryEctoDropValue;
    [Header("Value of 'Most Scary' Ectoplasm when picked up.")]
    public float mostScaryEctoDropValue;

    [Header("The modifier on an item based on how scary the 'Least Scary' item is.")]
    public float leastScaryNPCResponse;
    [Header("The modifier on an item based on how scary the 'Medium Scary' item is.")]
    public float mediumScaryNPCResponse;
    [Header("The modifier on an item based on how scary the 'Most Scary' item is.")]
    public float mostScaryNPCResponse;

    [Header("How much the 'Least Scary' Items are to use.")]
    public float leastScaryUsedCost;
    [Header("How much the 'Medium Scary' Items are to use.")]
    public float mediumScaryUsedCost;
    [Header("How much the 'Most Scary' Items are to use.")]
    public float mostScaryUsedCost;

    [Header("How scared an NPC gets if they see Sid.")]public float EctoSidSeenScareValue;
    [Header("How scared an NPC gets if they are hit by a thrown item.")]public float EctoThrowScareValue;
    [Header("How scared an NPC gets if they are repelled using Sid's repel ability.")]public float EctoRepelledScareValue;

    [Header("How hard an item is thrown by Sid.")]public float thrownItemVelocity;

    [Header("Range of Sid's, 'Lure' ability.")]public float lureRange;
    [Header("Range of Sid's 'Possession ability.")]public float possessionRange;

    [Header("How far a civillian can spot Sid.")]public float civillianLineOfSight;

    [Header("How long it takes the camera to catch up with Sid's movements.")]public float cameraSmoothTime;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
