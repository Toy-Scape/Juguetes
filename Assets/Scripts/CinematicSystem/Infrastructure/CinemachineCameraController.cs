using CinematicSystem.Core;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace CinematicSystem.Infrastructure
{
    [RequireComponent(typeof(ISceneReferenceResolver))]
    public class CinemachineCameraController : MonoBehaviour, ICameraController
    {
        [SerializeField] private CinemachineCamera cinematicCamera;

        private ISceneReferenceResolver _resolver;
        private CinemachineBrain _brain;

        private Transform _initialFollow;
        private Transform _initialLookAt;

        private GameObject _pivot;
        private GameObject _cameraHandle;

        private bool _isOrbiting;
        private float _orbitSpeed;

        private Tween _moveTween;

        // Bandera para identificar si es el primer shot de la cinemática.
        // Se activa en ActivateCinematicCamera y se consume en el primer SetShot.
        private bool _isFirstShot;

        private void Awake()
        {
            _resolver = GetComponent<ISceneReferenceResolver>();
            _brain = FindFirstObjectByType<CinemachineBrain>();

            _pivot = new GameObject("Cinematic_Pivot");
            _cameraHandle = new GameObject("Cinematic_Handle");
            _cameraHandle.transform.SetParent(_pivot.transform);

            if (cinematicCamera != null)
                cinematicCamera.Priority = 0;
        }

        private void Update()
        {
            if (_isOrbiting && _pivot != null)
            {
                _pivot.transform.Rotate(Vector3.up, _orbitSpeed * Time.unscaledDeltaTime, Space.World);
            }
        }

        public void SetActive(bool active, bool instant = false)
        {
            if (active)
                ActivateCinematicCamera();
            else
                ResetCamera(instant);
        }

        public void SetShot(
            string targetId,
            string lookAtId,
            Vector3 offset,
            float fov,
            float duration,
            bool useOrbit,
            float orbitSpeed,
            bool instant = false,
            bool ignoreCollision = false)
        {
            if (cinematicCamera == null) return;

            Transform target = _resolver.Resolve(targetId);
            if (target == null) return;

            Transform lookAt = string.IsNullOrEmpty(lookAtId)
                ? target
                : _resolver.Resolve(lookAtId);

            _isOrbiting = false;

            if (_moveTween != null && _moveTween.IsActive())
                _moveTween.Kill();

#pragma warning disable CS0618
            var collider = cinematicCamera.GetComponent<CinemachineCollider>();
            if (collider != null) collider.enabled = !ignoreCollision;
#pragma warning restore CS0618

            var deoccluder = cinematicCamera.GetComponent<CinemachineDeoccluder>();
            if (deoccluder != null) deoccluder.enabled = !ignoreCollision;

            cinematicCamera.Follow = _cameraHandle.transform;
            cinematicCamera.LookAt = lookAt;
            cinematicCamera.Lens.FieldOfView = fov;

            // El handle necesita estar desplazado del pivot para que el orbit funcione:
            // al rotar el pivot, el handle orbita alrededor de él.
            _cameraHandle.transform.localPosition = offset;

            // Resetear la rotación del pivot para que el offset local sea axis-aligned.
            _pivot.transform.rotation = Quaternion.identity;

            Vector3 finalPos = target.position;

            // Primer shot de la cinemática: hacer snap directo a la posición final.
            // Esto permite que el CinemachineBrain se encargue del blend desde la cámara de juego
            // hacia un objetivo fijo y predecible, evitando cortar a través de paredes con un movimieno
            // extra o dobles curvaturas generadas por animar el destino mientras el brain hace el blend.
            // Shots posteriores dentro de la misma cinemática heredarán la posición para simular panning.
            if (_isFirstShot)
            {
                _pivot.transform.position = finalPos;
                _isFirstShot = false;
            }

            // El pivot se mueve hacia el target; el handle (en offset local) quedará en target+offset.
            // Al orbitar, el pivot rota en target.position y el handle orbita a su alrededor.

            if (instant || duration <= 0f)
            {
                _pivot.transform.position = finalPos;
                ActivateOrbit(useOrbit, orbitSpeed);
            }
            else
            {
                _moveTween = _pivot.transform
                    .DOMove(finalPos, duration)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        ActivateOrbit(useOrbit, orbitSpeed);
                    });
            }
        }

        private void ActivateOrbit(bool useOrbit, float speed)
        {
            _isOrbiting = useOrbit;
            _orbitSpeed = speed;
        }

        public void MoveTo(string targetId, float duration, bool smooth = true)
        {
            MoveTo(targetId, duration, smooth, false, false);
        }

        public void MoveTo(string targetId, float duration, bool smooth, bool instant, bool ignoreCollision)
        {
            SetShot(
                targetId,
                "",
                new Vector3(0, 5, -10),
                60,
                duration,
                false,
                0f,
                instant,
                ignoreCollision);
        }

        public void LookAt(string targetId, float duration)
        {
            Transform target = _resolver.Resolve(targetId);
            if (target != null && cinematicCamera != null)
                cinematicCamera.LookAt = target;
        }

        public void ActivateCinematicCamera()
        {
            if (cinematicCamera == null) return;

            _initialFollow = cinematicCamera.Follow;
            _initialLookAt = cinematicCamera.LookAt;

            // Activar la bandera del primer shot para que la posición inicial se estabilice
            // y el CinemachineBrain pueda hacer su mezcla (blend) normalmente.
            if (_brain != null && _brain.OutputCamera != null)
            {
                _isFirstShot = true;
            }

            cinematicCamera.Priority = 100;
        }

        public void ResetCamera(bool instant = false)
        {
            if (cinematicCamera == null) return;

            if (_moveTween != null && _moveTween.IsActive())
                _moveTween.Kill();

            _isOrbiting = false;

            cinematicCamera.Priority = 0;
            cinematicCamera.Follow = _initialFollow;
            cinematicCamera.LookAt = _initialLookAt;
        }

        public System.Collections.IEnumerator WaitForBlend()
        {
            if (_brain == null) yield break;

            yield return null;
            yield return null;

            while (_brain.IsBlending)
                yield return null;
        }
    }
}