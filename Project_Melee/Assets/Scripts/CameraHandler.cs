using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] private PlayerStateHandler playerStateHandler;

    [SerializeField] private GameObject freeLookCam;
    [SerializeField] private GameObject wallrunCamera;

    private void OnEnable()
    {
        playerStateHandler.onStateChange += OnStateChange;
    }
    private void OnDisable()
    {
        playerStateHandler.onStateChange -= OnStateChange;
    }
    private void OnStateChange()
    {
        switch(playerStateHandler.PlayerState)
        {
            case PlayerState.Normal:
            case PlayerState.Airborne:
            case PlayerState.Climbing:
                freeLookCam.SetActive(true);
                wallrunCamera.SetActive(false);
                break;

            case PlayerState.Wallrunning:
                freeLookCam.SetActive(false);
                wallrunCamera.SetActive(true);
                break;
        }
    }
}
