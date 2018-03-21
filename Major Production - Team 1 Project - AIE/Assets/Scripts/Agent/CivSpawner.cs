////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <20/03/18>                               
// Brief: <Agent spawner for the Civillians, the spawner picks a point inside the sphere, then walks to that initial point>  
////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CivSpawner : MonoBehaviour
{
    public float spawnRadius = 10;
    [Range(10, 50)]public int numberOfAgents = 40;
    public static int currentSpawned = 0;
    public GameObject civPrefab;
    public Vector3 spherePos;
    private float timer = 0;

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= Random.Range(1, 2) && currentSpawned < numberOfAgents) //Spawn a civ every .25 seconds
        {
            Vector2 initialSpherePos = Random.insideUnitCircle * spawnRadius;
            Vector3 adjustedSpawnPos = new Vector3(transform.position.x + spherePos.x + initialSpherePos.x, transform.position.y, transform.position.z + spherePos.z + initialSpherePos.y);

            NavMeshHit hit;
            NavMesh.SamplePosition(adjustedSpawnPos, out hit, 100f, 1);
            Debug.DrawLine(hit.position, new Vector3(hit.position.x, 20f, hit.position.z), Color.red, 99f);
            adjustedSpawnPos = hit.position;


            //Instantiate the civ at the spawners inital position, because i want them to "walk" into the shop as if its realistic ish
            GameObject o = Instantiate(civPrefab, transform.position, transform.rotation);

            //Randomise Colours
            CivillianController civ = o.GetComponent<CivillianController>();
            civ.civilianPantsColour = Random.ColorHSV(0, 1, .5f, .7f, .5f, 1, 1, 1);
            civ.civilianTop1Colour = Random.ColorHSV(0, 1, .3f, .7f, .5f, 1, 1, 1);
            civ.civilianTop2Colour = Random.ColorHSV(0, 1, .3f, .7f, .5f, 1, 1, 1);
            civ.initialSpawn = true;

            if (Time.time > .1f)
            {
                o.GetComponent<NavMeshAgent>().enabled = true;
                o.GetComponent<NavMeshAgent>().SetDestination(adjustedSpawnPos); //Path towards the random point found within the spherePos(should be inside the mall)
            }


            Debug.DrawLine(adjustedSpawnPos, new Vector3(adjustedSpawnPos.x, 20f, adjustedSpawnPos.z), Color.green, 99f);
            



            currentSpawned++;
            timer = 0;
        }
    }

    //Debugging the radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + spherePos, spawnRadius);
    }
}
