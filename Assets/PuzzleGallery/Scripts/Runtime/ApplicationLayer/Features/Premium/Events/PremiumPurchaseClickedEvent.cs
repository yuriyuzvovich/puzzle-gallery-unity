using PuzzleGallery.Core.EventBus;

namespace PuzzleGallery.Features.Premium
{
    public sealed class PremiumPurchaseClickedEvent : IEvent
    {
        public string EventName => "premium_purchase_clicked";
    }
}
