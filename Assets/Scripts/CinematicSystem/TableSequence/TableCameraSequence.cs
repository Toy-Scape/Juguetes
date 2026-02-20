using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CinematicSystem.TableSequence
{
    public enum IdleMoveType
    {
        None,
        Vertical,
        Horizontal
    }

    [System.Serializable]
    public class TableTarget
    {
        public Transform TargetObject;

        [Header("Timing Overrides")]
        public bool OverrideTiming = false;
        public float MoveToCloseDuration = 2f;
        public float StayDuration = 2f;
        public float MoveToFarDuration = 2f;
        public float TransitionToNextDuration = 2f;

        [Header("Ease Overrides")]
        public bool OverrideEase = false;
        public Ease ApproachEase = Ease.InOutQuad;
        public Ease RetractEase = Ease.InOutQuad;
        public Ease TransitionEase = Ease.Linear;

        [Header("Idle Animation")]
        public bool OverrideIdle = false;
        public IdleMoveType IdleType = IdleMoveType.None;
        public float IdleAmount = 0.5f;
    }

    [System.Serializable]
    public struct CinematicSubtitle
    {
        [Tooltip("Key from the Localization Database")]
        public string Key;

        [TextArea] public string Text;
        public float Duration;
        public float PreDelay;
    }

    public class TableCameraSequence : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private List<TableTarget> _targets;

        [Header("Subtitle Sequence")]
        [SerializeField] private List<CinematicSubtitle> _subtitles;

        [Header("Global Settings")]
        [SerializeField] private float _closeDistance = 1f;
        [SerializeField] private float _farDistance = 2.5f;
        [SerializeField] private float _heightOffset = 1f;
        [SerializeField] private Vector3 _approachDirection = new Vector3(0, 0, -1); // From Front
        [SerializeField] private float _lookAtHeightOffset = 0f;

        [Header("Global Timing")]
        [SerializeField] private float _defaultMoveToClose = 2f;
        [SerializeField] private float _defaultStay = 2f;
        [SerializeField] private float _defaultMoveToFar = 2f;
        [SerializeField] private float _defaultTransition = 1.5f;

        [Header("Global Idle")]
        [SerializeField] private IdleMoveType _defaultIdleType = IdleMoveType.None;
        [SerializeField] private float _defaultIdleAmount = 0.2f;

        [SerializeField] private bool _loop = false;
        [SerializeField] private bool _startOnEnable = true;
        [SerializeField] private bool _stopAtLastObject = false; // New Option

        [Header("Scene Transition")]
        [SerializeField] private string _nextSceneName;
        [SerializeField] private float _transitionDuration = 1f;
        [SerializeField] private float _delayBeforeTransition = 0f;

        private Sequence _sequence;

        private void OnEnable()
        {
            if (_startOnEnable)
                PlaySequence();
        }

        private void OnDisable()
        {
            if (_sequence != null)
                _sequence.Kill();
        }

        public void PlaySequence()
        {
            if (_targets == null || _targets.Count == 0 || _camera == null) return;

            _sequence = DOTween.Sequence();

            // Setup initial position
            if (_targets.Count > 0 && _targets[0].TargetObject != null)
            {
                // Start independent subtitle sequence
                if (_subtitles != null && _subtitles.Count > 0)
                {
                    StartCoroutine(PlaySubtitleRoutine());
                }
                // Start at Close Position of first object (as requested)
                Vector3 startPos = GetClosePos(_targets[0].TargetObject);
                Vector3 startLookAt = GetLookAtPos(_targets[0].TargetObject);

                _camera.transform.position = startPos;
                _camera.transform.LookAt(startLookAt);

                // PRELOAD SCENE (Background Loading)
                if (!string.IsNullOrEmpty(_nextSceneName))
                {
                    var transitionManager = CinematicSystem.Transitions.SceneTransitionManager.Instance;
                    if (transitionManager == null)
                    {
                        GameObject go = new GameObject("SceneTransitionManager");
                        transitionManager = go.AddComponent<CinematicSystem.Transitions.SceneTransitionManager>();
                    }
                    if (transitionManager != null)
                        transitionManager.PreloadScene(_nextSceneName);
                }
            }

            for (int i = 0; i < _targets.Count; i++)
            {
                TableTarget targetData = _targets[i];
                Transform obj = targetData.TargetObject;
                if (obj == null) continue;

                int nextIndex = (i + 1) % _targets.Count;
                TableTarget nextTargetData = _targets[nextIndex];
                Transform nextObj = nextTargetData.TargetObject;

                // Durations
                float dClose = targetData.OverrideTiming ? targetData.MoveToCloseDuration : _defaultMoveToClose;
                float dStay = targetData.OverrideTiming ? targetData.StayDuration : _defaultStay;
                float dFar = targetData.OverrideTiming ? targetData.MoveToFarDuration : _defaultMoveToFar;
                float dTrans = targetData.OverrideTiming ? targetData.TransitionToNextDuration : _defaultTransition;

                // Eases
                Ease eClose = targetData.OverrideEase ? targetData.ApproachEase : Ease.InOutQuad;
                Ease eRetract = targetData.OverrideEase ? targetData.RetractEase : Ease.InOutQuad;
                Ease eTrans = targetData.OverrideEase ? targetData.TransitionEase : Ease.InOutQuad;

                // Calculated Positions
                Vector3 closePos = GetClosePos(obj);
                Vector3 farPos = GetFarPos(obj);
                Vector3 lookAtPos = GetLookAtPos(obj);

                // Idle Settings
                IdleMoveType iType = targetData.OverrideIdle ? targetData.IdleType : _defaultIdleType;
                float iAmount = targetData.OverrideIdle ? targetData.IdleAmount : _defaultIdleAmount;

                // Force First Object to have NO Idle (as requested)
                if (i == 0) iType = IdleMoveType.None;

                // Calculate Idle Start/End Offsets
                Vector3 idleOffset = Vector3.zero;
                if (iType != IdleMoveType.None)
                {
                    Vector3 forward = (lookAtPos - closePos).normalized;
                    Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                    Vector3 up = Vector3.Cross(forward, right).normalized;

                    if (iType == IdleMoveType.Horizontal) idleOffset = right * iAmount;
                    else if (iType == IdleMoveType.Vertical) idleOffset = up * iAmount;
                }

                Vector3 approachEndPos = closePos - (idleOffset * 0.5f);
                Vector3 stayEndPos = closePos + (idleOffset * 0.5f);

                // 1. Approach (Far -> Close Start)
                // Skip approach for the FIRST item, as we start there.
                if (i != 0)
                {
                    _sequence.Append(_camera.transform.DOMove(approachEndPos, dClose).SetEase(eClose));
                    _sequence.Join(_camera.transform.DOLookAt(lookAtPos, dClose).SetEase(eClose));
                }
                else
                {
                    // For the first item, we are already at 'closePos' (startPos).
                    // But 'startPos' is essentially 'approachEndPos' if we consider where approach ends?
                    // Actually, if we want to support Idle, we should start at 'approachEndPos'.
                    // Let's ensure the initial position set above matches 'approachEndPos' for i=0.
                    // Recalculate start pos above locally or just force it here?
                    // Better to just snap it here to be safe or rely on the setup above.
                    // To be safe, let's force the initial position to be exactly 'approachEndPos' before the sequence starts?
                    // Or just let the sequence run.
                    // If we skip the append, the camera is at 'startPos'.
                    // 'stayEndPos' tween starts from current pos.
                }

                // 2. Stay (Idle Move)
                if (iType != IdleMoveType.None)
                {
                    _sequence.Append(_camera.transform.DOMove(stayEndPos, dStay).SetEase(Ease.InOutSine));
                    // No rotation during idle, just position drift (strafe)
                }
                else
                {
                    _sequence.AppendInterval(dStay);
                }

                // 3. Retract (Close End -> Far)
                // Note: We start retracting from wherever Stay ended (stayEndPos)
                // Use DORotate here instead of DOLookAt to avoid the "wiggle" (yaw change) if we strafed.
                // We want to smoothly rotate to the "Ideal Far Rotation", not track the object.
                Quaternion farRot = Quaternion.LookRotation(lookAtPos - farPos);

                // Check if we should stop here (Last Object & StopAtLastObject enabled & Not Looping)
                bool isLast = (i == _targets.Count - 1);
                if (isLast && _stopAtLastObject && !_loop)
                {
                    // Do nothing more. The sequence ends here, staying at the "Close" (technically "Stay End") position.
                }
                else
                {
                    _sequence.Append(_camera.transform.DOMove(farPos, dFar).SetEase(eRetract));
                    _sequence.Join(_camera.transform.DORotate(farRot.eulerAngles, dFar).SetEase(eRetract));

                    // 4. Transition to Next (Far -> Next Far)
                    if (_loop || i < _targets.Count - 1)
                    {
                        Vector3 nextFarPos = GetFarPos(nextObj);
                        Vector3 nextLookAt = GetLookAtPos(nextObj);

                        // The rotation at Far Position is constant because _approachDirection is global.
                        // We simply rotate to the 'Ideal Far Rotation' for the NEXT object (which is the same as current), 
                        // ensuring we don't 'look at' the object while moving (no panning), just strafing.
                        Quaternion nextFarRot = Quaternion.LookRotation(nextLookAt - nextFarPos);

                        _sequence.Append(_camera.transform.DOMove(nextFarPos, dTrans).SetEase(eTrans));
                        // Rotate to the target's ideal rotation (which should be effectively keeping it straight if aligned)
                        _sequence.Join(_camera.transform.DORotate(nextFarRot.eulerAngles, dTrans).SetEase(eTrans));
                    }
                }

                if (_loop)
                    _sequence.SetLoops(-1);
                else if (!string.IsNullOrEmpty(_nextSceneName))
                {
                    // Transition to Next Scene on complete
                    _sequence.OnComplete(() =>
                    {
                        // Delay before transition if requested (using a coroutine or tween delay)
                        DOVirtual.DelayedCall(_delayBeforeTransition, () =>
                        {
                            var transitionManager = CinematicSystem.Transitions.SceneTransitionManager.Instance;
                            if (transitionManager == null)
                            {
                                // Create Manager if missing (generic fallback)
                                GameObject go = new GameObject("SceneTransitionManager");
                                transitionManager = go.AddComponent<CinematicSystem.Transitions.SceneTransitionManager>();
                            }

                            transitionManager.CrossfadeToScene(_nextSceneName, _transitionDuration);
                        });
                    });
                }
            }
        }

        private System.Collections.IEnumerator PlaySubtitleRoutine()
        {
            // Wait for instance if null (race condition)
            while (UI_System.Subtitles.SimpleSubtitleUI.Instance == null)
            {
                yield return null;
            }

            var subtitleUI = UI_System.Subtitles.SimpleSubtitleUI.Instance;


            Debug.Log($"[TableCameraSequence] Starting Subtitles. Count: {_subtitles.Count}");

            foreach (var sub in _subtitles)
            {
                if (sub.PreDelay > 0)
                {
                    Debug.Log($"[TableCameraSequence] Waiting PreDelay: {sub.PreDelay}");
                    yield return new WaitForSeconds(sub.PreDelay);
                }

                string textToShow = sub.Text;
                // If a key is provided, try to localize it
                if (!string.IsNullOrEmpty(sub.Key))
                {
                    if (Localization.LocalizationManager.Instance != null)
                    {
                        textToShow = Localization.LocalizationManager.Instance.GetLocalizedValue(sub.Key);
                    }
                }

                if (!string.IsNullOrEmpty(textToShow))
                {
                    Debug.Log($"[TableCameraSequence] Showing Subtitle: {textToShow} for {sub.Duration}s");
                    subtitleUI.ShowSubtitle(textToShow, sub.Duration);

                    // Wait for the duration of the subtitle before processing the next one?
                    // Usually subtitles flow: Delay -> Show(duration) -> (Wait duration) -> Next Delay...
                    yield return new WaitForSeconds(sub.Duration);
                }
            }
        }

        private Vector3 GetClosePos(Transform target, Vector3 idleOffset = default)
        {
            if (target == null) return Vector3.zero;
            Vector3 basePos = target.position + (_approachDirection.normalized * _closeDistance) + (Vector3.up * _heightOffset);
            return basePos - (idleOffset * 0.5f); // Used for initial positioning accounting for idle start
        }

        private Vector3 GetFarPos(Transform target)
        {
            if (target == null) return Vector3.zero;
            return target.position + (_approachDirection.normalized * _farDistance) + (Vector3.up * _heightOffset);
        }

        private Vector3 GetLookAtPos(Transform target)
        {
            if (target == null) return Vector3.zero;
            return target.position + Vector3.up * _lookAtHeightOffset;
        }

        private void OnDrawGizmos()
        {
            if (_targets == null || _targets.Count == 0) return;

            for (int i = 0; i < _targets.Count; i++)
            {
                var t = _targets[i];
                if (t.TargetObject == null) continue;

                Vector3 closePos = GetClosePos(t.TargetObject);
                Vector3 farPos = GetFarPos(t.TargetObject);
                Vector3 lookAt = GetLookAtPos(t.TargetObject);

                // Draw Positions
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(closePos, 0.1f); // Close Camera Pos
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(farPos, 0.1f);   // Far Camera Pos

                // Draw Look Direction
                Gizmos.color = Color.red;
                Gizmos.DrawLine(closePos, lookAt);
                Gizmos.DrawLine(farPos, lookAt);
                Gizmos.DrawWireSphere(lookAt, 0.05f); // Focus Point

                // Draw Path Flow
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(farPos, closePos); // Approach/Retract path

                // Draw Transition to Next
                if (i < _targets.Count - 1 || _loop)
                {
                    var nextT = _targets[(i + 1) % _targets.Count];
                    if (nextT.TargetObject != null)
                    {
                        Vector3 nextFarPos = GetFarPos(nextT.TargetObject);
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(farPos, nextFarPos); // Lateral Move
                    }
                }
            }
        }
    }
}
