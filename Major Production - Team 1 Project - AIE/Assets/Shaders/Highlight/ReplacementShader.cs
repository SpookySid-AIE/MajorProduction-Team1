////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <16/02/18>                               
// Brief: <This script needs to be on a second camera that will render the highlight shader>
////////////////////////////////////////////////////////////
using UnityEngine;

[ExecuteInEditMode]
public class ReplacementShader : MonoBehaviour
{
    public Shader HighlightShader;

    void OnEnable()
    {
        GetComponent<Camera>().SetReplacementShader(HighlightShader, "EdgeHighlight");
    }
}
