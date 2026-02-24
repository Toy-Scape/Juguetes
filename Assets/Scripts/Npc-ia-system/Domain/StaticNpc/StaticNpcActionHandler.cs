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
        public Transform targetTransform;
        public float duration = 1f;
        public Ease easeType = Ease.Linear;
        public bool waitForCompletion = true;

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
        private Coroutine _actionRoutine;
        private Sequence _currentMoveSequence;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (playSequenceOnStart)
            {
                PlayAllActionsSequentially();
            }
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
                        // A small delay to ensure the animation states transition properly
                        yield return new WaitForSeconds(0.1f);
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
            if (action.targetTransform == null)
            {
                Debug.LogWarning($"{gameObject.name}: Move action has no target transform.");
                yield break;
            }

            _currentMoveSequence = DOTween.Sequence();
            _currentMoveSequence.Append(transform.DOMove(action.targetTransform.position, action.duration).SetEase(action.easeType));

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
            _currentMoveSequence.Append(transform.DORotate(action.targetTransform.rotation.eulerAngles, action.duration).SetEase(action.easeType));

            if (action.waitForCompletion)
            {
                yield return _currentMoveSequence.WaitForCompletion();
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
    }
}
