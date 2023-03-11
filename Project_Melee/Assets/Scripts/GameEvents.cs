using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;

    private void Awake()
    {
        Instance = this;
    }

    public event Action onResetAttacks;
    public void ResetAttacks()
    {
        if (onResetAttacks != null) onResetAttacks();
    }
}
