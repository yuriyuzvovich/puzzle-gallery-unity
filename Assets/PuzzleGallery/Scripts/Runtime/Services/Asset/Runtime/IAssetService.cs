using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.ServiceLocator;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PuzzleGallery.Services.Asset
{
    public interface IAssetService : IService
    {
        UniTask<T> LoadAssetAsync<T>(string key) where T : Object;

        UniTask<T> LoadAssetAsync<T>(AssetReference reference) where T : Object;

        UniTask<GameObject> InstantiateAsync(string key, Transform parent = null);

        UniTask<GameObject> InstantiateAsync(AssetReference reference, Transform parent = null);

        void ReleaseAsset<T>(T asset) where T : Object;

        void ReleaseInstance(GameObject instance);

        UniTask PreloadAssetsAsync(string[] keys);
    }
}
