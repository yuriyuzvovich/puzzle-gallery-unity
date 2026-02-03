using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public interface IGalleryView : IView
    {
        void Initialize(int itemCount);

        void RefreshData();

        void RefreshWithNewCount(int itemCount, bool forceRecycle = false, bool scrollToTop = false);

        void ScrollToTop();

        void SetColumnCount(int columns);

        UniTask ShowAsync();

        event Action<int> OnItemClicked;

        event Action<int, GalleryItemData> OnConfigureCell;
    }
}
