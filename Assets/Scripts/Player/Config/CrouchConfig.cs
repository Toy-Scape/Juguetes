public class CrouchConfig
{
    public float standingHeight;
    public float crouchingHeight;
    public float transitionSpeed;

    public CrouchConfig(float standingHeight, float crouchingHeight, float transitionSpeed = 10f)
    {
        this.standingHeight = standingHeight;
        this.crouchingHeight = crouchingHeight;
        this.transitionSpeed = transitionSpeed;
    }
}
