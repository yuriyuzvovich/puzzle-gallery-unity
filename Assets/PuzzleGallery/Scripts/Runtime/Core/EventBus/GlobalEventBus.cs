using System;
using System.Collections.Generic;
using PuzzleGallery.Core.ServiceLocator;

namespace PuzzleGallery.Core.EventBus
{
    public sealed class GlobalEventBus : IService
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers =
            new Dictionary<Type, List<Delegate>>();

        public event Action<IEvent> OnEventPublished;

        public void Initialize() { }

        public void Dispose()
        {
            _subscribers.Clear();
            OnEventPublished = null;
        }

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
            {
                _subscribers[type] = new List<Delegate>();
            }

            _subscribers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public void Publish<T>(T evt) where T : IEvent
        {
            OnEventPublished?.Invoke(evt);

            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var handlers))
            {
                return;
            }

            foreach (var handler in handlers.ToArray())
            {
                ((Action<T>)handler)?.Invoke(evt);
            }
        }
    }
}
