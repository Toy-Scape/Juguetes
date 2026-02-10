public class JumpConfig
{
    public float jumpHeight;
    public float gravity;
    public float swimSpeed;
    public float diveSpeed;

    public float coyoteTime;
    public float jumpBufferTime;

    public JumpConfig(float jumpHeight, float gravity, float swimSpeed, float diveSpeed, float coyoteTime, float jumpBufferTime)
    {
        this.jumpHeight = jumpHeight;
        this.gravity = gravity;
        this.swimSpeed = swimSpeed;
        this.diveSpeed = diveSpeed;
        this.coyoteTime = coyoteTime;
        this.jumpBufferTime = jumpBufferTime;
    }
}
