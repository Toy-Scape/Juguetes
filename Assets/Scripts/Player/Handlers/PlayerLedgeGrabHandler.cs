using UnityEngine;

public class PlayerLedgeGrabHandler
{
    private readonly PlayerConfig config;
    private readonly Transform playerTransform;
    private Vector3 ledgePosition;
    private Vector3 ledgeNormal;

    private bool isClimbing = false;
    private int climbPhase = 0;
    private Vector3 climbTarget;

    public bool IsClimbing => isClimbing;

    public PlayerLedgeGrabHandler(PlayerConfig config, Transform playerTransform)
    {
        this.config = config;
        this.playerTransform = playerTransform;
    }

    public bool CheckForLedge(Vector3 velocity, bool isGrounded)
    {
        if (isGrounded || velocity.y >= 0) return false;

        Vector3 origin = playerTransform.position + Vector3.up * config.ledgeGrabHeight;

        if (Physics.Raycast(origin, playerTransform.forward, out RaycastHit hit, config.ledgeDetectionDistance))
        {
            if (hit.collider.GetComponent<LedgeGrabSurface>() != null)
            {
                ledgePosition = hit.point;
                ledgeNormal = hit.normal;
                return true;
            }
        }
        return false;
    }

    public void SnapToLedge()
    {
        Vector3 targetPos = new Vector3(
            ledgePosition.x,
            ledgePosition.y - config.ledgeSnapOffsetY,
            ledgePosition.z
        );
        playerTransform.position = targetPos;

        Quaternion targetRot = Quaternion.LookRotation(-ledgeNormal, Vector3.up);
        playerTransform.rotation = targetRot;
    }

    public void StartClimb()
    {
        climbTarget = new Vector3(
            playerTransform.position.x,
            ledgePosition.y + config.ledgeSnapOffsetY,
            playerTransform.position.z
        );

        isClimbing = true;
        climbPhase = 1;
    }

    public bool UpdateClimb()
    {
        if (!isClimbing) return false;

        if (climbPhase == 1)
        {
            playerTransform.position = Vector3.MoveTowards(
                playerTransform.position,
                climbTarget,
                config.ledgeClimbUpSpeed * Time.deltaTime
            );

            if (Vector3.Distance(playerTransform.position, climbTarget) < 0.01f)
            {
                climbTarget = climbTarget + playerTransform.forward * config.ledgeForwardOffset;
                climbPhase = 2;
            }
        }
        else if (climbPhase == 2)
        {
            playerTransform.position = Vector3.MoveTowards(
                playerTransform.position,
                climbTarget,
                config.ledgeClimbUpSpeed * Time.deltaTime
            );

            if (Vector3.Distance(playerTransform.position, climbTarget) < 0.01f)
            {
                isClimbing = false;
                climbPhase = 0;
                return true; 
            }
        }

        return false; 
    }
}
