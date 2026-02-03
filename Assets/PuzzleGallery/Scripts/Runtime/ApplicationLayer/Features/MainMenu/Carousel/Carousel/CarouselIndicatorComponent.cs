using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class CarouselIndicatorComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _image;
        [SerializeField] private RectTransform _root;

        [Header("State Sprites")]
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _unselectedSprite;

        [Header("State Styling")]
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _unselectedColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float _selectedScale = 1.3f;
        [SerializeField] private float _animationDuration = 0.2f;
        [SerializeField] private Ease _ease = Ease.OutQuad;

        private Tween _colorTween;
        private Tween _scaleTween;
        private bool _isSelected;

        private void Awake()
        {
            if (_root == null)
            {
                _root = GetComponent<RectTransform>();
            }

            if (_image == null)
            {
                _image = GetComponentInChildren<Image>(true);
            }
        }

        private void OnDestroy()
        {
            _colorTween?.Kill();
            _scaleTween?.Kill();
        }

        public void SetSelected(bool isSelected, bool animated)
        {
            _isSelected = isSelected;

            if (_image != null)
            {
                var targetSprite = isSelected ? _selectedSprite : _unselectedSprite;
                if (targetSprite == null)
                {
                    targetSprite = _unselectedSprite != null ? _unselectedSprite : _selectedSprite;
                }

                if (targetSprite != null)
                {
                    _image.sprite = targetSprite;
                }
            }

            var targetColor = isSelected ? _selectedColor : _unselectedColor;
            var targetScale = isSelected ? _selectedScale : 1f;

            if (!animated || _animationDuration <= 0f)
            {
                _colorTween?.Kill();
                _scaleTween?.Kill();

                if (_image != null)
                {
                    _image.color = targetColor;
                }

                if (_root != null)
                {
                    _root.localScale = Vector3.one * targetScale;
                }

                return;
            }

            _colorTween?.Kill();
            _scaleTween?.Kill();

            if (_image != null)
            {
                _colorTween = _image.DOColor(targetColor, _animationDuration)
                    .SetEase(_ease);
            }

            if (_root != null)
            {
                _scaleTween = _root.DOScale(targetScale, _animationDuration)
                    .SetEase(_ease);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_root == null)
            {
                _root = GetComponent<RectTransform>();
            }

            if (_image == null)
            {
                _image = GetComponentInChildren<Image>(true);
            }

            if (!Application.isPlaying && _image != null)
            {
                var targetSprite = _isSelected ? _selectedSprite : _unselectedSprite;
                if (targetSprite == null)
                {
                    targetSprite = _unselectedSprite != null ? _unselectedSprite : _selectedSprite;
                }

                if (targetSprite != null)
                {
                    _image.sprite = targetSprite;
                }
            }
        }
#endif
    }
}
