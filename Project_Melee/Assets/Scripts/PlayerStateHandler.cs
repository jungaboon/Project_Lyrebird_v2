using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Normal,
    Airborne,
    Climbing,
    Wallrunning
}

public class PlayerStateHandler : MonoBehaviour
{
    public static PlayerStateHandler Instance;
    [field:SerializeField]
    public PlayerState PlayerState { get; private set; }
    private PlayerState prevPlayerState;

    public delegate void OnStateChange();
    public OnStateChange onStateChange;

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayerState(PlayerState state)
    {
        prevPlayerState = PlayerState;
        PlayerState = state;

        if(PlayerState != prevPlayerState)
        {
            if (onStateChange != null) onStateChange();
        }
    }


}
