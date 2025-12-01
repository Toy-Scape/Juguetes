using UnityEngine;

public class WallDetectionHandler
{
    private Transform playerTransform;

    public WallDetectionHandler (Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }

    public bool CheckForWall (PlayerContext context, float detectionDistance = 1f)
    {
        if (Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit hit, detectionDistance))
        {
            WalkableWallSurface wall = hit.collider.GetComponent<WalkableWallSurface>();
            if (wall != null)
            {
                context.IsOnWall = true;
                context.WallNormal = hit.normal;
                return true;
            }
        }

        context.IsOnWall = false;
        return false;
    }
}
