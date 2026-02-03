using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.ServiceLocator;

namespace PuzzleGallery.Services.Save.Runtime
{
    public interface ILocalStorageService : IService
    {
        UniTask SaveAsync<T>(string key, T data);
        UniTask<T> LoadAsync<T>(string key, T defaultValue = default);
        UniTask DeleteAsync(string key);
        bool HasKey(string key);
    }
}
