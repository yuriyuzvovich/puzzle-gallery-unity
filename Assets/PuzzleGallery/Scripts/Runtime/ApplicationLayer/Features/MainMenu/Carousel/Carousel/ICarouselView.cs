using System;
using System.Collections.Generic;
using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public interface ICarouselView : IView
    {
        void SetBanners(IReadOnlyList<BannerData> banners);

        void ScrollToIndex(int index, bool animated = true);

        void ScrollToNext();

        int CurrentIndex { get; }

        event Action<int> OnBannerClicked;

        event Action<int> OnBannerChanged;

        float AutoScrollInterval { get; set; }

        bool AutoScrollEnabled { get; set; }
    }
}
