using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Cinemachine;
 
public class Fairy : MonoBehaviour
{
    public GameObject target;
 
    CharacterController con;
    Animator anim;

    Vector3 position; // 現在位置
    Vector3 delta_position; // 位置の変化
    Vector3 before_position; // １フレーム前の位置
    Vector3 startPos;


    List<Vector3> positions_list = new List<Vector3>();

 
    float normalSpeed = 6f; // 通常時の移動速度
    float sprintSpeed = 5f; // ダッシュ時の移動速度
    float jump = 8f;        // ジャンプ力
    //float gravity = 10f;    // 重力の大きさ
    float gravity = 0f;    // 重力の大きさ
 
    Vector3 moveDirection = Vector3.zero;

    bool IsFirstExecution = true;
    public int followdelay;
    int count = 0;
 
 
 
    void Start()
    {
        con = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
       
        startPos = transform.position;

        delta_position = Vector3.zero;

        // positions_list.Add(startPos);
    }
 
    void Update()
    {
        // キャラクターの向きの計算
        // transform.LookAt(target.transform.position);
        Make_Animation();
    }

    // おそらくオーバーライド
    public void Set_Position(Vector3 pos, Vector3 offset)
    {
        /*
        if (IsFirstExecution) {
            positions_list.Add(pos);
            if (count == followdelay) {
                IsFirstExecution = false;
                count = 0;
            }
            ++count;
            return;
        }
        */

        /*
        positions_list.Add(pos);
        Vector3 pos2 = positions_list[0];
        positions_list.RemoveAt(0);
        transform.position = pos2 + offset;
        */

        transform.position = pos + offset;
    }

    public void Set_Rotation(Quaternion rot)
    {
        transform.rotation = rot;
    }

    public void Make_Animation()
    {
        // Debug.Log("flag = "+anim.die);
    }
}