using Cysharp.Threading.Tasks;

namespace PuzzleGallery.Services.Save.Runtime
{
    public interface ISaveStrategy
    {
        UniTask SaveAsync<T>(string key, T data);
        UniTask<T> LoadAsync<T>(string key, T defaultValue = default);
        UniTask DeleteAsync(string key);
        bool HasKey(string key);
    }
}
