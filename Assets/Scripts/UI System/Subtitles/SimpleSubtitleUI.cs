using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI_System.Subtitles
{
    public class SimpleSubtitleUI : MonoBehaviour
    {
        public static SimpleSubtitleUI Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private Image _imageObj;
        [SerializeField] private float _fadeDuration = 0.5f;

        private Sequence _currentSequence;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Ideally this is part of the scene, not persisted, or part of a persistent UI manager
            }
            else
            {
                Destroy(gameObject);
            }

            if (_subtitleText != null)
            {
                _subtitleText.alpha = 0f;
            }
            if (_imageObj != null)
            {
                var c = _imageObj.color;
                c.a = 0f;
                _imageObj.color = c;
            }
        }

        public void ShowSubtitle(string text, float duration)
        {
            if (_subtitleText == null) return;

            // Kill any ongoing sequence
            if (_currentSequence != null)
                _currentSequence.Kill();

            _currentSequence = DOTween.Sequence();

            // Set text
            _subtitleText.text = text;

            // Fade In
            _currentSequence.Append(_subtitleText.DOFade(1f, _fadeDuration));
            if (_imageObj != null) _currentSequence.Join(_imageObj.DOFade(0.5f, _fadeDuration));

            // Wait for duration (minus fade times mostly, or full duration visible)
            _currentSequence.AppendInterval(duration);

            // Fade Out
            _currentSequence.Append(_subtitleText.DOFade(0f, _fadeDuration));
            if (_imageObj != null) _currentSequence.Join(_imageObj.DOFade(0f, _fadeDuration));
        }

        public void HideImmediate()
        {
            if (_currentSequence != null) _currentSequence.Kill();
            if (_subtitleText != null) _subtitleText.alpha = 0f;
            if (_imageObj != null)
            {
                var c = _imageObj.color;
                c.a = 0f;
                _imageObj.color = c;
            }
        }
    }
}
