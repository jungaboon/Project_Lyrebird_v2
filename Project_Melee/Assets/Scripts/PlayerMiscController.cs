using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMiscController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private Animator animator;

    private int _dodge = Animator.StringToHash("dodge");

    private void OnEnable()
    {
        inputHandler.onDodge += Dodge;
    }
    private void OnDisable()
    {
        inputHandler.onDodge -= Dodge;
    }
    private void Dodge()
    {
        Debug.Log("Dodge");
        animator.SetTrigger(_dodge);
    }
}
