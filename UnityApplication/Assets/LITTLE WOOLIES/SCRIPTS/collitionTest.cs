using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collitionTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnCollisionEnter (Collision other)
    {
        Debug.Log(other.gameObject.tag);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
