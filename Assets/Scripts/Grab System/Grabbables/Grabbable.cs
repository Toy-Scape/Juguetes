using InteractionSystem.Interfaces;
using UnityEngine;
using System.Linq;

public class Grabbable : MonoBehaviour, IGrabbable
{
    [SerializeField] private float moveResistance = 1f;
    [SerializeField] private GrabConditionSO[] grabConditions;

    private Rigidbody rb;
    private ConfigurableJoint joint;

    public float MoveResistance => moveResistance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public bool CanBeGrabbed()
    {
        if (grabConditions == null || grabConditions.Length == 0) return true;
        return grabConditions.All(c => c != null && c.CanGrab());
    }

    public void StartGrab(Rigidbody grabAnchorRb, Vector3 grabPoint)
    {
        if (!CanBeGrabbed()) return;

        rb.isKinematic = false;
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

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        float resistance = Mathf.Clamp(moveResistance, 0.1f, 10f);

        JointDrive drive = new JointDrive
        {
            positionSpring = 300f / resistance,
            positionDamper = 50f / resistance,
            maximumForce = 2000f / resistance
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
        rb.isKinematic = true;
        if (joint != null)
            Destroy(joint);
    }
}
