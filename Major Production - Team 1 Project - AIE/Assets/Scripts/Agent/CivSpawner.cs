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
    public GameObject enemyPrefab;
    public Vector3 spherePos;
    private float timer = 0;

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= 1 && currentSpawned < numberOfAgents) //Spawn a civ every .25 seconds
        {
            Vector2 randomLoc2d = Random.insideUnitCircle * spawnRadius;
            Vector3 randomLoc3d = new Vector3(transform.position.x + spherePos.x + randomLoc2d.x, transform.position.y, transform.position.z + spherePos.z + randomLoc2d.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomLoc3d, out hit, spawnRadius, -1))
            {
                randomLoc3d = hit.position;
            }

            //Instantiate the civ at the spawners inital position, because i want them to "walk" into the shop as if its realistic ish
            GameObject o = (GameObject)Instantiate(enemyPrefab, transform.position, transform.rotation);
            o.GetComponent<NavMeshAgent>().SetDestination(randomLoc3d); //Path towards the random point found within the spherePos(should be inside the mall)

            currentSpawned++;
            timer = 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + spherePos, spawnRadius);
    }
}
