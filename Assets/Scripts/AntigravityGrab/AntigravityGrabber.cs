using UnityEngine;

namespace AntigravityGrab
{
    public class AntigravityGrabber : MonoBehaviour
    {
        [SerializeField] private float grabRadius = 1.0f;
        [SerializeField] private Transform grabOrigin;
        [SerializeField] private LayerMask grabLayer;
        [SerializeField] private Transform holdPoint;

        private AntigravityGrabbable currentGrabbable;
        private AntigravityPickable currentPickable;
        private bool isGrabbing;
        private bool isPicking;
        private Vector3 grabOffset;
        private Quaternion grabRotationOffset;

        private CharacterController characterController;
        private Collider[] playerColliders;

        private void Awake ()
        {
            characterController = GetComponent<CharacterController>();
            playerColliders = GetComponentsInChildren<Collider>();
            if (grabOrigin == null) grabOrigin = transform;
        }

        public bool IsGrabbing => isGrabbing;
        public bool IsPicking => isPicking;
        public float CurrentResistance => currentGrabbable != null ? currentGrabbable.MoveResistance : 0f;
        public Transform GrabbedObjectTransform => currentGrabbable != null ? currentGrabbable.transform : null;

        public bool TryPick()
        {
            if (isPicking || isGrabbing) return false;

            Collider[] hits = Physics.OverlapSphere(grabOrigin.position, grabRadius, grabLayer);
            float closestDist = float.MaxValue;
            AntigravityPickable bestTarget = null;

            foreach (var hit in hits)
            {
                var pickable = hit.GetComponentInParent<AntigravityPickable>();
                if (pickable != null)
                {
                    Vector3 closestPoint = hit.ClosestPoint(grabOrigin.position);
                    float d = Vector3.SqrMagnitude(closestPoint - grabOrigin.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        bestTarget = pickable;
                    }
                }
            }

            if (bestTarget != null && bestTarget.CanBePicked())
            {
                StartPick(bestTarget);
                return true;
            }

            return false;
        }

        public void DropPicked()
        {
            if (!isPicking) return;

            if (currentPickable != null)
            {
                var pickedColliders = currentPickable.GetComponentsInChildren<Collider>();
                foreach (var pc in playerColliders)
                    foreach (var oc in pickedColliders)
                        Physics.IgnoreCollision(pc, oc, false);

                if (characterController != null)
                    foreach (var oc in pickedColliders)
                        Physics.IgnoreCollision(characterController, oc, false);

                currentPickable.Drop();
            }

            currentPickable = null;
            isPicking = false;
        }

        private void StartPick(AntigravityPickable pickable)
        {
            currentPickable = pickable;
            isPicking = true;

            currentPickable.Pick(holdPoint);

            var pickedColliders = currentPickable.GetComponentsInChildren<Collider>();
            foreach (var pc in playerColliders)
                foreach (var oc in pickedColliders)
                    Physics.IgnoreCollision(pc, oc, true);

            if (characterController != null)
                foreach (var oc in pickedColliders)
                    Physics.IgnoreCollision(characterController, oc, true);
        }

        public bool TryGrab ()
        {
            if (isPicking) return false;
            if (isGrabbing) return false;

            Collider[] hits = Physics.OverlapSphere(grabOrigin.position, grabRadius, grabLayer);
            float closestDist = float.MaxValue;
            AntigravityGrabbable bestTarget = null;
            Vector3 bestPoint = Vector3.zero;

            foreach (var hit in hits)
            {
                var grabbable = hit.GetComponentInParent<AntigravityGrabbable>();
                if (grabbable != null)
                {
                    Vector3 closestPoint = hit.ClosestPoint(grabOrigin.position);
                    float d = Vector3.SqrMagnitude(closestPoint - grabOrigin.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        bestTarget = grabbable;
                        bestPoint = closestPoint;
                    }
                }
            }

            if (bestTarget != null)
            {
                if (bestTarget.CanBeGrabbed())
                {
                    StartGrab(bestTarget, bestPoint);
                    return true;
                }
                else
                {
                    var failThought = bestTarget.FailThought;
                    if (failThought != null && DialogueBox.Instance != null && !DialogueBox.Instance.IsOpen)
                    {
                        DialogueBox.Instance.StartDialogue(failThought);
                    }
                }
            }

            return false;
        }

        public void ReleaseGrab ()
        {
            if (!isGrabbing) return;

            if (currentGrabbable != null)
            {
                foreach (var pc in playerColliders)
                    currentGrabbable.IgnoreCollisionWith(pc, false);

                if (characterController != null)
                    currentGrabbable.IgnoreCollisionWith(characterController, false);

                currentGrabbable.StopGrab();
            }

            currentGrabbable = null;
            isGrabbing = false;
        }

        private void StartGrab (AntigravityGrabbable grabbable, Vector3 hitPoint)
        {
            currentGrabbable = grabbable;
            isGrabbing = true;

            Vector3 direction = hitPoint - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction);

            grabOffset = transform.InverseTransformPoint(currentGrabbable.transform.position);
            grabRotationOffset = Quaternion.Inverse(transform.rotation) * currentGrabbable.transform.rotation;

            foreach (var pc in playerColliders)
                currentGrabbable.IgnoreCollisionWith(pc, true);

            if (characterController != null)
                currentGrabbable.IgnoreCollisionWith(characterController, true);

            currentGrabbable.StartGrab();
        }

        public bool CheckMove (Vector3 movement)
        {
            if (!isGrabbing || currentGrabbable == null)
                return true;

            // Si el objeto no puede moverse, el jugador tampoco
            if (!currentGrabbable.ValidateMovement(movement))
                return false;

            // Además, validar la configuración final (rotación + movimiento)
            Vector3 targetPos = transform.TransformPoint(grabOffset) + movement;
            Quaternion targetRot = transform.rotation * grabRotationOffset;

            if (currentGrabbable.CheckCollision(targetPos, targetRot))
                return false;

            return true;
        }


        public void ValidateRotation (float proposedAngle, Vector3 playerPos, out float allowedAngle, out Vector3 effectivePivot)
        {
            if (!isGrabbing || currentGrabbable == null)
            {
                allowedAngle = proposedAngle;
                effectivePivot = Vector3.zero;
                return;
            }

            currentGrabbable.ValidateRotation(proposedAngle, playerPos, out allowedAngle, out effectivePivot);
        }

        public bool IsConfigurationValid (Vector3 playerPosition, Quaternion playerRotation)
        {
            if (!isGrabbing || currentGrabbable == null) return true;

            Vector3 targetPos = playerPosition + (playerRotation * grabOffset);
            Quaternion targetRot = playerRotation * grabRotationOffset;

            return !currentGrabbable.CheckCollision(targetPos, targetRot);
        }

        public void UpdateObjectPosition ()
        {
            if (!isGrabbing || currentGrabbable == null) return;

            Vector3 targetPos = transform.TransformPoint(grabOffset);
            Quaternion targetRot = transform.rotation * grabRotationOffset;

            if (currentGrabbable.CheckCollision(targetPos, targetRot))
                return; // No mover ni rotar

            currentGrabbable.MoveTo(targetPos, targetRot);
        }

        private void OnDrawGizmosSelected ()
        {
            if (grabOrigin != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(grabOrigin.position, grabRadius);
            }
        }
    }
}
