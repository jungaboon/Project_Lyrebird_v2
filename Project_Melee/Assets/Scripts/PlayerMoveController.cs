using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveController : MonoBehaviour
{
    public enum PlayerState
    {
        Normal,
        Airborne,
        Attacking,
        Hurt
    }
    public PlayerState playerState;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Camera mainCam;
    [HideInInspector] public Transform cam;
    [HideInInspector] public Animator animator;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private PlayerInputHandler inputHandler;

    private Vector3 moveDirection;
    private Vector3 playerVelocity;

    private bool canDoubleJump;
    [SerializeField] private bool faceCameraDirection;
    [SerializeField] private float groundcheckRadius = 0.2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 0.2f;
    [SerializeField] private float smoothDampMultiplier = 0.2f;
    [SerializeField] private float defaultStepOffset = 0.3f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float dashDistance = 5f;

    [HideInInspector] public float turnSmoothVelocity;
    [HideInInspector] public float velocity;
    [HideInInspector] public bool grounded, previouslyGrounded;

    private int _velocity = Animator.StringToHash("velocity");
    private int _grounded = Animator.StringToHash("grounded");

    [SerializeField] private UnityEvent onLeaveGround;
    [SerializeField] private UnityEvent onReachGround;
    [SerializeField] private UnityEvent onDoubleJump;

    [SerializeField] private Vector3 prevHitDirection;
    public float prevYPosition;

    private void OnEnable()
    {
        inputHandler.onJump += Jump;
    }
    private void OnDisable()
    {
        inputHandler.onJump -= Jump;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, groundcheckRadius);
    }

    public void SetPlayerState(int stateIndex)
    {
        playerState = (PlayerState)stateIndex;
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

    private void LateUpdate()
    {
        MoveOnStuck();
    }

    private void OnAnimatorMove()
    {
        moveDirection = animator.deltaPosition;
        controller.Move(moveDirection);
    }

    private void MoveOnStuck()
    {
        if (prevHitDirection == Vector3.zero) return;
        switch(playerState)
        {
            case PlayerState.Airborne:
                if(Mathf.Abs(transform.position.y - prevYPosition) < 0.1f)
                {
                    moveDirection += prevHitDirection;
                }
                break;
        }

        prevYPosition = transform.position.y;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch(playerState)
        {
            case PlayerState.Airborne:
                prevHitDirection = hit.normal;
                prevHitDirection.y = 0f;
                break;
        }
    }

    public virtual void MoveControls()
    {
        velocity = inputHandler.MoveInput().sqrMagnitude;

        if (velocity >= 0.01f)
        {
            float targetAngle = Mathf.Atan2(inputHandler.MoveInput().x, inputHandler.MoveInput().y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothDampMultiplier);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        animator.SetFloat(_velocity, velocity, 0.07f, Time.deltaTime);
    }

    public virtual void GroundCheck()
    {
        grounded = Physics.CheckSphere(transform.position, groundcheckRadius, groundMask);

        switch (playerState)
        {
            case PlayerState.Normal:
                if(playerVelocity.y < 0f) playerVelocity.y = -5f;
                break;
            case PlayerState.Airborne:
                playerVelocity.y -= gravity * Time.deltaTime;
                break;
        }


        controller.Move(playerVelocity * Time.deltaTime);
        animator.SetBool(_grounded, grounded);

        if(!grounded && previouslyGrounded)
        {
            // Need to make this execute BEFORE the Jump function
            playerState = PlayerState.Airborne;
            canDoubleJump = true;
            onLeaveGround.Invoke();
        }

        if (grounded && !previouslyGrounded)
        {
            playerState = PlayerState.Normal;
            onReachGround.Invoke();
            prevHitDirection = Vector3.zero;
        }
        previouslyGrounded = grounded;
    }
    public virtual void Jump()
    {
        switch(playerState)
        {
            case PlayerState.Normal:
                playerVelocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
                animator.Play("Jump Start");
                playerState = PlayerState.Airborne;
                canDoubleJump = true;
                break;
            case PlayerState.Airborne:
                if (!canDoubleJump) return;
                playerVelocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
                animator.Play("Double Jump Start");
                playerState = PlayerState.Airborne;
                canDoubleJump = false;
                onDoubleJump.Invoke();
                break;
        }

    }
}
