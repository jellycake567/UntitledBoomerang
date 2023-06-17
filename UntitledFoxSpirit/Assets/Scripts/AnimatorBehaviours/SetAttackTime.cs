using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;

public class SetAttackTime : StateMachineBehaviour
{
    [Header("Normalised Time")]
    [SerializeField][Range(0, 1)] public float allowInputTime = 0.1f; // Eg: After 0.1s of normalised animation time, allow attack input
    [SerializeField][Range(0, 1)] public float resetComboTime = 0.3f; // Reset combo count to 0

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("attackInputTime", allowInputTime);
        animator.SetFloat("resetComboTime", resetComboTime);

        int atkNum = Random.Range(1, 6);
        if (animator.GetInteger("LastAtkInt") == atkNum)
        {
            atkNum++;

            if (atkNum > 5)
                atkNum = 1;
        }

        animator.SetInteger("LastAtkInt", atkNum);
        animator.SetInteger("RngAttack", atkNum);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isAttacking", false);
    }

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
