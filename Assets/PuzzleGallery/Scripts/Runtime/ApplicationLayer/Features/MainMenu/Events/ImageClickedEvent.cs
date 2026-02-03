using PuzzleGallery.Core.EventBus;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class ImageClickedEvent : IEvent
    {
        public string EventName => "image_clicked";
        public GalleryItemData ImageData { get; }

        public ImageClickedEvent(GalleryItemData imageData)
        {
            ImageData = imageData;
        }
    }
}
