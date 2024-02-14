using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obstacleInteraction : MonoBehaviour
{
    private Animator anim;

    public void Start()
    {
        anim = GetComponentInParent<Animator>();
    }
    //get trigger on colission with obstacles
    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "rocks")
        {
            anim.SetTrigger("acting");
        }
    }
}
