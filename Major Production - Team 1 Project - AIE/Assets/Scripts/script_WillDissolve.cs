using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script_WillDissolve : MonoBehaviour {
    public bool startDissolve; // Disolve trigger
    public GameObject target; // Target reference
    public GameObject[] characterObjects; // reference to objects to disolve
    public GameObject particlePrefab;// Particle to system to be thrown at target
    public float particleVerticleOffset = -1.86f; // Vertically offset the instanced particle system
    public float dissovleSpeed = 0.1f; // Speed for the disolve
    public float currentDissolve = 3.0f; // Dissolve goes from 3 to -2   'Visible to Invisible'
    public float particleDelay = 0.27f; // Delay Particle instance creation
    public float particleTimer; // Timer for Particle Instance Creation
    public float currentTime; // Current Time
    public float particleLaunchDelay = 0.2f; // Delay for Particle to start moving to target
    public bool transferred = false; // Used while transfering into an object - when to trigger final transition into hide() method inside playerPossesion

    private bool dissolve; // Used during disolve
    private Renderer[] rend; // Reference for renderers in each characterObject
    private Material mat;// Mat to be used for each characterObject

    void Start()
    {
        rend = new Renderer[characterObjects.Length];
        mat = characterObjects[0].GetComponent<Renderer>().material; // Get reference for the material from the first characterObject so it can be used on the others

        for (int i = 0; i < characterObjects.Length ; i++)
        {
            rend[i] = characterObjects[i].GetComponent<Renderer>();
            rend[i].material = mat; // Set all characterObjects with the same Material reference
        }
    }


    void Update() {

        PlayerDissolve();

    }

    void PlayerDissolve() { 

		if (startDissolve) // Start the disolve process
        {
            if (!dissolve) { particleTimer = Time.time + particleDelay; }
            dissolve = true;
            currentDissolve = 3.0f;
        }


        if (dissolve)
        {
            if (currentDissolve > -2)
            {
                currentDissolve -= dissovleSpeed;
                mat.SetFloat("_ParticleMaskingPosition", currentDissolve);
            }
        }

        currentTime = Time.time;
        if (Time.time > particleTimer && startDissolve)
        {
            GameObject particle = Instantiate(particlePrefab, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + particleVerticleOffset, gameObject.transform.position.z), Quaternion.identity);
            CurveToTarget curveToTarget = particle.GetComponent<CurveToTarget>();
            curveToTarget.target = target;
            curveToTarget.particleLaunchDelay = particleLaunchDelay;
            //Debug.Log("Yo");
            startDissolve = false;
            transferred = true;
        }
        if (currentDissolve < -2 && dissolve)
        {
            currentDissolve = -2.0f;
            dissolve = false;
        }

	}
}
