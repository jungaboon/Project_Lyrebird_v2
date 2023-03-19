using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveController : MonoBehaviour
{
    [SerializeField] private PlayerStateHandler playerStateHandler;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Camera mainCam;
    [HideInInspector] public Transform cam;
    [HideInInspector] public Animator animator;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private PlayerInputHandler inputHandler;

    public Vector3 moveDirection;
    public Vector3 inputDirection;
    public Vector3 additiveDirection;

    [HideInInspector] public bool canDoubleJump;
    [SerializeField] private float groundcheckRadius = 0.2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothDampMultiplier = 0.2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 5f;
    private RaycastHit groundHit;
    public float Gravity() { return gravity; }

    [HideInInspector] public float turnSmoothVelocity;
    [HideInInspector] public float velocity;
    [HideInInspector] public bool grounded, previouslyGrounded;

    private int _velocity = Animator.StringToHash("velocity");
    private int _grounded = Animator.StringToHash("grounded");

    [SerializeField] private UnityEvent onLeaveGround;
    [SerializeField] private UnityEvent onReachGround;
    [SerializeField] private UnityEvent onDoubleJump;

    private void OnEnable()
    {
        inputHandler.onJump += Jump;
    }
    private void OnDisable()
    {
        inputHandler.onJump -= Jump;
    }

    public virtual void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCam = Camera.main;
        cam = mainCam.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public virtual void Update()
    {
        GroundCheck();
        GetMoveDirection();
        ApplyMove();
        ApplyGravity();
        switch (playerStateHandler.PlayerState)
        {
            case PlayerState.Normal:
                animator.applyRootMotion = true;
                break;
            case PlayerState.Airborne:
                animator.applyRootMotion = false;
                break;
        }
    }

    public virtual void GetMoveDirection()
    {
        velocity = inputHandler.MoveInput().sqrMagnitude;

        switch(playerStateHandler.PlayerState)
        {
            case PlayerState.Normal:
                if (velocity >= 0.01f)
                {
                    float targetAngle = Mathf.Atan2(inputHandler.MoveInput().x, inputHandler.MoveInput().y) * Mathf.Rad2Deg + cam.eulerAngles.y;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothDampMultiplier);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    inputDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                }
                break;
            case PlayerState.Airborne:
                if (velocity >= 0.01f)
                {
                    float targetAngle = Mathf.Atan2(inputHandler.MoveInput().x, inputHandler.MoveInput().y) * Mathf.Rad2Deg + cam.eulerAngles.y;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothDampMultiplier);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    inputDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                }
                else inputDirection = Vector3.zero;
                break;
            case PlayerState.Wallrunning:
                if (velocity >= 0.01f)
                {
                    float targetAngle = Mathf.Atan2(inputHandler.MoveInput().x, inputHandler.MoveInput().y) * Mathf.Rad2Deg + cam.eulerAngles.y;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothDampMultiplier);

                    inputDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                }
                else inputDirection = Vector3.zero;
                break;
        }

        animator.SetFloat(_velocity, velocity, 0.07f, Time.deltaTime);
    }
    private void ApplyMove()
    {
        switch(playerStateHandler.PlayerState)
        {
            case PlayerState.Normal:
                break;
            case PlayerState.Airborne:
                moveDirection = inputDirection.normalized * moveSpeed;
                moveDirection += additiveDirection;
                controller.Move(moveDirection * Time.deltaTime);
                break;
            case PlayerState.Climbing:
                break;
            case PlayerState.Wallrunning:
                controller.Move(additiveDirection * Time.deltaTime);
                break;
        }

        Debug.DrawRay(transform.position, inputDirection, Color.green);
        additiveDirection = Vector3.Lerp(additiveDirection, Vector3.zero, 5f * Time.deltaTime);
    }
    public virtual void GroundCheck()
    {
        grounded = Physics.SphereCast(transform.position + transform.up * 0.1f, groundcheckRadius, Vector3.down, out groundHit, 0.5f, groundMask);
        animator.SetBool(_grounded, grounded);

        if(!grounded && previouslyGrounded)
        {
            OnLeaveGround();
        }
        if (grounded && !previouslyGrounded)
        {
            OnReachGround();
        }
        
        previouslyGrounded = grounded;
    }
    private void OnLeaveGround()
    {
        playerStateHandler.SetPlayerState(PlayerState.Airborne);
        canDoubleJump = true;
        onLeaveGround.Invoke();
    }
    private void OnReachGround()
    {
        controller.enabled = false;
        transform.position = new Vector3(transform.position.x, groundHit.point.y + 0.05f, transform.position.z);
        controller.enabled = true;
        playerStateHandler.SetPlayerState(PlayerState.Normal);
        onReachGround.Invoke();
        moveDirection = Vector3.zero;
    }
    private void ApplyGravity()
    {
        switch(playerStateHandler.PlayerState)
        {
            case PlayerState.Normal:
                additiveDirection.y = -10f;
                break;
            case PlayerState.Airborne:
                additiveDirection.y -= Time.deltaTime * gravity;
                break;
            case PlayerState.Climbing:
                break;
            case PlayerState.Wallrunning:
                additiveDirection.y -= 2f * Time.deltaTime;
                break;
        }
    }
    public virtual void Jump()
    {
        switch(playerStateHandler.PlayerState)
        {
            case PlayerState.Normal:
                animator.Play("Jump Start", 0, 0f);
                playerStateHandler.SetPlayerState(PlayerState.Airborne);
                canDoubleJump = true;
                additiveDirection.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
                break;
            case PlayerState.Airborne:
                if (!canDoubleJump) return;
                animator.Play("Double Jump Start", 0, 0f);
                playerStateHandler.SetPlayerState(PlayerState.Airborne);
                canDoubleJump = false;
                onDoubleJump.Invoke();
                additiveDirection.y = Mathf.Sqrt(jumpHeight * 4f * gravity);
                break;
        }
    }
    public void ResetAdditiveVelocity()
    {
        additiveDirection = Vector3.zero;
    }
    public void AddVelocity(Vector3 v)
    {
        additiveDirection = v;
    }
}
