using InteractionSystem.Interfaces;
using UnityEngine;

public class Grabbable : MonoBehaviour, IGrabbable
{
    [SerializeField] private float moveResistance = 1f;

    private Rigidbody rb;
    private ConfigurableJoint joint;

    public float MoveResistance => moveResistance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void StartGrab(Rigidbody grabAnchorRb, Vector3 grabPoint)
    {
        if (joint != null)
            Destroy(joint);

        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = grabAnchorRb;

        joint.autoConfigureConnectedAnchor = false;

        Vector3 localAnchor = transform.InverseTransformPoint(grabPoint);
        joint.anchor = localAnchor;
        joint.connectedAnchor = Vector3.zero;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        JointDrive drive = new JointDrive
        {
            positionSpring = 800 / moveResistance,
            positionDamper = 40,
            maximumForce = 10000
        };

        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;

        JointDrive angDrive = new JointDrive
        {
            positionSpring = 200,
            positionDamper = 20,
            maximumForce = 5000
        };

        joint.angularXDrive = angDrive;
        joint.angularYZDrive = angDrive;
    }

    public void StopGrab()
    {
        if (joint != null)
            Destroy(joint);
    }
}
