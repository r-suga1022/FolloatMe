using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterOperation : MonoBehaviour
{
    // オブジェクト関係
    public OptitrackRigidBody _target;
    public GameObject _character;

    // 座標関係
    private Vector3 xvec_i;
    private Vector3 xvec_imin1;
    private Quaternion rotvec_i;
    public Vector3 PositionOffset;
    public Vector3 MousePositionOffset;
    public float XPositionRate;
    public float YPositionRate;

    float NewPositionX, NewPositionY;
    float BeforePositionX, BeforePositionY;
    public float MoveThreshold;
    

    // フラグ関係
    public bool TrackingStop = false;
    public bool MousePrototyping;
    bool CharacterRed = false;
    bool PositionChanged = false;

    public Text PositionDifferenceText;


    void Start()
    {

    }


    void Update()
    // void FixedUpdate()
    // void LateUpdate()
    {
        if (TrackingStop) return;

        // マウスに追従させるテスト
        if (MousePrototyping) {
            xvec_i = Input.mousePosition;
            xvec_i.z = 1.0f;
            Vector3 NewPosition = Camera.main.ScreenToWorldPoint(xvec_i);

            Vector3 NewPositionInScreen = Camera.main.WorldToScreenPoint(NewPosition + MousePositionOffset);

            UnityEngine.Debug.Log("screen = "+NewPositionInScreen);
            _character.transform.position = NewPosition + MousePositionOffset;

        // トラッキングに基づく追従
        } else {
            // 座標取得
            xvec_imin1 = xvec_i;
            xvec_i = _target.rbStatePosition;
            rotvec_i = _target.rbStateOrientation;

            // 座標計算
            if (!_target.TrackingDone) return;
            BeforePositionX = NewPositionX; BeforePositionY = NewPositionY;
            NewPositionX = (xvec_i.x + (xvec_i.x - xvec_imin1.x)*25f/1000f)*XPositionRate + PositionOffset.x;
            NewPositionY = (xvec_i.y + (xvec_i.y - xvec_imin1.y)*25f/1000f)*YPositionRate + PositionOffset.y;
            //NewPositionX = xvec_imin1.x + (xvec_i.x - xvec_imin1.x)*XPositionRate + PositionOffset.x;
            //NewPositionY = xvec_imin1.y + (xvec_i.y - xvec_imin1.y)*YPositionRate + PositionOffset.y;
            //NewPositionX = xvec_i.x*XPositionRate + PositionOffset.x;
            //NewPositionY = xvec_i.y*YPositionRate + PositionOffset.y;
            Vector3 NewPosition = new Vector3(NewPositionX, NewPositionY, -1f);

            // 姿勢
            Quaternion new_rot = _target.rbStateOrientation;

            //bool XChanged = Mathf.Abs(NewPositionX - BeforePositionX) > MoveThreshold;
            //bool YChangedPositive = (NewPositionY - BeforePositionY) > MoveThreshold;
            bool XChanged = (xvec_i.x - xvec_imin1.x) > MoveThreshold;
            bool YChanged = false;
            PositionChanged = XChanged | YChanged;


            // 遅延測定の際、色を変える
            if (_target.LatencyMeasuring)
            {
                if (_target.TrackingDone && PositionChanged && !CharacterRed)
                {
                    _character.GetComponent<Renderer>().material.color = Color.red;
                    CharacterRed = true;
                }

                else if (_target.TrackingDone && !PositionChanged && CharacterRed)
                {
                    _character.GetComponent<Renderer>().material.color = Color.white;
                    CharacterRed = false;
                }

                if (Input.GetKeyDown(KeyCode.W))
                {
                    _character.GetComponent<Renderer>().material.color = Color.white;
                    CharacterRed = false;
                }
            }

            /*
            if (_target.TrackingDone)
            {  
                PositionDifferenceText.text = (xvec_i.x - xvec_imin1.x).ToString();
            }
            */

            //if (!PositionChanged) NewPosition = BeforePosition;
            _character.transform.position = NewPosition;
            //_character.transform.rotation = rotvec_i;
        }
    }
}
