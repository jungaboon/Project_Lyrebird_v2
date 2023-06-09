using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMB_ResetTriggers : StateMachineBehaviour
{
    [SerializeField] private bool resetsGlobalAttacks = false;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(resetsGlobalAttacks) GameEvents.Instance.ResetAttacks();

        for (int i = 0; i < animator.parameters.Length; i++)
        {
            if (animator.parameters[i].type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(animator.parameters[i].name);
            }
        }
    }
}
