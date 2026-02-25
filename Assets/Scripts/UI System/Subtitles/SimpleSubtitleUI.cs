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
        private float _originalTextAlpha;
        private Color _originalImageColor;
        private float _originalImageAlpha;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            if (_subtitleText != null)
            {
                _originalTextAlpha = _subtitleText.alpha;
                _subtitleText.alpha = 0f;
            }

            if (_imageObj != null)
            {
                _originalImageColor = _imageObj.color;
                _originalImageAlpha = _imageObj.color.a;
                var c = _imageObj.color;
                c.a = 0f;
                _imageObj.color = c;
            }
        }

        public void ShowSubtitle(string text, float duration)
        {
            if (_subtitleText == null) return;

            if (_currentSequence != null)
                _currentSequence.Kill();

            _currentSequence = DOTween.Sequence();

            _subtitleText.text = text;

            _currentSequence.Append(_subtitleText.DOFade(1f, _fadeDuration));
            if (_imageObj != null)
                _currentSequence.Join(_imageObj.DOFade(_originalImageAlpha, _fadeDuration));

            _currentSequence.AppendInterval(duration);

            _currentSequence.Append(_subtitleText.DOFade(0f, _fadeDuration));
            if (_imageObj != null)
                _currentSequence.Join(_imageObj.DOFade(0f, _fadeDuration));
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