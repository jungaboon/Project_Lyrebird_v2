using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private int lightAttackIndex;
    [SerializeField] private int heavyAttackIndex;
    [SerializeField] private float inputTime;
    [SerializeField] private float inputTimer;

    private void OnEnable()
    {
        inputHandler.onLightAttack += TriggerLightAttack;
        inputHandler.onHeavyAttack += TriggerHeavyAttacks;
    }
    private void OnDisable()
    {
        inputHandler.onLightAttack -= TriggerLightAttack;
        inputHandler.onHeavyAttack -= TriggerHeavyAttacks;
    }

    private void Update()
    {
        RemoveInputBuffer();
    }

    private void RemoveInputBuffer()
    {
        if (inputTime > 0f) inputTime -= Time.deltaTime;
        else
        {
            lightAttackIndex = 0;
            heavyAttackIndex = 0;
            ResetAllTriggers();
        }
    }

    private void ResetAllTriggers()
    {
        for (int i = 0; i < animator.parameters.Length; i++)
        {
            if(animator.parameters[i].type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(animator.parameters[i].name);
            }
        }
    }

    private void TriggerHeavyAttacks()
    {
        animator.SetTrigger($"heavyAttack{heavyAttackIndex}");
        inputTime = inputTimer;
        heavyAttackIndex++;
        if (heavyAttackIndex > 2)
        {
            lightAttackIndex = 2;
        }
    }

    private void TriggerLightAttack()
    {
        animator.SetTrigger($"lightAttack{lightAttackIndex}");
        inputTime = inputTimer;
        lightAttackIndex++;
        if (lightAttackIndex > 3)
        {
            lightAttackIndex = 3;
        }
    }

    public void LightAttack()
    {
        Collider[] coll = Physics.OverlapSphere(transform.position, 1.5f, enemyMask);
        for (int i = 0; i < coll.Length; i++)
        {
            float angleToTarget = Vector3.Angle(transform.forward, (coll[i].transform.position - transform.position));
            if(angleToTarget <= 45f)
            {
                Debug.Log($"Hit {coll[i]}");
            }
        }
    }
}
