using System;
using System.Collections.Generic;
using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class CarouselModel : IModel
    {
        private readonly List<BannerData> _banners = new List<BannerData>();

        public IReadOnlyList<BannerData> Banners => _banners;
        public bool IsEnabled { get; }
        public bool AutoScrollEnabled { get; }
        public float AutoScrollInterval { get; }
        public bool HasBanners => _banners.Count > 0;
        public bool CanAutoScroll => IsEnabled && AutoScrollEnabled && _banners.Count > 1;

        public CarouselModel(CarouselConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            IsEnabled = config.Enabled;
            AutoScrollEnabled = config.AutoScrollEnabled;
            AutoScrollInterval = config.AutoScrollInterval < 0f
                ? 0f
                : config.AutoScrollInterval;

            LoadBanners(config.Banners);
        }

        private void LoadBanners(BannerData[] banners)
        {
            _banners.Clear();

            if (banners == null)
            {
                return;
            }

            for (int i = 0; i < banners.Length; i++)
            {
                var banner = banners[i];
                if (banner != null && !string.IsNullOrWhiteSpace(banner.AssetKey))
                {
                    _banners.Add(banner);
                }
            }
        }
    }
}
