////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <00/00/00>                               
// Brief: <Description>  
////////////////////////////////////////////////////////////
using UnityEngine;

public class TriggerHighlight : MonoBehaviour
{
    private Material highlightMaterial;
    private Material old;
    private Renderer render;

    private void Start()
    {
        highlightMaterial = GetComponentInParent<playerPossession>().highlightMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Civillian")
        {
            render = other.GetComponentInChildren<Renderer>();
            old = render.material;
            render.material = highlightMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Civillian")
            render.material = old;
    }
}
