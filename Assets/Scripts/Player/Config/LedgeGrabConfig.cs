public class LedgeGrabConfig
{
    public float detectionDistance;
    public float ledgeGrabHeight;
    public float climbUpSpeed;

    public LedgeGrabConfig(float detectionDistance = 0.5f, float ledgeGrabHeight = 1.5f, float climbUpSpeed = 5f)
    {
        this.detectionDistance = detectionDistance;
        this.ledgeGrabHeight = ledgeGrabHeight;
        this.climbUpSpeed = climbUpSpeed;
    }
}
