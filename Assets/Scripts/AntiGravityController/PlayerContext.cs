using UnityEngine;

namespace Assets.Scripts.AntiGravityController
{
    public class PlayerContext
    {
        // Input
        public Vector2 MoveInput { get; set; }
        public Vector2 LookInput { get; set; }
        public bool IsJumping { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }
        public bool IsGrabbing { get; set; } // For manual grab if needed

        // State
        public Vector3 Velocity { get; set; }
        public bool IsGrounded { get; set; }
        public bool IsHanging;
        public bool IsGrabbingLedge { get { return IsHanging; } set { IsHanging = value; } } // Alias for IsHanging
        public bool IsClimbing;
        public bool IsPicking;  // Carrying
        public bool IsPushing;  // (Legacy/Same as Grabbing)
        public float PushSpeedMultiplier = 1f;

        public Transform GrabTarget; // The object being pushed/pulled
    }
}
