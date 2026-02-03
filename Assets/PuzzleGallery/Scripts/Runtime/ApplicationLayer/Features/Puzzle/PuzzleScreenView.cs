using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGallery.Features.MainMenu.Cell;
using PuzzleGallery.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.Puzzle
{
    public class PuzzleScreenView : MonoBehaviour, IPuzzleScreenView
    {
        [Header("Components")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;

        [Header("References")]
        [SerializeField] private RemoteImageView _remoteImage;
        [SerializeField] private Button _backButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _imageContainer;

        [Header("Animation Settings")]
        [SerializeField] private float _transitionDuration = 0.4f;
        [SerializeField] private float _imageScaleFrom = 0.8f;

        private Sequence _currentTransition;

        public bool IsVisible => _canvas && _canvas.enabled;

        public event Action OnBackClicked;

        private void Awake()
        {
            _backButton.onClick.AddListener(HandleBackClicked);
        }

        private void OnDestroy()
        {
            _currentTransition?.Kill();
            if (_backButton)
            {
                _backButton.onClick.RemoveListener(HandleBackClicked);
            }
        }

        public void SetImageUrl(string url)
        {
            _remoteImage.LoadImage(url);
        }

        public void Show()
        {
            _canvas.enabled = true;
            _graphicRaycaster.enabled = true;

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _imageContainer.localScale = Vector3.one;
        }

        public async UniTask ShowAsync()
        {
            _currentTransition?.Kill();

            _canvas.enabled = true;
            _graphicRaycaster.enabled = true;

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _imageContainer.localScale = Vector3.one * _imageScaleFrom;

            _currentTransition = DOTween.Sequence();

            _currentTransition.Append(
                _canvasGroup.DOFade(1f, _transitionDuration)
                    .SetEase(Ease.OutQuad)
            );

            _currentTransition.Join(
                _imageContainer.DOScale(1f, _transitionDuration)
                    .SetEase(Ease.OutBack)
            );

            await _currentTransition.AsyncWaitForCompletion();

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _currentTransition?.Kill();
            _remoteImage.Clear();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvas.enabled = false;
            _graphicRaycaster.enabled = false;
        }

        public async UniTask HideAsync()
        {
            _currentTransition?.Kill();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _currentTransition = DOTween.Sequence();

            _currentTransition.Append(
                _canvasGroup.DOFade(0f, _transitionDuration * 0.7f)
                    .SetEase(Ease.InQuad)
            );
            _currentTransition.Join(
                _imageContainer.DOScale(_imageScaleFrom, _transitionDuration * 0.7f)
                    .SetEase(Ease.InBack)
            );

            await _currentTransition.AsyncWaitForCompletion();

            _remoteImage.Clear();
            _canvas.enabled = false;
            _graphicRaycaster.enabled = false;
        }

        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
        }
    }
}