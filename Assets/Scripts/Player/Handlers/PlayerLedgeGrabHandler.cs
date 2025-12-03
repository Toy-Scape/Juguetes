using UnityEngine;

public class PlayerLedgeGrabHandler
{
    private readonly PlayerConfig config;
    private readonly Transform playerTransform;
    private Vector3 ledgePosition;
    private Vector3 ledgeNormal;

    private bool isSnapping = false;
    private bool isClimbing = false;
    private int climbPhase = 0;
    private Vector3 snapTargetPosition;
    private Quaternion snapTargetRotation;
    private Vector3 climbTarget;

    public bool IsClimbing => isClimbing;
    public bool IsSnapping => isSnapping;

    public PlayerLedgeGrabHandler (PlayerConfig config, Transform playerTransform)
    {
        this.config = config;
        this.playerTransform = playerTransform;
    }

    public bool CheckForLedge (Vector3 velocity, bool isGrounded)
    {
        // Don't grab if grounded or moving upward
        if (isGrounded || velocity.y >= 0) return false;

        // Reduced threshold to detect earlier during fall
        if (velocity.y > -0.3f) return false;

        Vector3 origin = playerTransform.position + Vector3.up * config.LedgeGrabHeight;

        // Primary raycast at chest height
        if (Physics.Raycast(origin, playerTransform.forward, out RaycastHit hit, config.LedgeDetectionDistance))
        {
            if (hit.collider.GetComponent<LedgeGrabSurface>() != null)
            {
                // Additional check: ensure the ledge is roughly at chest height
                float heightDifference = hit.point.y - playerTransform.position.y;
                if (heightDifference < 0.5f || heightDifference > 2.5f)
                {
                    return false;
                }

                // CRITICAL: Verify this is actually a top edge, not middle of a wall
                if (IsValidLedgeEdge(hit.point, hit.normal))
                {
                    ledgePosition = hit.point;
                    ledgeNormal = hit.normal;
                    return true;
                }
            }
        }

        // Predictive raycast: check slightly ahead and below to catch ledges early
        Vector3 predictiveOrigin = origin + playerTransform.forward * 0.2f + Vector3.down * 0.3f;
        if (Physics.Raycast(predictiveOrigin, playerTransform.forward, out RaycastHit predictiveHit, config.LedgeDetectionDistance * 0.8f))
        {
            if (predictiveHit.collider.GetComponent<LedgeGrabSurface>() != null)
            {
                float heightDifference = predictiveHit.point.y - playerTransform.position.y;
                if (heightDifference >= 0.3f && heightDifference <= 2.5f)
                {
                    // CRITICAL: Verify this is actually a top edge
                    if (IsValidLedgeEdge(predictiveHit.point, predictiveHit.normal))
                    {
                        ledgePosition = predictiveHit.point;
                        ledgeNormal = predictiveHit.normal;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsValidLedgeEdge(Vector3 hitPoint, Vector3 hitNormal)
    {
        // Check if there's empty space above the hit point (confirming it's a top edge)
        Vector3 checkAbove = hitPoint + Vector3.up * 0.3f - hitNormal * 0.1f;
        if (Physics.Raycast(checkAbove, Vector3.up, 0.5f))
        {
            // There's something above, this is not a top edge
            return false;
        }

        // Check if there's a surface on top (walkable ledge)
        Vector3 checkTop = hitPoint + Vector3.up * 0.1f - hitNormal * 0.2f;
        if (Physics.Raycast(checkTop, Vector3.down, out RaycastHit topHit, 0.3f))
        {
            // Verify the surface is roughly horizontal (walkable)
            if (Vector3.Dot(topHit.normal, Vector3.up) > 0.7f)
            {
                return true;
            }
        }

        return false;
    }

    public void StartSnap ()
    {
        snapTargetPosition = new Vector3(
            ledgePosition.x,
            ledgePosition.y - config.LedgeSnapOffsetY,
            ledgePosition.z
        );
        snapTargetRotation = Quaternion.LookRotation(-ledgeNormal, Vector3.up);
        isSnapping = true;
        isClimbing = false;
        climbPhase = 0;
    }

    public void StartClimb ()
    {
        climbTarget = new Vector3(
            playerTransform.position.x,
            ledgePosition.y + config.LedgeSnapOffsetY,
            playerTransform.position.z
        );

        isSnapping = false;
        isClimbing = true;
        climbPhase = 1;
    }

    public bool Update ()
    {
        // Phase 1: Snapping to ledge
        if (isSnapping)
        {
            playerTransform.position = Vector3.Lerp(
                playerTransform.position,
                snapTargetPosition,
                config.LedgeSnapSpeed * Time.deltaTime
            );

            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                snapTargetRotation,
                config.LedgeSnapSpeed * Time.deltaTime
            );

            // Check if snap is complete
            if (Vector3.Distance(playerTransform.position, snapTargetPosition) < 0.01f &&
                Quaternion.Angle(playerTransform.rotation, snapTargetRotation) < 1f)
            {
                isSnapping = false;
            }
            return false;
        }

        // Phase 2: Climbing
        if (isClimbing)
        {
            if (climbPhase == 1)
            {
                playerTransform.position = Vector3.Lerp(
                    playerTransform.position,
                    climbTarget,
                    config.LedgeClimbUpSpeed * Time.deltaTime
                );

                if (Vector3.Distance(playerTransform.position, climbTarget) < 0.01f)
                {
                    climbTarget = climbTarget + playerTransform.forward * config.LedgeForwardOffset;
                    climbPhase = 2;
                }
            }
            else if (climbPhase == 2)
            {
                playerTransform.position = Vector3.Lerp(
                    playerTransform.position,
                    climbTarget,
                    config.LedgeClimbUpSpeed * Time.deltaTime
                );

                if (Vector3.Distance(playerTransform.position, climbTarget) < 0.01f)
                {
                    isClimbing = false;
                    climbPhase = 0;
                    return true;
                }
            }
        }

        return false;
    }
}
