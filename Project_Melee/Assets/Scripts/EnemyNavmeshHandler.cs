using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavmeshHandler : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    private Coroutine moveAgentCoroutine;

    public void MoveAgent(ScriptableMoveDirection scriptableMoveDirection)
    {
        if (moveAgentCoroutine != null) StopCoroutine(moveAgentCoroutine);
        moveAgentCoroutine = StartCoroutine(_MoveAgent());

        IEnumerator _MoveAgent()
        {
            float time = 0f;
            while(time < scriptableMoveDirection.duration)
            {
                Vector3 moveDir = transform.forward * scriptableMoveDirection.moveDirection.z + transform.right * scriptableMoveDirection.moveDirection.x;
                if (scriptableMoveDirection.constantSpeed)
                {
                    agent.Move(moveDir * scriptableMoveDirection.moveSpeed * Time.deltaTime);
                }
                else
                {
                    float timePercent = time / scriptableMoveDirection.duration;
                    agent.Move(moveDir * scriptableMoveDirection.moveSpeedCurve.Evaluate(timePercent) * Time.deltaTime);
                }

                time += Time.deltaTime;
                yield return null;
            }
        }
    }
}
