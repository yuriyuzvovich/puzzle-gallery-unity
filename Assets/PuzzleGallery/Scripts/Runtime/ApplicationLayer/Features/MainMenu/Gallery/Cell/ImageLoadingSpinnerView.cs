using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu.Cell
{
    public class ImageLoadingSpinnerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _spinnerTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _spinnerImage;

        [Header("Animation Settings")]
        [SerializeField] private float _rotationSpeed = 360f;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private bool _clockwise = true;

        private Tween _rotationTween;
        private Tween _fadeTween;
        private bool _isSpinning;

        private void Awake()
        {
            if (_spinnerTransform == null)
            {
                _spinnerTransform = GetComponent<RectTransform>();
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void OnDisable()
        {
            StopSpinning();
        }

        private void OnDestroy()
        {
            _rotationTween?.Kill();
            _fadeTween?.Kill();
        }

        public void StartSpinning()
        {
            if (_isSpinning)
            {
                return;
            }

            _isSpinning = true;

            _rotationTween?.Kill();

            _spinnerTransform.localRotation = Quaternion.identity;

            float rotationDirection = _clockwise ? -1f : 1f;
            _rotationTween = _spinnerTransform
                .DOLocalRotate(new Vector3(0, 0, 360f * rotationDirection), 360f / _rotationSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        public void StopSpinning()
        {
            _isSpinning = false;
            _rotationTween?.Kill();

            if (_spinnerTransform != null)
            {
                _spinnerTransform.localRotation = Quaternion.identity;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            _fadeTween?.Kill();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _fadeTween = _canvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.OutQuad);
            }

            StartSpinning();
        }

        public void Hide()
        {
            StopSpinning();

            _fadeTween?.Kill();

            if (_canvasGroup != null)
            {
                _fadeTween = _canvasGroup.DOFade(0f, _fadeDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        if (gameObject != null)
                        {
                            gameObject.SetActive(false);
                        }
                    });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SetColor(Color color)
        {
            if (_spinnerImage != null)
            {
                _spinnerImage.color = color;
            }
        }

        public void SetSize(float size)
        {
            if (_spinnerTransform != null)
            {
                _spinnerTransform.sizeDelta = new Vector2(size, size);
            }
        }
    }
}
