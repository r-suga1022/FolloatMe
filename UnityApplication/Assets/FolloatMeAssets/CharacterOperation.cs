using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterOperation : MonoBehaviour
{
    // オブジェクト関係
    //public OptitrackRigidBody _target;
    public GameObject _MouseCharacter;
    public List<GameObject> _characterlist;
    public GameObject _LookAtTarget;
    public List<OptitrackRigidBody> _targetlist = new List<OptitrackRigidBody>();
    public OptitrackRigidBody _target;
    public int CurrentCharacterNumber;
    public GameObject _character;
 
    // 座標関係
    private Vector3 xvec_i;
    private Vector3 xvec_imin1;
    private Quaternion rotvec_i;
    public Vector3 PositionOffset;
    public Vector3 MousePositionOffset;
    public Vector3 OrientationOffset;
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
    bool CharacterActive = true;

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
            FollowToMouse();
        // トラッキングに基づく追従
        } else {
            for (CurrentCharacterNumber = 0; CurrentCharacterNumber < _characterlist.Count; ++CurrentCharacterNumber)
            //foreach (OptitrackRigidBody _target in _targetlist)
            {
                _target = _targetlist[CurrentCharacterNumber];
                _character = _characterlist[CurrentCharacterNumber];

                // 座標取得
                xvec_imin1 = xvec_i;
                xvec_i = _target.rbStatePosition;
                rotvec_i = _target.rbStateOrientation;

                // 座標計算
                //if (!_target.TrackingDone) return;
                //UnityEngine.Debug.Log("ID = "+_target.RigidBodyId);
                BeforePositionX = NewPositionX; BeforePositionY = NewPositionY;
                NewPositionX = (xvec_i.x + (xvec_i.x - xvec_imin1.x)*25f/1000f)*XPositionRate + PositionOffset.x;
                NewPositionY = (xvec_i.y + (xvec_i.y - xvec_imin1.y)*25f/1000f)*YPositionRate + PositionOffset.y;
                //NewPositionX = xvec_imin1.x + (xvec_i.x - xvec_imin1.x)*XPositionRate + PositionOffset.x;
                //NewPositionY = xvec_imin1.y + (xvec_i.y - xvec_imin1.y)*YPositionRate + PositionOffset.y;
                //NewPositionX = xvec_i.x*XPositionRate + PositionOffset.x;
                //NewPositionY = xvec_i.y*YPositionRate + PositionOffset.y;
                Vector3 NewPosition = new Vector3(NewPositionX, NewPositionY, -1f);

                // 姿勢
                Vector3 new_rot_vec3 = _target.rbStateOrientation.eulerAngles + OrientationOffset;
                Quaternion new_rot = _target.rbStateOrientation;

                //bool XChanged = Mathf.Abs(NewPositionX - BeforePositionX) > MoveThreshold;
                //bool YChangedPositive = (NewPositionY - BeforePositionY) > MoveThreshold;
                bool XChanged = (xvec_i.x - xvec_imin1.x) > MoveThreshold;
                bool YChanged = false;
                PositionChanged = XChanged | YChanged;

                //if (!PositionChanged) NewPosition = BeforePosition;
                _character.transform.position = NewPosition;
                _character.transform.LookAt(_LookAtTarget.transform);
                //_character.transform.rotation = Quaternion.Euler(new_rot_vec3);
                _character.transform.rotation = new_rot;


                LatencyMeasure();
                ChangeOffset();
            }
        }
    }

    public float PositionOffsetChangeIncrement;
    void ChangeOffset()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) PositionOffset.y += PositionOffsetChangeIncrement;
        if (Input.GetKeyDown(KeyCode.DownArrow)) PositionOffset.y -= PositionOffsetChangeIncrement;
        if (Input.GetKeyDown(KeyCode.RightArrow)) PositionOffset.x += PositionOffsetChangeIncrement;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) PositionOffset.x -= PositionOffsetChangeIncrement;
    }


    void FollowToMouse()
    {
        xvec_i = Input.mousePosition;
        xvec_i.z = 1.0f;
        Vector3 NewPosition = Camera.main.ScreenToWorldPoint(xvec_i);
        Vector3 CharacterToLookAtTarget = _LookAtTarget.transform.position - _MouseCharacter.transform.position;
        // Quaternion NewOrientation = Quaternion.LookRotation(CharacterToLookAtTarget);
        // _character.transform.LookAt(_LookAtTarget.transform);

        Vector3 NewPositionInScreen = Camera.main.WorldToScreenPoint(NewPosition + MousePositionOffset);

        UnityEngine.Debug.Log("screen = "+NewPositionInScreen);
        _MouseCharacter.transform.position = NewPosition + MousePositionOffset;
        _MouseCharacter.transform.LookAt(_LookAtTarget.transform);
        // _character.transform.rotation = NewOrientation;}
    }

    void LatencyMeasure()
    {
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
    }
}
