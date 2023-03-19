using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using MoreMountains.Feedbacks;
using FIMSpace;
using DG.Tweening;

public class PlayerWallrunController : MonoBehaviour
{
    [SerializeField] private PlayerStateHandler playerStateHandler;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMoveController playerMoveController;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [Space]
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private float minWallrunHeight = 2f;
    [SerializeField] private float wallOffset = 0.75f;
    [SerializeField] private float wallRunSpeed = 7f;
    [SerializeField] private float wallJumpSpeed = 5f;
    [SerializeField] private LayerMask wallrunMask;
    [SerializeField] private LayerMask groundMask;
    [Space]
    [SerializeField] private bool wallLeft;
    [SerializeField] private bool wallRight;
    [SerializeField] private bool atMinHeight;
    [Space]
    [SerializeField] private Rig[] armIKRigs;
    [SerializeField] private Transform[] armIKTargets;
    [Space]
    [SerializeField] private MMFeedbacks wallrunFeedback;

    private Vector3 wallOffsetPos;
    private Vector3 currentWallHit;
    private Vector3 currentWallNormal;
    private Vector3 currentForwardDir;

    private int _wallLeft = Animator.StringToHash("wallLeft");
    private int _wallRight = Animator.StringToHash("wallRight");

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + transform.up, -transform.right * raycastDistance);
        Gizmos.DrawRay(transform.position + transform.up, transform.right * raycastDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(wallOffsetPos, 0.2f);
    }
    private void OnEnable()
    {
        inputHandler.onJump += WallJump;
        playerStateHandler.onStateChange += OnStopWallrun;
    }
    private void OnDisable()
    {
        inputHandler.onJump -= WallJump;
        playerStateHandler.onStateChange += OnStopWallrun;
    }
    private void Start()
    {
        for (int i = 0; i < armIKRigs.Length; i++)
        {
            armIKRigs[i].weight = 0f;
        }
    }
    private void OnStopWallrun()
    {
        if(playerStateHandler.PlayerState != PlayerState.Wallrunning)
        {
            for (int i = 0; i < armIKRigs.Length; i++)
            {
                armIKRigs[i].weight = 0f;
            }
            wallrunFeedback.StopFeedbacks();
        }
    }
    private void WallJump()
    {
        switch(playerStateHandler.PlayerState)
        {
            case PlayerState.Wallrunning:

                Vector3 jumpDir = currentWallNormal + Vector3.up;
                playerMoveController.ResetAdditiveVelocity();
                playerMoveController.AddVelocity(jumpDir * wallJumpSpeed);
                playerMoveController.canDoubleJump = true;
                animator.Play("Double Jump Start", 0, 0f);

                Quaternion targetRot = Quaternion.LookRotation(currentWallNormal);
                transform.DORotate(targetRot.eulerAngles, 0.2f);
                playerStateHandler.SetPlayerState(PlayerState.Airborne);
                break;
        }

    }
    private void Update()
    {
        animator.SetBool(_wallLeft, false);
        animator.SetBool(_wallRight, false);
        switch (playerStateHandler.PlayerState)
        {
            case PlayerState.Airborne:
                wallLeft = Physics.Raycast(transform.position + transform.up, -transform.right, raycastDistance, wallrunMask);
                wallRight = Physics.Raycast(transform.position + transform.up, transform.right, raycastDistance, wallrunMask);
                atMinHeight = !Physics.Raycast(transform.position + transform.up * 0.1f, Vector3.down, minWallrunHeight, groundMask);
                if (!atMinHeight) break;

                if (wallLeft && !wallRight && playerMoveController.velocity > 0.1f)
                {
                    playerStateHandler.SetPlayerState(PlayerState.Wallrunning);
                    wallrunFeedback.PlayFeedbacks();
                }
                else if (!wallLeft && wallRight && playerMoveController.velocity > 0.1f)
                {
                    playerStateHandler.SetPlayerState(PlayerState.Wallrunning);
                    wallrunFeedback.PlayFeedbacks();
                }
                break;

            case PlayerState.Wallrunning:
                RaycastHit rayHitLeft;
                RaycastHit rayHitRight;
                wallLeft = Physics.Raycast(transform.position + transform.up, -transform.right, out rayHitLeft, raycastDistance, wallrunMask);
                wallRight = Physics.Raycast(transform.position + transform.up, transform.right, out rayHitRight, raycastDistance, wallrunMask);

                Vector2 moveInput = inputHandler.MoveInput();

                if (wallLeft && !wallRight)
                {
                    currentWallHit = rayHitLeft.point;
                    currentWallNormal = rayHitLeft.normal;
                    currentForwardDir = Vector3.Cross(currentWallNormal, Vector3.up);
                    wallOffsetPos = currentWallHit + currentWallNormal * wallOffset;
                    armIKRigs[0].weight = Mathf.Lerp(armIKRigs[0].weight, 1f, Time.deltaTime * 5f);
                    armIKTargets[0].position = currentWallHit + currentWallNormal * 0.1f;
                }
                else if (!wallLeft && wallRight)
                {
                    currentWallHit = rayHitRight.point;
                    currentWallNormal = rayHitRight.normal;
                    currentForwardDir = -Vector3.Cross(currentWallNormal, Vector3.up);
                    wallOffsetPos = currentWallHit + currentWallNormal * wallOffset;
                    armIKRigs[1].weight = Mathf.Lerp(armIKRigs[1].weight, 1f, Time.deltaTime * 5f);
                    armIKTargets[1].position = currentWallHit + currentWallNormal * 0.1f;
                }
                else
                {
                    playerStateHandler.SetPlayerState(PlayerState.Airborne);
                }

                wallOffsetPos.y = 0f;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentForwardDir), 10f * Time.deltaTime);
                characterController.Move(currentForwardDir * wallRunSpeed * Time.deltaTime);

                animator.SetBool(_wallLeft, wallLeft);
                animator.SetBool(_wallRight, wallRight);

                wallrunFeedback.transform.position = currentWallHit + currentWallNormal * 0.15f;
                break;
        }
    }
}
