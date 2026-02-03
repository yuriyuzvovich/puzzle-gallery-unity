using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.EasingCore;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class InfiniteCarouselScrollView : FancyScrollView<BannerData, CarouselContext>
    {
        [SerializeField] private Scroller _scroller;
        [SerializeField] private GameObject _cellPrefab;

        [Header("Scroll Settings")]
        [SerializeField] private float _scrollDuration = 0.25f;
        [SerializeField, Range(0.2f, 1f)] private float _cellInterval = 0.3f;

        protected override GameObject CellPrefab => _cellPrefab;

        public int CurrentIndex => Context.SelectedIndex >= 0
            ? Context.SelectedIndex % Mathf.Max(1, ItemsSource.Count)
            : 0;

        public event Action<int> OnSelectionChanged;

        public void SetBannerClickCallback(Action<int> callback)
        {
            Context.OnBannerClicked = callback;
        }

        protected override void Initialize()
        {
            base.Initialize();

            loop = true;

            cellInterval = _cellInterval;

            Context.CellInterval = _cellInterval;

            scrollOffset = 0.5f;

            if (_scroller != null)
            {
                _scroller.MovementType = MovementType.Unrestricted;

                _scroller.OnValueChanged(UpdatePosition);
                _scroller.OnSelectionChanged(HandleSelectionChanged);
            }
        }

        public void SetBanners(IReadOnlyList<BannerData> banners)
        {
            var items = banners as IList<BannerData> ?? new List<BannerData>(banners);
            UpdateContents(items);

            if (_scroller != null)
            {
                _scroller.SetTotalCount(items.Count);
            }

            if (items.Count > 0)
            {
                Context.SelectedIndex = 0;
                Refresh();
            }
        }

        public void ScrollToIndex(int index, bool animated = true)
        {
            if (_scroller == null || ItemsSource.Count == 0)
            {
                return;
            }

            index = Mathf.Clamp(index, 0, ItemsSource.Count - 1);

            if (animated)
            {
                _scroller.ScrollTo(index, _scrollDuration, Ease.OutCubic);
            }
            else
            {
                _scroller.JumpTo(index);
            }

            HandleSelectionChanged(index);
        }

        public void ScrollToNext()
        {
            if (_scroller == null || ItemsSource.Count == 0)
            {
                return;
            }

            float nextPosition = _scroller.Position + 1f;
            _scroller.ScrollTo(nextPosition, _scrollDuration, Ease.OutCubic);
        }

        public void ScrollToPrevious()
        {
            if (_scroller == null || ItemsSource.Count == 0)
            {
                return;
            }

            float prevPosition = _scroller.Position - 1f;
            _scroller.ScrollTo(prevPosition, _scrollDuration, Ease.OutCubic);
        }

        private void HandleSelectionChanged(int index)
        {
            if (ItemsSource.Count == 0)
            {
                return;
            }

            int normalizedIndex = index >= 0
                ? index % ItemsSource.Count
                : ItemsSource.Count - 1 + (index + 1) % ItemsSource.Count;

            if (Context.SelectedIndex == normalizedIndex)
            {
                return;
            }

            Context.SelectedIndex = normalizedIndex;
            Refresh();
            OnSelectionChanged?.Invoke(normalizedIndex);
        }
    }
}
