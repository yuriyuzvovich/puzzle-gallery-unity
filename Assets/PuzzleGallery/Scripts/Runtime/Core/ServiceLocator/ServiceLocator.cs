using System;
using System.Collections.Generic;

namespace PuzzleGallery.Core.ServiceLocator
{
    public sealed class ServiceLocator : IServiceRegistry
    {
        private static readonly Lazy<ServiceLocator> _instance = new Lazy<ServiceLocator>(() => new ServiceLocator());

        public static ServiceLocator Instance => _instance.Value;

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly object _lock = new object();

        private ServiceLocator() {}

        // Universal registration (accepts both IService and config types)
        public void Register<T>(T item) where T: class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_services.ContainsKey(type))
                {
                    throw new InvalidOperationException($"Item {type.Name} is already registered");
                }

                _services[type] = item;

                // Call Initialize() if item implements IService
                if (item is IService service)
                {
                    service.Initialize();
                }
            }
        }

        public T Get<T>() where T: class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_services.TryGetValue(type, out var item))
                {
                    throw new InvalidOperationException($"Item {type.Name} is not registered");
                }

                return (T) item;
            }
        }

        public bool TryGet<T>(out T item) where T: class
        {
            lock (_lock)
            {
                if (_services.TryGetValue(typeof(T), out var obj))
                {
                    item = (T) obj;
                    return true;
                }
                item = null;
                return false;
            }
        }

        public void Unregister<T>() where T: class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_services.TryGetValue(type, out var item))
                {
                    // Call Dispose() if item implements IService
                    if (item is IService service)
                    {
                        service.Dispose();
                    }
                    _services.Remove(type);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                foreach (var item in _services.Values)
                {
                    // Call Dispose() if item implements IService
                    if (item is IService service)
                    {
                        service.Dispose();
                    }
                }
                _services.Clear();
            }
        }
    }
}