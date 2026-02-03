using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PuzzleGallery.Features.MainMenu.Cell;
using PuzzleGallery.Scripts.UI;
using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    public class GalleryView : MonoBehaviour, IGalleryView
    {
        [Header("References")]
        [SerializeField] private VirtualizedScrollRect _scrollRect;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private bool _isFirstShow = true;
        private Tween _fadeTween;

        public bool IsVisible => gameObject.activeSelf;

        public event Action<int> OnItemClicked;
        public event Action<int, GalleryItemData> OnConfigureCell;

        private void Awake()
        {
            if (_scrollRect != null)
            {
                _scrollRect.OnCellClicked += HandleCellClicked;
                _scrollRect.OnConfigureCell += HandleConfigureCell;
            }
        }

        private void OnDestroy()
        {
            if (_scrollRect != null)
            {
                _scrollRect.OnCellClicked -= HandleCellClicked;
                _scrollRect.OnConfigureCell -= HandleConfigureCell;
            }

            _fadeTween?.Kill();
        }

        public void Initialize(int itemCount)
        {
            _scrollRect.Initialize(itemCount);
        }

        public void RefreshData()
        {
            _scrollRect.RefreshData();
        }

        public void RefreshWithNewCount(int itemCount, bool forceRecycle = false, bool scrollToTop = false)
        {
            _scrollRect.RefreshWithNewCount(itemCount, forceRecycle, scrollToTop);
        }

        public void ScrollToTop()
        {
            _scrollRect.ScrollToTop();
        }

        public void SetColumnCount(int columns)
        {
            _scrollRect.SetColumnCount(columns);
        }

        public async UniTask ShowAsync()
        {
            gameObject.SetActive(true);

            if (_isFirstShow && _canvasGroup != null)
            {
                _isFirstShow = false;

                _fadeTween?.Kill();

                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;

                _fadeTween = TweenPresets.FadeIn(_canvasGroup, TweenPresets.SlowDuration);
                await _fadeTween.AsyncWaitForCompletion().AsUniTask();

                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleCellClicked(int index)
        {
            OnItemClicked?.Invoke(index);
        }

        private void HandleConfigureCell(VirtualizedCell cell, int index)
        {
            if (cell is GalleryItemCellView galleryCell)
            {
                var data = new GalleryItemData();
                OnConfigureCell?.Invoke(index, data);
                galleryCell.Configure(data);
            }
        }
    }
}
