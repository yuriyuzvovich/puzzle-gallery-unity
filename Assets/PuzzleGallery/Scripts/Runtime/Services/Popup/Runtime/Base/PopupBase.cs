using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Services.Popup
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour, IPopup
    {
        [Header("Base References")]
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform _panel;
        [SerializeField] protected Button _backgroundButton;
        [SerializeField] protected Button _closeButton;

        [Header("Animation Settings")]
        [SerializeField] protected float _animationDuration = 0.3f;
        [SerializeField] protected float _scaleFrom = 0.8f;
        [SerializeField] protected Ease _showEase = Ease.OutBack;
        [SerializeField] protected Ease _hideEase = Ease.InBack;

        protected PopupData _data;
        private Tween _showTween;
        private Tween _hideTween;

        public event Action OnClosed;
        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_backgroundButton != null)
            {
                _backgroundButton.onClick.AddListener(OnBackgroundClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }

            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            _showTween?.Kill();
            _hideTween?.Kill();

            if (_backgroundButton != null)
            {
                _backgroundButton.onClick.RemoveListener(OnBackgroundClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseClicked);
            }
        }

        public virtual void SetData(PopupData data)
        {
            _data = data;
        }

        public virtual async UniTask ShowAsync()
        {
            if (this == null || gameObject == null)
            {
                return;
            }

            gameObject.SetActive(true);
            IsVisible = true;

            _showTween?.Kill();
            _hideTween?.Kill();

            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (_panel != null)
            {
                _panel.localScale = Vector3.one * _scaleFrom;
            }

            var sequence = DOTween.Sequence();

            sequence.Join(_canvasGroup.DOFade(1f, _animationDuration));

            if (_panel != null)
            {
                sequence.Join(_panel.DOScale(1f, _animationDuration).SetEase(_showEase));
            }

            _showTween = sequence;
            await sequence.AsyncWaitForCompletion().AsUniTask();

            if (this == null || _canvasGroup == null)
            {
                return;
            }

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            OnShown();
        }

        public virtual async UniTask HideAsync()
        {
            if (!IsVisible)
            {
                return;
            }

            IsVisible = false;

            _showTween?.Kill();
            _hideTween?.Kill();

            if (this == null || _canvasGroup == null)
            {
                return;
            }

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            var sequence = DOTween.Sequence();

            sequence.Join(_canvasGroup.DOFade(0f, _animationDuration));

            if (_panel != null)
            {
                sequence.Join(_panel.DOScale(_scaleFrom, _animationDuration).SetEase(_hideEase));
            }

            _hideTween = sequence;
            await sequence.AsyncWaitForCompletion().AsUniTask();

            if (this == null || gameObject == null)
            {
                return;
            }

            gameObject.SetActive(false);

            OnHidden();
            OnClosed?.Invoke();
        }

        protected virtual void OnShown()
        {
        }

        protected virtual void OnHidden()
        {
        }

        protected virtual void OnBackgroundClicked()
        {
            HideAsync().Forget();
        }

        protected virtual void OnCloseClicked()
        {
            HideAsync().Forget();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }
#endif
    }
}
