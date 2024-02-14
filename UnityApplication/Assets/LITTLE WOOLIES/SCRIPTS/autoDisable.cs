using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autoDisable : MonoBehaviour
{

    IEnumerator waiter(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        this.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void OnEnable()
    {
        StartCoroutine(waiter(2));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
