using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTest : MonoBehaviour
{
    public float DeltaXRate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float DeltaX = Input.GetAxis("Horizontal");
        Vector3 position = this.transform.position;
        position.x += DeltaX*DeltaXRate;
        this.transform.position = position;
    }
}
