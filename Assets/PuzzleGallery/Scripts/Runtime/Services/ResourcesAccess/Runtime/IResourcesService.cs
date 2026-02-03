using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.ServiceLocator;
using UnityEngine;

namespace PuzzleGallery.Services.ResourcesAccess
{
    public interface IResourcesService : IService
    {
        T Load<T>(string path) where T : Object;

        UniTask<T> LoadAsync<T>(string path) where T : Object;

        GameObject Instantiate(string path, Transform parent = null);

        UniTask<GameObject> InstantiateAsync(string path, Transform parent = null);

        void Unload(Object asset);
    }
}
