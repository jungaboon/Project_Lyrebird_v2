using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private InputAction moveInputAction;
    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private InputAction jumpAction;
    private InputAction dodgeAction;

    public delegate void OnJump();
    public OnJump onJump;

    public delegate void OnLightAttack();
    public OnLightAttack onLightAttack;

    public delegate void OnHeavyAttack();
    public OnHeavyAttack onHeavyAttack;

    public delegate void OnDodge();
    public OnDodge onDodge;

    private void OnEnable()
    {
        playerInputActions = new PlayerInputActions();
        moveInputAction = playerInputActions.Player.Move;
        moveInputAction.Enable();

        lightAttackAction = playerInputActions.Player.LightAttack;
        lightAttackAction.Enable();
        lightAttackAction.started += _ => LightAttack();

        heavyAttackAction = playerInputActions.Player.HeavyAttack;
        heavyAttackAction.Enable();
        heavyAttackAction.started += _ => HeavyAttack();

        jumpAction = playerInputActions.Player.Jump;
        jumpAction.Enable();
        jumpAction.started += _ => Jump();

        dodgeAction = playerInputActions.Player.Dodge;
        dodgeAction.Enable();
        dodgeAction.started += _ => Dodge();
    }

    private void OnDisable()
    {
        moveInputAction.Disable();
        lightAttackAction.Disable();
        jumpAction.Disable();
    }

    public Vector2 MoveInput()
    {
        return moveInputAction.ReadValue<Vector2>();
    }

    private void Jump() { onJump?.Invoke(); }
    private void LightAttack() { onLightAttack?.Invoke(); }
    private void HeavyAttack() { onHeavyAttack?.Invoke(); }
    private void Dodge() { onDodge?.Invoke(); }
}
