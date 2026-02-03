using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGallery.Scripts.UI
{
    public sealed class RecyclePool<T> where T : Component
    {
        private readonly Stack<T> _available = new Stack<T>();
        private readonly Func<T> _factory;
        private readonly Transform _parent;

        public int AvailableCount => _available.Count;

        public RecyclePool(Func<T> factory, Transform parent, int initialSize = 0)
        {
            _factory = factory;
            _parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                var item = _factory();
                item.gameObject.SetActive(false);
                _available.Push(item);
            }
        }

        public T Get()
        {
            T item;

            if (_available.Count > 0)
            {
                item = _available.Pop();
            }
            else
            {
                item = _factory();
            }

            item.gameObject.SetActive(true);
            return item;
        }

        public void Return(T item)
        {
            if (item == null)
            {
                return;
            }

            item.gameObject.SetActive(false);
            item.transform.SetParent(_parent);
            _available.Push(item);
        }

        public void Clear()
        {
            while (_available.Count > 0)
            {
                var item = _available.Pop();
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
        }
    }
}
