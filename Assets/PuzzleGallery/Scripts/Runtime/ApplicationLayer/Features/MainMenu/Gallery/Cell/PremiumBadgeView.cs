using DG.Tweening;
using UnityEngine;

namespace PuzzleGallery.Features.MainMenu.Cell
{
    public class PremiumBadgeView : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float _showDuration = 0.3f;
        [SerializeField] private float _pulseScale = 1.1f;
        [SerializeField] private float _pulseDuration = 0.5f;
        [SerializeField] private bool _enablePulse = true;

        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private Tween _pulseTween;
        private bool _isVisible;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
        }

        private void OnEnable()
        {
            if (_enablePulse && _isVisible)
            {
                StartPulseAnimation();
            }
        }

        private void OnDisable()
        {
            StopPulseAnimation();
        }

        public void Show(bool animated = true)
        {
            _isVisible = true;
            gameObject.SetActive(true);

            if (animated)
            {
                _rectTransform.localScale = Vector3.zero;
                _rectTransform.DOScale(_originalScale, _showDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        if (_enablePulse)
                        {
                            StartPulseAnimation();
                        }
                    });
            }
            else
            {
                _rectTransform.localScale = _originalScale;
                if (_enablePulse)
                {
                    StartPulseAnimation();
                }
            }
        }

        public void Hide(bool animated = true)
        {
            _isVisible = false;
            StopPulseAnimation();

            if (animated)
            {
                _rectTransform.DOScale(Vector3.zero, _showDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            gameObject.SetActive(visible);

            if (visible)
            {
                _rectTransform.localScale = _originalScale;
                if (_enablePulse)
                {
                    StartPulseAnimation();
                }
            }
            else
            {
                StopPulseAnimation();
            }
        }

        private void StartPulseAnimation()
        {
            StopPulseAnimation();

            _pulseTween = _rectTransform
                .DOScale(_originalScale * _pulseScale, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopPulseAnimation()
        {
            _pulseTween?.Kill();
            _pulseTween = null;

            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale;
            }
        }

        private void OnDestroy()
        {
            StopPulseAnimation();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
        }
#endif
    }
}
