using System;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class CarouselContext
    {
        public int SelectedIndex { get; set; } = -1;

        public Action<int> OnBannerClicked { get; set; }

        public float CellInterval { get; set; } = 1f;
    }
}
