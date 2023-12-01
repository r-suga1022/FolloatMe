using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOperation : MonoBehaviour
{

    public OptitrackRigidBody target;
    // public Fairy character;
    public GameObject character;

    private Vector3 target_pos;
    public Vector3 pos_offset;
    public Vector3 pos_offset_mousever;
    private Vector3 latest_pos;
    public float pos_rate;

    bool IsFirstExecution = true;
    public bool IsTrackingStop = false;

    public bool mouse_prototyping;


    void Start()
    {

    }


    // void Update()
    // void FixedUpdate()
    void LateUpdate()
    {
        if (IsTrackingStop) return;

        // プロトタイプ段階ではマウス座標
        if (mouse_prototyping) {
            target_pos = Input.mousePosition;
            target_pos.z = 1.0f;
            Vector3 world_target_pos = Camera.main.ScreenToWorldPoint(target_pos);
            // character.Set_Position(target_pos, pos_offset_mousever);
            character.transform.position = target_pos + pos_offset_mousever;
            Debug.Log("Character Operation\n");
        // トラッキング座標を用いた追従の実現
        } else {
            target_pos = target.rbStatePosition;
            target_pos.z = 0f;
            // target_pos.z = -7.0f;
            Vector3 new_pos = new Vector3(target_pos.x*pos_rate, target_pos.y*pos_rate, target_pos.z);
            Quaternion new_rot = target.transform.rotation;
            // character.Set_Position(new_pos, pos_offset);
            // character.Set_Rotation(new_rot);
            character.transform.position = new_pos + pos_offset;
            // Debug.Log("鉛直、水平方向にキャラクターを移動");
            // character.transform.rotation = new_rot;
        }
    }
}
