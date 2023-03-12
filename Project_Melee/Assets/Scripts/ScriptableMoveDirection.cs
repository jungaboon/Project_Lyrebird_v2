using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "Scriptable Move Direction", menuName = "Scriptable Objects/Create Scriptable Move Direction")]
public class ScriptableMoveDirection : ScriptableObject
{
    public Vector3 moveDirection;
    public float duration;
    public bool constantSpeed = true;
    [ShowIf("constantSpeed")] public float moveSpeed = 5f;
    [HideIf("constantSpeed")] public AnimationCurve moveSpeedCurve;
}
