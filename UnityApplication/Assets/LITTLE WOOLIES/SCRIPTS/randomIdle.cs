using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomIdle : StateMachineBehaviour
{
    float num = 0;
    float num2 = 0;
    public string trig;
    public float mintime = 0f;
    public float maxtime = 1f;

    
    public string parameter;
    public float time = 3f;
    public int min = 0;
    public int max = 2;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        num = 0f;
        num2 = 0f;
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetFloat("Y") < 0.1f)
        {
            num += Time.deltaTime;
            int seconds = (int)(num % 60);
            //Debug.Log(num);
            if (seconds >= time)
            {
                //Debug.Log("entro");
                animator.SetInteger(parameter,Random.Range(min,max+1));
                num = 0f;
            }
        }
        //condicion random para caerse
        //Debug.Log(Mathf.Pow(animator.GetFloat("Y") - 2.0f,2));
        if (Mathf.Pow((animator.GetFloat("Y")-2.0f),2)<0.001f)
        {
            num2 += Time.deltaTime;
            //Debug.Log(num2);
            if (num2 > Random.Range(mintime,maxtime))
            {
                //animator.SetTrigger(trig);
                num2 = 0f;
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
        
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
