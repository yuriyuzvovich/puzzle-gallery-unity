using PuzzleGallery.Core.EventBus;

namespace PuzzleGallery.Features.Premium
{
    public sealed class SubscriptionSelectedEvent : IEvent
    {
        public SubscriptionType SubscriptionType { get; }
        public string EventName => $"subscription_selected_{SubscriptionType.ToString().ToLower()}";

        public SubscriptionSelectedEvent(SubscriptionType subscriptionType)
        {
            SubscriptionType = subscriptionType;
        }
    }
}
