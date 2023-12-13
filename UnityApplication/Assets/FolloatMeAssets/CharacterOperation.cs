using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOperation : MonoBehaviour
{

    public OptitrackRigidBody target;
    // public Fairy character;
    public GameObject character;

    private Vector3 xvec_i;
    public Vector3 pos_offset;
    public Vector3 pos_offset_mousever;
    private Vector3 latest_pos;
    public float Xpos_rate;
    public float Ypos_rate;

    private Vector3 xvec_imin1;
    private Vector3 xvec_initial;

    bool IsFirstExecution = true;
    public bool IsTrackingStop = false;

    public bool mouse_prototyping;

    bool CharacterRed = false;

    public float proportion;

    void Start()
    {
        xvec_initial = pos_offset;
    }


    // void Update()
    // void FixedUpdate()
    void LateUpdate()
    {
        if (IsTrackingStop) return;

        // プロトタイプ段階ではマウス座標
        if (mouse_prototyping) {
            xvec_i = Input.mousePosition;
            xvec_i.z = 1.0f;
            Vector3 world_xvec_i = Camera.main.ScreenToWorldPoint(xvec_i);
            // character.Set_Position(xvec_i, pos_offset_mousever);
            character.transform.position = xvec_i + pos_offset_mousever;
            Debug.Log("Character Operation\n");
        // トラッキング座標を用いた追従の実現
        } else {
            //if (!target.PositionChanged) return;
            xvec_imin1 = xvec_i;
            xvec_i = target.rbStatePosition;
            xvec_i.z = 0f;
            //Vector3 new_pos = xvec_imin1 + (xvec_i - xvec_imin1)*proportion;
            //Vector3 new_pos = xvec_i;
            //xvec_i.z = 1f;
            //Vector3 new_pos = Camera.main.ScreenToWorldPoint(xvec_i); //new_pos.z = 0f;
            Vector3 new_pos = (xvec_i + (xvec_i - xvec_imin1)*25f/1000f)*Xpos_rate + pos_offset;
            //Vector3 new_pos = new Vector3(xvec_i.x*Xpos_rate + pos_offset.x, xvec_i.y*Ypos_rate + pos_offset.y, xvec_i.z);
            Quaternion new_rot = target.transform.rotation;
            character.transform.position = new_pos;

            if (target.LatencyMeasuring)
            {
                if (target.PositionChanged && !CharacterRed)
                {
                    character.GetComponent<Renderer>().material.color = Color.red;
                    CharacterRed = true;
                }

                if (Input.GetKeyDown(KeyCode.W))
                //if (!target.PositionChanged && CharacterRed)
                {
                    character.GetComponent<Renderer>().material.color = Color.white;
                    CharacterRed = false;
                }
            }
            // Debug.Log("鉛直、水平方向にキャラクターを移動");
            // character.transform.rotation = new_rot;
        }
    }
}
