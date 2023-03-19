using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallrunController : MonoBehaviour
{
    [SerializeField] private PlayerStateHandler playerStateHandler;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [Space]
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private float minWallrunHeight = 2f;
    [SerializeField] private float wallRunSpeed = 7f;
    [SerializeField] private LayerMask wallrunMask;
    [SerializeField] private LayerMask groundMask;
    [Space]
    [SerializeField] private bool wallLeft;
    [SerializeField] private bool wallRight;
    [SerializeField] private bool atMinHeight;

    private Vector3 currentWallNormal;
    private Vector3 currentForwardDir;

    private int _wallLeft = Animator.StringToHash("wallLeft");
    private int _wallRight = Animator.StringToHash("wallRight");

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + transform.up, -transform.right * raycastDistance);
        Gizmos.DrawRay(transform.position + transform.up, transform.right * raycastDistance);
    }

    private void Update()
    {
        switch (playerStateHandler.PlayerState)
        {
            case PlayerState.Airborne:
                wallLeft = Physics.Raycast(transform.position + transform.up, -transform.right, raycastDistance, wallrunMask);
                wallRight = Physics.Raycast(transform.position + transform.up, transform.right, raycastDistance, wallrunMask);
                atMinHeight = !Physics.Raycast(transform.position + transform.up * 0.1f, Vector3.down, minWallrunHeight, groundMask);
                if (!atMinHeight) break;

                if (wallLeft && !wallRight)
                {
                    playerStateHandler.SetPlayerState(PlayerState.Wallrunning);
                }
                else if (!wallLeft && wallRight)
                {
                    playerStateHandler.SetPlayerState(PlayerState.Wallrunning);
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
                    currentWallNormal = rayHitLeft.normal;
                    currentForwardDir = Vector3.Cross(currentWallNormal, Vector3.up);
                }
                else if (!wallLeft && wallRight)
                {
                    currentWallNormal = rayHitRight.normal;
                    currentForwardDir = -Vector3.Cross(currentWallNormal, Vector3.up);
                }
                else
                {
                    playerStateHandler.SetPlayerState(PlayerState.Airborne);
                }

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentForwardDir), 10f * Time.deltaTime);
                characterController.Move(transform.forward * moveInput.y * wallRunSpeed * Time.deltaTime);

                break;
        }

        animator.SetBool(_wallLeft, wallLeft);
        animator.SetBool(_wallRight, wallRight);
    }
}
