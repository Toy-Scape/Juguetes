using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Domain.StaticNpc
{
    public enum NpcActionType
    {
        Move,
        Rotate,
        PlayAnimation,
        Wait
    }

    [Serializable]
    public class StaticNpcAction
    {
        [Tooltip("A unique identifier for this action so it can be played individually.")]
        public string actionId;

        public NpcActionType actionType;

        [Header("Movement / Rotation")]
        [Tooltip("Single target for movement or rotation.")]
        public Transform targetTransform;
        [Tooltip("Multiple points for 'Move' action. If empty, uses targetTransform.")]
        public List<Transform> movePath = new List<Transform>();
        [Tooltip("If true, the NPC will look in the direction it's moving.")]
        public bool lookForwardWhileMoving = true;
        [Tooltip("If true, Move uses 'movementSpeed'. If false, uses 'duration'.")]
        public bool moveBySpeed = true;
        [Tooltip("Units per second for movement.")]
        public float movementSpeed = 3f;
        public float duration = 1f;
        public Ease easeType = Ease.Linear;
        public bool waitForCompletion = true;

        [Header("Animator Control During Move")]
        [Tooltip("Float parameter Name to set on the Animator while moving (e.g. 'Speed'). Leave empty if not needed.")]
        public string moveSpeedParameter;
        [Tooltip("Value to set for the parameter while moving.")]
        public float moveSpeedValue = 1f;

        [Header("Animation")]
        public string animationStateOrTriggerName;
        [Tooltip("If true, uses CrossFade to transition. If false, uses SetTrigger.")]
        public bool crossFade = false;
        public float crossFadeDuration = 0.2f;

        [Header("Wait")]
        public float waitTime = 1f;

        [Header("Events")]
        public UnityEvent onActionStart;
        public UnityEvent onActionComplete;
    }

    [RequireComponent(typeof(Animator))]
    public class StaticNpcActionHandler : MonoBehaviour
    {
        [Header("Actions Configuration")]
        [Tooltip("List of actions. Can be executed sequentially or individually by ID.")]
        public List<StaticNpcAction> actions = new List<StaticNpcAction>();
        public bool playSequenceOnStart = false;

        [Header("Sequence Events")]
        public UnityEvent onSequenceStart;
        public UnityEvent onSequenceComplete;

        private Animator _animator;
        private StaticNpcScannerIK _scannerIK;
        private Coroutine _actionRoutine;
        private Sequence _currentMoveSequence;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _scannerIK = GetComponent<StaticNpcScannerIK>();
        }

        private void Start()
        {
            if (playSequenceOnStart)
            {
                PlayAllActionsSequentially();
            }
        }

        private void OnDestroy()
        {
            StopActions();
        }

        /// <summary>
        /// Starts executing the entire list of actions from the beginning.
        /// </summary>
        public void PlayAllActionsSequentially()
        {
            StopActions();
            _actionRoutine = StartCoroutine(ExecuteActionsRoutine(actions));
        }

        /// <summary>
        /// Executes a single action by its ID.
        /// </summary>
        /// <param name="actionId">The actionId defined in the action.</param>
        public void PlayActionById(string actionId)
        {
            var action = actions.Find(a => a.actionId == actionId);
            if (action != null)
            {
                StopActions();
                _actionRoutine = StartCoroutine(ExecuteSingleActionRoutine(action));
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Action with ID '{actionId}' not found.");
            }
        }

        /// <summary>
        /// Executes a sequence of actions by their IDs, provided as a comma-separated string.
        /// Useful for calling from UnityEvents.
        /// </summary>
        /// <param name="commaSeparatedIds">E.g. "stand-up,Move"</param>
        public void PlayActionsByIdSequence(string commaSeparatedIds)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedIds))
            {
                Debug.LogWarning($"{gameObject.name}: PlayActionsByIdSequence called with empty string.");
                return;
            }

            Debug.Log($"[{gameObject.name}] PlayActionsByIdSequence parsing: {commaSeparatedIds}");

            string[] ids = commaSeparatedIds.Split(',');
            List<StaticNpcAction> actionsToPlay = new List<StaticNpcAction>();

            foreach (var id in ids)
            {
                string cleanId = id.Trim();
                var action = actions.Find(a => a.actionId == cleanId);
                if (action != null)
                {
                    actionsToPlay.Add(action);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: Action with ID '{cleanId}' not found in sequence.");
                }
            }

            if (actionsToPlay.Count > 0)
            {
                Debug.Log($"[{gameObject.name}] Playing {actionsToPlay.Count} actions sequentially.");
                StopActions();
                _actionRoutine = StartCoroutine(ExecuteActionsRoutine(actionsToPlay));
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] No valid actions were found to play for sequence: {commaSeparatedIds}");
            }
        }

        /// <summary>
        /// Stops the current sequence or action.
        /// </summary>
        public void StopActions()
        {
            if (_actionRoutine != null)
            {
                StopCoroutine(_actionRoutine);
                _actionRoutine = null;
            }
            if (_currentMoveSequence != null && _currentMoveSequence.IsActive())
            {
                _currentMoveSequence.Kill();
            }
        }

        private IEnumerator ExecuteActionsRoutine(List<StaticNpcAction> actionsToPlay)
        {
            onSequenceStart?.Invoke();

            foreach (var action in actionsToPlay)
            {
                yield return ExecuteSingleActionInstruction(action);
            }

            _actionRoutine = null;
            onSequenceComplete?.Invoke();
        }

        private IEnumerator ExecuteSingleActionRoutine(StaticNpcAction action)
        {
            yield return ExecuteSingleActionInstruction(action);
            _actionRoutine = null;
        }

        private IEnumerator ExecuteSingleActionInstruction(StaticNpcAction action)
        {
            action.onActionStart?.Invoke();

            switch (action.actionType)
            {
                case NpcActionType.Move:
                    yield return ExecuteMove(action);
                    break;
                case NpcActionType.Rotate:
                    yield return ExecuteRotate(action);
                    break;
                case NpcActionType.PlayAnimation:
                    ExecuteAnimation(action);

                    if (action.waitForCompletion)
                    {
                        yield return WaitForAnimation(action.animationStateOrTriggerName);
                    }
                    break;
                case NpcActionType.Wait:
                    yield return new WaitForSeconds(action.waitTime);
                    break;
            }

            action.onActionComplete?.Invoke();
        }

        private IEnumerator ExecuteMove(StaticNpcAction action)
        {
            List<Transform> path = new List<Transform>();
            if (action.movePath != null && action.movePath.Count > 0)
            {
                path.AddRange(action.movePath);
            }
            else if (action.targetTransform != null)
            {
                path.Add(action.targetTransform);
            }

            if (path.Count == 0)
            {
                Debug.LogWarning($"{gameObject.name}: Move action has no target transform or path.");
                yield break;
            }

            if (_animator != null && !string.IsNullOrEmpty(action.moveSpeedParameter))
            {
                _animator.SetFloat(action.moveSpeedParameter, action.moveSpeedValue);
            }

            if (_scannerIK != null)
            {
                _scannerIK.enabled = false;
            }

            _currentMoveSequence = DOTween.Sequence();
            _currentMoveSequence.SetLink(gameObject);
            _currentMoveSequence.OnKill(() => ResetMoveAnimation(action));

            // Setup Waypoints
            Vector3[] waypoints = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                waypoints[i] = path[i].position;
            }

            // DOPath will follow all points.
            var pathTween = transform.DOPath(waypoints, action.moveBySpeed ? action.movementSpeed : action.duration, PathType.Linear)
                .SetEase(action.easeType);
                
            if (action.moveBySpeed) 
            {
                pathTween.SetSpeedBased();
            }

            if (action.lookForwardWhileMoving)
            {
                // lookAhead between 0.001 and 1. A small value handles straight line segments well.
                pathTween.SetLookAt(0.01f); 
            }

            _currentMoveSequence.Append(pathTween);

            if (action.waitForCompletion)
            {
                yield return _currentMoveSequence.WaitForCompletion();
            }
        }

        private IEnumerator ExecuteRotate(StaticNpcAction action)
        {
            if (action.targetTransform == null)
            {
                Debug.LogWarning($"{gameObject.name}: Rotate action has no target transform.");
                yield break;
            }

            _currentMoveSequence = DOTween.Sequence();
            _currentMoveSequence.SetLink(gameObject);
            _currentMoveSequence.Append(transform.DORotate(action.targetTransform.rotation.eulerAngles, action.duration).SetEase(action.easeType));

            if (action.waitForCompletion)
            {
                yield return _currentMoveSequence.WaitForCompletion();
            }
        }

        IEnumerator WaitForAnimation(string stateName)
        {
            // Esperar a que el Animator entre en el estado
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

            while (!state.IsName(stateName))
            {
                yield return null;
                state = _animator.GetCurrentAnimatorStateInfo(0);
            }

            // Esperar a que termine
            while (state.normalizedTime < 1f)
            {
                yield return null;
                state = _animator.GetCurrentAnimatorStateInfo(0);
            }
        }

        private void ExecuteAnimation(StaticNpcAction action)
        {
            if (_animator == null) return;

            if (!string.IsNullOrEmpty(action.animationStateOrTriggerName))
            {
                if (action.crossFade)
                {
                    _animator.CrossFade(action.animationStateOrTriggerName, action.crossFadeDuration);
                }
                else
                {
                    _animator.SetTrigger(action.animationStateOrTriggerName);
                }
            }
        }

        private void ResetMoveAnimation(StaticNpcAction action)
        {
            if (_animator != null && !string.IsNullOrEmpty(action.moveSpeedParameter))
            {
                _animator.SetFloat(action.moveSpeedParameter, 0f);
            }
            if (_scannerIK != null)
            {
                _scannerIK.enabled = true;
            }
        }
    }
}
