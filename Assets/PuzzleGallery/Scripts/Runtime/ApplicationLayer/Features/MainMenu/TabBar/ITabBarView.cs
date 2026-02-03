using System;
using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public interface ITabBarView : IView
    {
        void SelectTab(FilterType filterType);

        event Action<FilterType> OnTabSelected;
    }
}
