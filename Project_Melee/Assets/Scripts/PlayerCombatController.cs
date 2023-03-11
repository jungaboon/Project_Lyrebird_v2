using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private int maxLightAttacks;
    [SerializeField] private int lightAttackIndex;
    [SerializeField] private int maxHeavyAttacks;
    [SerializeField] private int heavyAttackIndex;
    [SerializeField] private float inputTime;
    [SerializeField] private float inputTimer;
    [Space]
    [SerializeField] private UnityEvent onStartAttack;
    [SerializeField] private UnityEvent onEndAttack;
    [SerializeField] private UnityEvent onHitEnemy;

    [SerializeField] private Transform currentTarget;
    [SerializeField] private float targetRadius = 2f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float turnTimer = 0.3f;
    [SerializeField] private float turnTime;
    [SerializeField] private bool turningToTarget;

    private void OnEnable()
    {
        inputHandler.onLightAttack += TriggerLightAttack;
        inputHandler.onHeavyAttack += TriggerHeavyAttacks;
        GameEvents.Instance.onResetAttacks += ResetAttackIndex;
    }
    private void OnDisable()
    {
        inputHandler.onLightAttack -= TriggerLightAttack;
        inputHandler.onHeavyAttack -= TriggerHeavyAttacks;
        GameEvents.Instance.onResetAttacks -= ResetAttackIndex;
    }

    private void Update()
    {
        RemoveInputBuffer();
    }
    private void LateUpdate()
    {
        TurnToClosestTarget();
    }

    private void TurnToClosestTarget()
    {
        if (currentTarget == null) return;
        if (turnTime < turnTimer)
        {
            turnTime += Time.deltaTime;
            Vector3 toTarget = currentTarget.position - transform.position;
            toTarget.y = 0f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), Time.deltaTime * turnSpeed);
        }
        else
        {
            currentTarget = null;
        }
    }

    private void FindNearbyTargets()
    {
        Collider[] coll = Physics.OverlapSphere(transform.position, targetRadius, enemyMask);
        List<float> targetDistances = new List<float>();
        for (int i = 0; i < coll.Length; i++)
        {
            targetDistances.Add(Vector3.Distance(transform.position, coll[i].transform.position));
        }
        if (targetDistances.Count == 0) return;
        float closestDistance = targetDistances.Min();
        int indexOfClosestTarget = targetDistances.IndexOf(closestDistance);
        currentTarget = coll[indexOfClosestTarget].transform;
        turnTime = 0f;
    }

    private void RemoveInputBuffer()
    {
        if (inputTime > 0f) inputTime -= Time.deltaTime;
        else
        {
            lightAttackIndex = 0;
            heavyAttackIndex = 0;
            ResetAllTriggers();
            inputTime = inputTimer;
        }
    }
    private void ResetAttackIndex()
    {
        lightAttackIndex = 0;
        heavyAttackIndex = 0;
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
        onEndAttack.Invoke();
    }

    private void TriggerHeavyAttacks()
    {
        onStartAttack.Invoke();
        animator.SetTrigger($"heavyAttack{heavyAttackIndex}");
        inputTime = inputTimer;
        heavyAttackIndex++;
        if (heavyAttackIndex > maxHeavyAttacks)
        {
            lightAttackIndex = maxHeavyAttacks;
        }
        FindNearbyTargets();
    }

    private void TriggerLightAttack()
    {
        onStartAttack.Invoke();
        animator.SetTrigger($"lightAttack{lightAttackIndex}");
        inputTime = inputTimer;
        lightAttackIndex++;
        if (lightAttackIndex > maxLightAttacks)
        {
            lightAttackIndex = maxLightAttacks;
        }
        FindNearbyTargets();
    }

    public void Attack(ScriptableAttack attackParam)
    {
        Collider[] coll = Physics.OverlapSphere(transform.position, attackParam.attackRange, enemyMask);
        for (int i = 0; i < coll.Length; i++)
        {
            Vector3 attackDirection = (transform.forward * attackParam.attackDirection.z) + (transform.right * attackParam.attackDirection.x);
            Debug.DrawRay(transform.position, attackDirection * attackParam.attackRange, Color.red, 3f);
            float angleToTarget = Vector3.Angle(attackDirection, (coll[i].transform.position - transform.position));
            if(angleToTarget <= attackParam.attackAngle)
            {
                if (coll[i].transform.TryGetComponent(out IDamage iDamage))
                {
                    onHitEnemy.Invoke();
                    iDamage.Damage(attackDirection, attackParam.attackDamage, attackParam.damagePoint, attackParam.knockBack, attackParam.knockbackDirection);
                }
            }
        }
    }
}
