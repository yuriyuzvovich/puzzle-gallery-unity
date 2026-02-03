using Cysharp.Threading.Tasks;
using PuzzleGallery.Services.Logging;
using UnityEngine;

namespace PuzzleGallery.Services.ResourcesAccess
{
    public sealed class ResourcesService : IResourcesService
    {
        public void Initialize()
        {
            Logs.Info("ResourcesService initialized");
        }

        public void Dispose()
        {
        }

        public T Load<T>(string path) where T : Object
        {
            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                Logs.Warning($"Failed to load resource: {path}");
            }
            return asset;
        }

        public async UniTask<T> LoadAsync<T>(string path) where T : Object
        {
            var request = Resources.LoadAsync<T>(path);
            await request.ToUniTask();

            var asset = request.asset as T;
            if (asset == null)
            {
                Logs.Warning($"Failed to load resource async: {path}");
            }
            return asset;
        }

        public GameObject Instantiate(string path, Transform parent = null)
        {
            var prefab = Load<GameObject>(path);
            if (prefab == null)
            {
                return null;
            }

            return Object.Instantiate(prefab, parent);
        }

        public async UniTask<GameObject> InstantiateAsync(string path, Transform parent = null)
        {
            var prefab = await LoadAsync<GameObject>(path);
            if (prefab == null)
            {
                return null;
            }

            return Object.Instantiate(prefab, parent);
        }

        public void Unload(Object asset)
        {
            if (asset != null)
            {
                Resources.UnloadAsset(asset);
            }
        }
    }
}
