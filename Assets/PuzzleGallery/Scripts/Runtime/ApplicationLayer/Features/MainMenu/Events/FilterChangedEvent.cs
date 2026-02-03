using PuzzleGallery.Core.EventBus;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class FilterChangedEvent : IEvent
    {
        public string EventName => "filter_changed";
        public FilterType FilterType { get; }

        public FilterChangedEvent(FilterType filterType)
        {
            FilterType = filterType;
        }
    }
}
