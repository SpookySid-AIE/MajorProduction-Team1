////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <16/02/18>                               
// Brief: <This script needs to be on a second camera that will create a depth texture with normals to be used in "Highlight.shader">
////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RenderDepth : MonoBehaviour
{
    void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
    }
}