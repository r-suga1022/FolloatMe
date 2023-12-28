using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOperation : MonoBehaviour
{
    // オブジェクト関係
    public OptitrackRigidBody _target;
    public GameObject _character;

    // 座標関係
    private Vector3 xvec_i;
    private Vector3 xvec_imin1;
    public Vector3 PositionOffset;
    public Vector3 MousePositionOffset;
    public float XPositionRate;
    public float YPositionRate;
    

    // フラグ関係
    public bool TrackingStop = false;
    public bool MousePrototyping;
    bool CharacterRed = false;


    void Start()
    {

    }


    // void Update()
    // void FixedUpdate()
    void LateUpdate()
    {
        if (TrackingStop) return;

        // マウスに追従させるテスト
        if (MousePrototyping) {
            xvec_i = Input.mousePosition;
            xvec_i.z = 1.0f;
            Vector3 NewPosition = Camera.main.ScreenToWorldPoint(xvec_i);
            _character.transform.position = NewPosition + MousePositionOffset;

        // トラッキングに基づく追従
        } else {
            // 座標取得
            xvec_imin1 = xvec_i;
            xvec_i = _target.rbStatePosition;

            // 座標計算
            float NewPositionX = (xvec_i.x + (xvec_i.x - xvec_imin1.x)*25f/1000f)*XPositionRate + PositionOffset.x;
            float NewPositionY = (xvec_i.y + (xvec_i.y - xvec_imin1.y)*25f/1000f)*YPositionRate + PositionOffset.y;
            Vector3 NewPosition = new Vector3(NewPositionX, NewPositionY, 0f);

            // 姿勢
            Quaternion new_rot = _target.rbStateRotation;

            _character.transform.position = NewPosition;


            // 遅延測定の際、色を変える
            if (_target.LatencyMeasuring)
            {
                if (_target.PositionChanged && !CharacterRed)
                {
                    _character.GetComponent<Renderer>().material.color = Color.red;
                    CharacterRed = true;
                }

                if (Input.GetKeyDown(KeyCode.W))
                {
                    _character.GetComponent<Renderer>().material.color = Color.white;
                    CharacterRed = false;
                }
            }
        }
    }
}
