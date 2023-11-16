using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowDelay : MonoBehaviour
{
    bool IsFirstExecution = true;
    public int followdelay;
    int count = 0;
    
    List<Vector3> positions_list = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3 set_position(Vector3 pos, Vector3 offset, int delay, bool IsFirstExecution)
    {
        if (IsFirstExecution) {
            positions_list.Add(pos);
            if (count == delay) {
                IsFirstExecution = false;
                count = 0;
            }
            ++count;
            // return;
        }

        positions_list.Add(pos);
        Vector3 pos2 = positions_list[0];
        positions_list.RemoveAt(0);
        return pos2 + offset;
    }
}
