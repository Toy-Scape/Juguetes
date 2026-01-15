using UnityEngine;

public class PlayerContext
{
    public Vector2 MoveInput { get; set; }
    public Vector2 LookInput { get; set; }
    public bool IsSprinting { get; set; }
    public bool IsJumping { get; set; }
    public bool IsCrouching { get; set; }

    public bool IsGrabbing { get; set; }
    public bool IsPicking { get; set; }
    public bool IsPushing { get; set; }
    public Transform GrabTarget { get; set; }
    public float PushSpeedMultiplier { get; set; } = 1f;

    public Vector3 Velocity { get; set; }
    public bool IsGrounded { get; set; }
    public bool IsInWater { get; set; }

    public bool IsHanging { get; set; }
    public bool IsGrabbingLedge { get; set; }
    public bool IsClimbing { get; set; }

    public bool IsInteracting { get; set; }
    public bool NextLimb { get; set; }
    public bool PreviousLimb { get; set; }

    public bool IsAttacking { get; set; }
    public bool IsAiming { get; set; }

    public bool CanWalkOnWalls { get; set; }
    public bool IsOnWall { get; set; }
    public Vector3 WallNormal { get; set; }
}
