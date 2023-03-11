using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveController : MonoBehaviour
{
    private enum PlayerState
    {
        Normal,
        Airborne,
        Attacking,
        Hurt
    }
    private PlayerState playerState;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Camera mainCam;
    [HideInInspector] public Transform cam;
    [HideInInspector] public Animator animator;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private PlayerInputHandler inputHandler;

    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public Vector3 playerVelocity;

    public bool faceCameraDirection;
    public float moveSpeed = 3f;
    public float turnSpeed = 0.2f;
    public float smoothDampMultiplier = 0.2f;
    public float defaultStepOffset = 0.3f;
    public float gravity = -9.8f;
    public float jumpHeight = 5f;
    public float dashDistance = 5f;

    [HideInInspector] public float turnSmoothVelocity;
    [HideInInspector] public float velocity;
    [HideInInspector] public bool grounded, previouslyGrounded;

    private int _velocity = Animator.StringToHash("velocity");
    private int _grounded = Animator.StringToHash("grounded");

    [SerializeField] private UnityEvent onLeaveGround;
    [SerializeField] private UnityEvent onReachGround;

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
        controller.stepOffset = defaultStepOffset;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public virtual void Update()
    {
        GroundCheck();
        MoveControls();

        switch(playerState)
        {
            case PlayerState.Normal:
                animator.applyRootMotion = true;
                break;
            case PlayerState.Airborne:
                animator.applyRootMotion = false;
                controller.Move(moveDirection * moveSpeed * Time.deltaTime);
                break;
        }
    }

    private void OnAnimatorMove()
    {
        moveDirection = animator.deltaPosition;
        controller.Move(moveDirection);
    }

    public virtual void MoveControls()
    {
        velocity = inputHandler.MoveInput().normalized.sqrMagnitude;

        if (velocity >= 0.01f)
        {
            float targetAngle = Mathf.Atan2(inputHandler.MoveInput().x, inputHandler.MoveInput().y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothDampMultiplier);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        animator.SetFloat(_velocity, velocity, 0.1f, Time.deltaTime);
    }

    public virtual void GroundCheck()
    {
        grounded = Physics.CheckSphere(transform.position, 0.2f, groundMask);

        if (grounded)
        {
            //playerVelocity.y += -2f * Time.deltaTime;
            controller.stepOffset = defaultStepOffset;
        }
        else
        {
            controller.stepOffset = 0f;
        }

        playerVelocity.y += gravity * Time.deltaTime;

        controller.Move(playerVelocity * Time.deltaTime);
        animator.SetBool(_grounded, grounded);

        if(!grounded && previouslyGrounded)
        {
            playerState = PlayerState.Airborne;
            onLeaveGround.Invoke();
        }

        if (grounded && !previouslyGrounded)
        {
            playerState = PlayerState.Normal;
            onReachGround.Invoke();
        }
        previouslyGrounded = grounded;
    }
    public virtual void Jump()
    {
        if (playerState != PlayerState.Normal) return;
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        animator.Play("Jump Start");
        playerState = PlayerState.Airborne;
    }
}
