using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Services.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PuzzleGallery.Services.Asset
{
    public sealed class AddressablesAssetService : IAssetService
    {
        private readonly Dictionary<string, AsyncOperationHandle> _loadedAssets =
            new Dictionary<string, AsyncOperationHandle>();

        public void Initialize()
        {
        }

        public void Dispose()
        {
            foreach (var handle in _loadedAssets.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _loadedAssets.Clear();
        }

        public async UniTask<T> LoadAssetAsync<T>(string key) where T : Object
        {
            if (_loadedAssets.TryGetValue(key, out var cachedHandle))
            {
                if (cachedHandle.IsValid() && cachedHandle.IsDone)
                {
                    return cachedHandle.Result as T;
                }
            }

            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedAssets[key] = handle;
                return handle.Result;
            }

            Logs.Error($"Failed to load asset: {key}");
            return null;
        }

        public async UniTask<T> LoadAssetAsync<T>(AssetReference reference) where T : Object
        {
            if (reference == null || !reference.RuntimeKeyIsValid())
            {
                Logs.Error("Invalid AssetReference");
                return null;
            }

            var handle = reference.LoadAssetAsync<T>();
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Logs.Error($"Failed to load asset from reference: {reference.AssetGUID}");
            return null;
        }

        public async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            var handle = Addressables.InstantiateAsync(key, parent);
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Logs.Error($"Failed to instantiate prefab: {key}");
            return null;
        }

        public async UniTask<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null)
        {
            if (reference == null || !reference.RuntimeKeyIsValid())
            {
                Logs.Error("Invalid AssetReference");
                return null;
            }

            var handle = reference.InstantiateAsync(parent);
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Logs.Error($"Failed to instantiate prefab from reference: {reference.AssetGUID}");
            return null;
        }

        public void ReleaseAsset<T>(T asset) where T : Object
        {
            if (asset == null)
            {
                return;
            }

            Addressables.Release(asset);
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            Addressables.ReleaseInstance(instance);
        }

        public async UniTask PreloadAssetsAsync(string[] keys)
        {
            var tasks = new List<UniTask>();

            foreach (var key in keys)
            {
                tasks.Add(LoadAssetAsync<Object>(key));
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
