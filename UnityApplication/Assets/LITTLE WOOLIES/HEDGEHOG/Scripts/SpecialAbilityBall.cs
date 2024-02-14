using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAbilityBall : MonoBehaviour
{
    public Avatar ball;
    public Avatar erizo;
    public float acceleration=3f;
    public Transform Roll;
    public float spin=40f;
    public float normalAgent = 2.0f;
    public float ballAgent = 3.0f;

    private Animator anim;
    private bool avatarOnOff = true;
    private float target;
    private float intOld;
    private float rot;
    private float targetRot;
    private float inputRot;
    private int idleTy;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    // rotation state
    float roll()
    {
        if (avatarOnOff)
        {
            rot = Mathf.LerpAngle(inputRot, targetRot, spin * Time.deltaTime *5.0f);
            //Debug.Log(targetRot + "<-targetRot;rot ->" + rot);
        }
        else
        {
            rot += spin * Time.deltaTime * 5.0f * anim.GetFloat("Y") * anim.GetFloat("X");
        }
        return rot;
    }

    // update avatar
    void avatarChange()
    {
        if (avatarOnOff)
        {
            idleTy = anim.GetInteger("idleType");
            intOld = anim.GetFloat("X");
            anim.avatar = ball;
            anim.Rebind();
            anim.SetFloat("X", intOld);
            anim.SetInteger("idleType", 0);
            target = 1.0f;
            this.gameObject.GetComponent<woolies_controller>().agentSpeed = ballAgent;
            avatarOnOff = false;
        }
        else
        {
            intOld = anim.GetFloat("X");
            anim.avatar = erizo;
            // devuelve target rot correcto
            targetRot = Mathf.RoundToInt(rot / 360) * 360;
            inputRot = transform.localRotation.x;
            anim.Rebind();
            anim.SetFloat("X", intOld);
            anim.SetInteger("idleType", idleTy);
            target = 0.0f;
            this.gameObject.GetComponent<woolies_controller>().agentSpeed = normalAgent;
            avatarOnOff = true;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            avatarChange();
        }
        float interpolX = Mathf.Lerp(anim.GetFloat("X"), target, acceleration * Time.deltaTime);
        anim.SetFloat("X", interpolX);
        //Debug.Log(target+"<-target y anim->" + anim.GetFloat("X"));

        roll();
        //Debug.Log(Mathf.RoundToInt(rot/360)*360+"<-target,rot->"+rot);
        Roll.transform.localRotation = Quaternion.Euler(rot,0,0);
    }
}
