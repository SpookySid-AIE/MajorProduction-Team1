using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script_AnimationEventTriggerParticles : MonoBehaviour
{

    public ParticleSystem particle1;
    public ParticleSystem particle2;

    public bool playPaticle1 = false;
    public bool playPaticle2 = false;



    private void Update()
    {
        if (playPaticle1)
        {
            playPaticle1 = false;
            particle1.Play();
        }
        if (playPaticle2)
        {
            playPaticle2 = false;
            particle2.Play();
        }


    }
}
