using DG.Tweening;
using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    public class AnimatedDecorView : MonoBehaviour
    {
        public enum AnimationPreset
        {
            Default,
            Subtle,
            Fast,
            Slow,
            Custom
        }

        [Header("Animation Settings")]
        [SerializeField] private AnimationPreset _preset = AnimationPreset.Default;
        [SerializeField] private bool _autoPlay = true;

        [Header("Custom Settings (when preset = Custom)")]
        [SerializeField] private float _minScale = 0f;
        [SerializeField] private float _maxScale = 1f;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _ease = Ease.OutQuad;
        [SerializeField] private float _startDelay = 0f;

        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private Tween _animationTween;
        private bool _isAnimating;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (_rectTransform != null)
            {
                _originalScale = _rectTransform.localScale;
            }
        }

        private void OnEnable()
        {
            if (_autoPlay)
            {
                StartAnimation();
            }
        }

        private void OnDisable()
        {
            StopAnimation();
        }

        private void OnDestroy()
        {
            _animationTween?.Kill();
        }

        public void StartAnimation()
        {
            if (_rectTransform == null)
            {
                return;
            }

            _animationTween?.Kill();

            _isAnimating = true;

            _rectTransform.localScale = _originalScale;

            GetPresetValues(out float minScale, out float maxScale, out float duration, out Ease ease, out bool useSequence);

            if (useSequence)
            {
                var seq = DOTween.Sequence();
                seq.Append(_rectTransform.DOScale(_originalScale * minScale, duration / 2f).SetEase(ease));
                seq.Append(_rectTransform.DOScale(_originalScale, duration / 2f).SetEase(ease));
                seq.SetDelay(_startDelay);
                seq.SetLoops(-1);
                _animationTween = seq;
            }
            else
            {
                _animationTween = _rectTransform
                    .DOScale(_originalScale * minScale, duration)
                    .SetEase(ease)
                    .SetDelay(_startDelay)
                    .SetLoops(-1, LoopType.Restart);
            }
        }

        public void StopAnimation()
        {
            _isAnimating = false;
            _animationTween?.Kill();
            _animationTween = null;

            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale;
            }
        }

        public void SetPreset(AnimationPreset preset)
        {
            _preset = preset;

            if (_isAnimating)
            {
                StartAnimation();
            }
        }

        private void GetPresetValues(out float minScale, out float maxScale, out float duration, out Ease ease, out bool useSequence)
        {
            switch (_preset)
            {
                case AnimationPreset.Default:
                    minScale = 0f;
                    maxScale = 1f;
                    duration = 1f;
                    ease = Ease.OutQuad;
                    useSequence = true;
                    break;

                case AnimationPreset.Subtle:
                    minScale = 0.7f;
                    maxScale = 1f;
                    duration = 1.5f;
                    ease = Ease.InOutSine;
                    useSequence = true;
                    break;

                case AnimationPreset.Fast:
                    minScale = 0f;
                    maxScale = 1f;
                    duration = 0.6f;
                    ease = Ease.OutQuad;
                    useSequence = true;
                    break;

                case AnimationPreset.Slow:
                    minScale = 0f;
                    maxScale = 1f;
                    duration = 2f;
                    ease = Ease.InOutSine;
                    useSequence = true;
                    break;

                case AnimationPreset.Custom:
                    minScale = _minScale;
                    maxScale = _maxScale;
                    duration = _duration;
                    ease = _ease;
                    useSequence = true;
                    break;

                default:
                    minScale = 0f;
                    maxScale = 1f;
                    duration = 1f;
                    ease = Ease.OutQuad;
                    useSequence = true;
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            _duration = Mathf.Max(0.1f, _duration);
            _minScale = Mathf.Clamp(_minScale, 0f, 2f);
            _maxScale = Mathf.Clamp(_maxScale, 0f, 2f);
            _startDelay = Mathf.Max(0f, _startDelay);
        }
#endif
    }
}
