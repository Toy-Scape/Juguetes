public class MovementConfig
{
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float swimSpeed;
    public float diveSpeed;
    public float rotationSpeed;

    public MovementConfig(float walkSpeed, float sprintSpeed, float crouchSpeed,
                          float swimSpeed, float diveSpeed, float rotationSpeed = 720f)
    {
        this.walkSpeed = walkSpeed;
        this.sprintSpeed = sprintSpeed;
        this.crouchSpeed = crouchSpeed;
        this.swimSpeed = swimSpeed;
        this.diveSpeed = diveSpeed;
        this.rotationSpeed = rotationSpeed;
    }
}
