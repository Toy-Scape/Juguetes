using DG.Tweening;
using UnityEngine;

namespace CinematicSystem.Transitions
{
    /// <summary>
    /// Attach this script to a GameObject in your scene to define behavior when the scene is loaded via TransitionManager.
    /// This allows you to serialize references (like Animator) directly in the Inspector.
    /// </summary>
    public class SceneIntroTrigger : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator _targetAnimator;
        [SerializeField] private string _triggerName = "init";

        [Header("Optional Delay")]
        [SerializeField] private float _delay = 0f;

        private void Start()
        {
            if (_targetAnimator != null && !string.IsNullOrEmpty(_triggerName))
            {
                if (_delay > 0)
                {
                    DOVirtual.DelayedCall(_delay, () =>
                    {
                        if (_targetAnimator != null)
                            _targetAnimator.SetTrigger(_triggerName);
                    });
                }
                else
                {
                    _targetAnimator.SetTrigger(_triggerName);
                }
            }
            else
            {
                Debug.LogWarning($"[SceneIntroTrigger] Animator missing or Trigger Name empty on '{gameObject.name}'.");
            }
        }
    }
}
