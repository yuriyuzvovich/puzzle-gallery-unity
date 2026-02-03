using System.Collections.Generic;
using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class GalleryModel : IModel
    {
        private readonly List<GalleryItemData> _images = new List<GalleryItemData>();

        public IReadOnlyList<GalleryItemData> AllImages => _images;

        public void LoadImages(string baseUrl, int totalCount, int premiumInterval)
        {
            _images.Clear();

            for (int i = 1; i <= totalCount; i++)
            {
                _images.Add(new GalleryItemData
                {
                    Index = i,
                    Url = $"{baseUrl}{i}.jpg",
                    IsPremium = (i % premiumInterval == 0)
                });
            }
        }
    }
}
