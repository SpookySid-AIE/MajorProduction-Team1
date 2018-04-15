using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////
// Author: <Mark Harrison>                                     
// Date Created: <07/08/17>                               
// Brief: <Creates a beam and fires it towards a target(Will)>  
////////////////////////////////////////////////////////////

public class script_ProtonBeam_v5 : MonoBehaviour {

    [Header("Currently set to Torch Range(Manually)")]public float beamLength = 15.0f;
    public GameObject bulletPrefab;
    public float minxBeamPartDistance = .03f;
    public float maxBeamPartDistance = .16f;
    public float minBeamOffset = 0.0f;
    public float maxBeamOffset = 1.53f;
    public float beamLife = .02f;

    //Used in Pursue State
    [HideInInspector]public Vector3 target;
    [Header("Transform from GeoGlock in the boneTree")]public Transform bulletspawn;
    [Header("Particle Beam in the boneTree")] public GameObject particleBeam;
    [HideInInspector]public bool fire;

    private float percentCounter;
    private Vector3 result;
    private int numberOfBeamInterations = 10; // Maybe shouldn't change this
    private Vector3[] beamInterationsWantedPos;
    private Vector3[] beamInterationsTarget;
    private GameObject bulletParent;

    private void Start()
    {
        beamInterationsWantedPos = new Vector3[numberOfBeamInterations];
        beamInterationsTarget = new Vector3[numberOfBeamInterations];
    }


    void Update()
    {
        
        if (fire && !Camera.main.GetComponent<GameManager>().isPaused)
        {
            //Disabled raycasting and trying an accuracy system to minus the health - Jak
            //RaycastHit hit;            
            //Vector3 forward;            
            //forward = transform.TransformDirection(Vector3.forward);

            //if (Physics.Raycast(bulletspawn.transform.position, bulletspawn.transform.forward, out hit, beamLength))
            //{
            //    target = hit.point;
            //}
            //else
            //{
            //    target = bulletspawn.transform.TransformDirection(bulletspawn.transform.forward * beamLength);
            //}

            float dist = Vector3.Distance(bulletspawn.transform.position, target);
            Vector3 nextbeamPartPosition = new Vector3(0, 0, 0);

            for (int i3 = 0; i3 < beamInterationsWantedPos.Length; i3++)
            {
                int i2 = (beamInterationsWantedPos.Length - i3) - 1;

                if (i2 >= 1) { beamInterationsTarget[i2] = beamInterationsTarget[i2 - 1]; }
                beamInterationsWantedPos[i2] = LerpByDistance(bulletspawn.transform.position, beamInterationsTarget[i2], (float)i2 / beamInterationsWantedPos.Length);
            }

            beamInterationsTarget[0] = target;

            float beamOffsetA = Random.Range(minBeamOffset, maxBeamOffset);
           
            for (int i = 0; i < beamLength; i++)
            {
                if (percentCounter <= 1)
                {
                    percentCounter += Random.Range(minxBeamPartDistance, maxBeamPartDistance);

                    int i2 = (int)(percentCounter * 10);

                    if (i2 >= 0 && i2 <= 9)
                    {
                        result = beamInterationsWantedPos[i2];
                    }

                    GameObject beamClone = Instantiate(bulletPrefab, bulletspawn.position, transform.rotation) as GameObject;
                    beamClone.gameObject.name = "Beam(" + i2 + ")";
                    beamClone.GetComponentInChildren<AgentReference>().spawner = gameObject; //Added by Jak - This line stores the current agent that fired the bullet, so i can access the Accuracy
                    Destroy(beamClone, Time.deltaTime + beamLife);

                    //Random postion of next beam part
                    float posX = Random.Range(-beamOffsetA, beamOffsetA);
                    float posY = Random.Range(-beamOffsetA, beamOffsetA);
                    float posZ = Random.Range(-beamOffsetA, beamOffsetA);
                    Vector3 beamOffset = new Vector3(posX * (1 - (Mathf.Abs(percentCounter - .5f)) * 2), posY * (1 - (Mathf.Abs(percentCounter - .5f)) * 2), posZ * (1 - (Mathf.Abs(percentCounter - .5f)) * 2));
                    result += beamOffset;

                    if (i >= 1)
                    {
                        beamClone.transform.position = nextbeamPartPosition;
                    }

                    if (percentCounter <= 1)
                    {
                        nextbeamPartPosition = result;
                    }
                    else
                    {
                        nextbeamPartPosition = target;
                    }
                    float beamDist = Vector3.Distance(beamClone.transform.position, nextbeamPartPosition);
                    beamClone.transform.localScale = new Vector3(1, 1, beamDist);

                    if (percentCounter <= 1)
                    {
                        beamClone.transform.LookAt(nextbeamPartPosition);
                        //particleBeam.transform.LookAt(nextbeamPartPosition);
                    }
                    else
                    {
                        beamClone.transform.LookAt(target);
                        particleBeam.GetComponentInChildren<Transform>().transform.LookAt(target);

                        if (beamDist >= dist /beamDist)
                        {
                            beamClone.transform.localScale = new Vector3(0, 0, 0);
                        }
                    }

                }

            }

        }

        percentCounter = 0;



    }





    Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        Vector3 P = Vector3.Lerp(A, B, x);
        return P;
    }
}