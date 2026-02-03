using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Features.Premium;
using PuzzleGallery.Services.Asset;
using PuzzleGallery.Services.Logging;
using PuzzleGallery.Services.ResourcesAccess;
using UnityEngine;

namespace PuzzleGallery.Services.Popup
{
    public sealed class PopupService : IPopupService
    {
        private const string POPUP_CANVAS_PATH = "Popups/PopupCanvas";
        
        private readonly IAssetService _assetService;
        private readonly IResourcesService _resourcesService;
        private readonly PremiumPopupConfig _config;
        private readonly Dictionary<Type, string> _popupAddresses;
        private readonly Dictionary<Type, IPopup> _loadedPopups = new Dictionary<Type, IPopup>();
        private readonly Stack<IPopup> _popupStack = new Stack<IPopup>();
        private Transform _popupContainer;

        public IPopup CurrentPopup => _popupStack.Count > 0 ? _popupStack.Peek() : null;

        public PopupService(IAssetService assetService, IResourcesService resourcesService, PremiumPopupConfig config)
        {
            _assetService = assetService;
            _resourcesService = resourcesService;
            _config = config;

            _popupAddresses = new Dictionary<Type, string>
            {
            };
        }

        public void Initialize()
        {
            var canvas = GameObject.Find("PopupCanvas");
            if (canvas == null)
            {
                canvas = _resourcesService.Instantiate(POPUP_CANVAS_PATH);
                if (canvas == null)
                {
                    Logs.Error($"Failed to load PopupCanvas from Resources/{POPUP_CANVAS_PATH}");
                    return;
                }

                GameObject.DontDestroyOnLoad(canvas);
            }
            _popupContainer = canvas.transform;
        }

        public void Dispose()
        {
            HideAllAsync().Forget();

            foreach (var popup in _loadedPopups.Values)
            {
                if (popup is MonoBehaviour mb && mb != null)
                {
                    GameObject.Destroy(mb.gameObject);
                }
            }
            _loadedPopups.Clear();
        }

        public async UniTask<T> ShowAsync<T>(PopupData data = null) where T : class, IPopup
        {
            var type = typeof(T);

            if (!_loadedPopups.TryGetValue(type, out var popup))
            {
                if (!_popupAddresses.TryGetValue(type, out var address))
                {
                    Logs.Error($"Popup address not registered for type: {type.Name}");
                    return null;
                }

                var go = await _assetService.InstantiateAsync(address, _popupContainer);
                if (go == null)
                {
                    Logs.Error($"Failed to instantiate popup: {type.Name}");
                    return null;
                }

                popup = go.GetComponent<IPopup>();
                if (popup == null)
                {
                    Logs.Error($"GameObject does not have IPopup component: {type.Name}");
                    GameObject.Destroy(go);
                    return null;
                }

                _loadedPopups[type] = popup;
            }

            popup.SetData(data);
            await popup.ShowAsync();
            _popupStack.Push(popup);

            return popup as T;
        }

        public async UniTask HideAsync<T>() where T : class, IPopup
        {
            var type = typeof(T);

            if (_loadedPopups.TryGetValue(type, out var popup) && popup.IsVisible)
            {
                await popup.HideAsync();

                var tempStack = new Stack<IPopup>();
                while (_popupStack.Count > 0)
                {
                    var p = _popupStack.Pop();
                    if (p != popup)
                    {
                        tempStack.Push(p);
                    }
                }
                while (tempStack.Count > 0)
                {
                    _popupStack.Push(tempStack.Pop());
                }
            }
        }

        public async UniTask HideAllAsync()
        {
            while (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                if (popup.IsVisible)
                {
                    await popup.HideAsync();
                }
            }
        }

        public bool IsPopupShown<T>() where T : class, IPopup
        {
            return _loadedPopups.TryGetValue(typeof(T), out var popup) && popup.IsVisible;
        }

        public void RegisterPopup<T>(string address) where T : class, IPopup
        {
            _popupAddresses[typeof(T)] = address;
        }
    }
}
