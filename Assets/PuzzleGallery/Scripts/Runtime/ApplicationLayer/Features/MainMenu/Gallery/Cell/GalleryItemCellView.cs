using DG.Tweening;
using PuzzleGallery.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu.Cell
{
    public class GalleryItemCellView : VirtualizedCell, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Components")]
        [SerializeField] private RemoteImageView _remoteImage;
        [SerializeField] private PremiumBadgeView _premiumBadge;
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _contentRoot;

        [Header("Animation")]
        [SerializeField] private bool _enablePressAnimation = true;
        [SerializeField] private float _pressScale = 0.95f;
        [SerializeField] private float _pressDuration = 0.1f;

        private GalleryItemData _data;
        private Tween _scaleTween;
        private Vector3 _originalScale;

        private void Awake()
        {
            if (!_contentRoot) throw new System.Exception("Content Root is not assigned in GalleryItemCell.");
            if (!_button) throw new System.Exception("_button is not assigned in GalleryItemCell.");

            _button.onClick.AddListener(OnButtonClicked);
            _originalScale = _contentRoot.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_enablePressAnimation)
            {
                return;
            }

            _scaleTween?.Kill();
            _scaleTween = _contentRoot
                .DOScale(_originalScale * _pressScale, _pressDuration)
                .SetEase(Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_enablePressAnimation)
            {
                return;
            }

            _scaleTween?.Kill();
            _scaleTween = _contentRoot
                .DOScale(_originalScale, _pressDuration)
                .SetEase(Ease.OutBack);
        }

        private void OnDestroy()
        {
            _scaleTween?.Kill();
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        public void Configure(GalleryItemData data)
        {
            _data = data;

            if (_remoteImage != null && !string.IsNullOrEmpty(data.Url))
            {
                _remoteImage.LoadImage(data.Url);
            }

            if (_premiumBadge != null)
            {
                _premiumBadge.SetVisible(data.IsPremium);
            }
        }

        public override void OnRecycle()
        {
            base.OnRecycle();

            if (_remoteImage != null)
            {
                _remoteImage.Clear();
            }

            if (_premiumBadge != null)
            {
                _premiumBadge.SetVisible(false);
            }

            _data = null;
        }

        private void OnButtonClicked()
        {
            if (Index >= 0)
            {
                InvokeCellClicked();
            }
        }
    }
}