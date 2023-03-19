using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FIMSpace;

public class PlayerMiscController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private Animator animator;
    [SerializeField] private LeaningAnimator leaningAnimator;

    private int _dodge = Animator.StringToHash("dodge");

    private void OnEnable()
    {
        inputHandler.onDodge += Dodge;
    }
    private void OnDisable()
    {
        inputHandler.onDodge -= Dodge;
    }
    private void Start()
    {
        Invoke("EnableLeaningAnimator", 0.5f);
    }
    private void Dodge()
    {
        animator.SetTrigger(_dodge);
    }
    private void EnableLeaningAnimator()
    {
        leaningAnimator.enabled = true;
    }
}
